using Avalonia.Controls;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GraphCalculatorAvalonia.Services;
using GraphCalculatorAvalonia.ViewModels;

namespace GraphCalculatorAvalonia.Views;

public partial class MainWindow : Window
{
    private readonly FunctionsDocumentState documentState = new();
    private IStorageFile? currentFile;
    private bool isFileOperationInProgress;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        Closed += (_, _) => currentFile?.Dispose();
    }

    private async void OnSaveFunctionsClick(object? sender, RoutedEventArgs e) =>
        await RunFileOperationAsync(async model => { await SaveDocumentAsync(model); });

    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.S || (e.KeyModifiers != KeyModifiers.Control &&
                              e.KeyModifiers != (KeyModifiers.Control | KeyModifiers.Shift)))
            return;

        e.Handled = true;
        bool saveAs = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        await RunFileOperationAsync(async model => { await SaveDocumentAsync(model, saveAs); });
    }

    private async Task<bool> SaveDocumentAsync(MainViewModel model, bool saveAs = false)
    {
        IStorageFile? target = currentFile;
        bool selectedNewFile = false;
        try
        {
            var snapshot = FunctionsDocumentState.Capture(model.Functions);
            if (target is null || saveAs)
            {
                target = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить функции",
                    SuggestedFileName = currentFile?.Name ?? "functions.json",
                    DefaultExtension = "json",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("Функции JSON") { Patterns = new[] { "*.json" } },
                    },
                });
                if (target is null)
                    return false;
                selectedNewFile = true;
            }

            await using (var stream = await target.OpenWriteAsync())
            {
                if (stream.CanSeek)
                    stream.SetLength(0);
                await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                await writer.WriteAsync(snapshot.Json);
                await writer.FlushAsync();
            }

            if (selectedNewFile)
            {
                currentFile?.Dispose();
                currentFile = target;
            }
            
            documentState.MarkSaved(snapshot);
            UpdateFileTitle();
            SetFileStatus($"Сохранено: {target.Name}", false);
            return true;
        }
        catch (Exception)
        {
            SetFileStatus("Не удалось сохранить файл. Изменения остались в документе. Проверьте путь и права доступа.", true);
            return false;
        }
        finally
        {
            if (selectedNewFile && !ReferenceEquals(target, currentFile))
                target?.Dispose();
        }
    }

    private async void OnLoadFunctionsClick(object? sender, RoutedEventArgs e) =>
        await RunFileOperationAsync(LoadDocumentAsync);

    private async Task LoadDocumentAsync(MainViewModel model)
    {
        if (!await ConfirmLoadAsync(model))
            return;

        var loaded = await new LoadFunctionsWindow().ShowDialog<LoadedFunctionsResult?>(this);
        if (loaded is null)
            return;

        try
        {
            var snapshot = FunctionsDocumentState.Capture(loaded.Functions);
            model.ReplaceFunctions(loaded.Functions);
            currentFile?.Dispose();
            currentFile = loaded.File;
            documentState.MarkSaved(snapshot);
            UpdateFileTitle();
            SetFileStatus($"Загружено функций: {loaded.Functions.Count}.", false);
        }
        finally
        {
            if (!ReferenceEquals(loaded.File, currentFile))
                loaded.File.Dispose();
        }
    }

    private async Task<bool> ConfirmLoadAsync(MainViewModel model)
    {
        if (!documentState.RequiresSavePrompt(FunctionsDocumentState.Capture(model.Functions)))
            return true;

        var choice = await new SaveChangesWindow().ShowDialog<SaveChangesChoice>(this);
        return choice switch
        {
            SaveChangesChoice.Save => await SaveDocumentAsync(model),
            SaveChangesChoice.Discard => true,
            _ => false,
        };
    }

    private async Task RunFileOperationAsync(Func<MainViewModel, Task> operation)
    {
        if (isFileOperationInProgress || DataContext is not MainViewModel model)
            return;
        isFileOperationInProgress = true;
        SaveFunctionsButton.IsEnabled = false;
        LoadFunctionsButton.IsEnabled = false;
        try
        {
            await operation(model);
        }
        catch (Exception)
        {
            SetFileStatus("Не удалось выполнить операцию с файлом.", true);
        }
        finally
        {
            isFileOperationInProgress = false;
            SaveFunctionsButton.IsEnabled = true;
            LoadFunctionsButton.IsEnabled = true;
        }
    }

    private void UpdateFileTitle()
    {
        Title = currentFile is null ? "Графический калькулятор" : $"{currentFile.Name} — Графический калькулятор";
        ToolTip.SetTip(SaveFunctionsButton, currentFile is null
            ? "Сохранить функции (Ctrl+S)"
            : $"Сохранить (Ctrl+S)\n{currentFile.Path}\nСохранить как… (Ctrl+Shift+S)");
    }

    private void SetFileStatus(string message, bool isError)
    {
        FileStatus.Text = message;
        FileStatus.Foreground = Brush.Parse(isError ? "#DC2626" : "#64748B");
        FileStatus.IsVisible = true;
    }
}
