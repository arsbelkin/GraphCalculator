using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GraphCalculatorAvalonia.Views;

public enum SaveChangesChoice
{
    Cancel,
    Save,
    Discard,
}

public partial class SaveChangesWindow : Window
{
    public SaveChangesWindow() => InitializeComponent();

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(SaveChangesChoice.Cancel);
    private void OnSaveClick(object? sender, RoutedEventArgs e) => Close(SaveChangesChoice.Save);
    private void OnDiscardClick(object? sender, RoutedEventArgs e) => Close(SaveChangesChoice.Discard);
}
