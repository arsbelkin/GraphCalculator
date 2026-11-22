using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GraphCalculatorAvalonia.Services;
using System.Collections.Generic;
using GraphCalculatorAvalonia.ViewModels;

namespace GraphCalculatorAvalonia.Views;

public sealed record LoadedFunctionsResult(IStorageFile File, IReadOnlyList<FunctionViewModel> Functions);

public partial class LoadFunctionsWindow : Window
{
    private bool isLoading;
    private bool isClosed;

    public LoadFunctionsWindow()
    {
        InitializeComponent();
        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropZone.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop);
        Closed += (_, _) => isClosed = true;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().ToArray();
        bool accepted = !isLoading && files?.Length == 1 && IsJson(files[0]);
        e.DragEffects = accepted ? DragDropEffects.Copy : DragDropEffects.None;
        DropZone.Classes.Set("dragover", accepted);
        e.Handled = true;
    }

    private void OnDragLeave(object? sender, DragEventArgs e) => DropZone.Classes.Remove("dragover");

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;
        DropZone.Classes.Remove("dragover");
        var files = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().ToArray();
        if (files?.Length == 1 && IsJson(files[0]))
            await LoadFileAsync(files[0]);
        else
            ShowError("Перетащите один файл с расширением .json.");
    }

    private async void OnChooseFileClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выбрать файл функций",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Функции JSON") { Patterns = new[] { "*.json" } } },
            });
            if (files.Count == 1)
                await LoadFileAsync(files[0]);
        }
        catch (Exception)
        {
            ShowError("Не удалось открыть диалог выбора файла.");
        }
    }

    private async Task LoadFileAsync(IStorageFile file)
    {
        if (isLoading)
            return;
        isLoading = true;
        ChooseFileButton.IsEnabled = false;
        LoadError.IsVisible = false;
        bool fileTransferred = false;
        try
        {
            if (!IsJson(file))
                throw new InvalidDataException("Выберите файл с расширением .json.");
            IReadOnlyList<FunctionViewModel> functions;
            await using (var stream = await file.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                functions = FunctionsFileService.Deserialize(json);
            }
            if (!isClosed)
            {
                Close(new LoadedFunctionsResult(file, functions));
                fileTransferred = true;
            }
        }
        catch (JsonException)
        {
            ShowError("Не удалось прочитать JSON. Проверьте формат файла функций.");
        }
        catch (InvalidDataException error)
        {
            ShowError(error.Message);
        }
        catch (Exception)
        {
            ShowError("Не удалось загрузить файл. Проверьте, что он доступен для чтения.");
        }
        finally
        {
            if (!fileTransferred)
                file.Dispose();
            isLoading = false;
            ChooseFileButton.IsEnabled = true;
        }
    }

    private static bool IsJson(IStorageFile file) =>
        string.Equals(Path.GetExtension(file.Name), ".json", StringComparison.OrdinalIgnoreCase);

    private void ShowError(string message)
    {
        LoadError.Text = message;
        LoadError.IsVisible = true;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close();
}
