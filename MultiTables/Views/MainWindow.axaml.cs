using Avalonia.Controls;
using Avalonia.Input;
using MultiTables.Models;
using MultiTables.ViewModels;

namespace MultiTables.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // события активности окна
        this.Activated += (_, _) => SetWindowActive(true);
        this.Deactivated += (_, _) => SetWindowActive(false);
        this.Opened += (_, _) => SetWindowActive(true);
        this.Closed += (_, _) => SetWindowActive(false);
    }

    private void SetWindowActive(bool isActive)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.IsWindowActive = isActive;
    }

    private void OnSectionGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is Section section)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ActiveSection = section;
            }
        }
    }
}