using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MultiTables.Models;
using MultiTables.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using System.IO;
using System.Linq;

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
        
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
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
    
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Проверяем что перетаскиваются файлы
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files))
            return;

        var files = e.Data.GetFiles();
        var file = files?.FirstOrDefault();
    
        if (file == null)
            return;

        var path = file.Path.LocalPath;
        var ext = Path.GetExtension(path).ToLower();
    
        // Проверяем что это изображение
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".bmp" && ext != ".gif")
            return;

        // Находим Section над которым произошел drop
        var position = e.GetPosition(this);
        var visual = this.GetVisualAt(position);
    
        Section? targetSection = null;
        Control? current = visual as Control;
    
        // Идем вверх по визуальному дереву пока не найдем Border с DataContext = Section
        while (current != null)
        {
            if (current.DataContext is Section section)
            {
                targetSection = section;
                break;
            }
            current = current.Parent as Control;
        }
    
        if (targetSection != null)
        {
            targetSection.ImagePath = path;
            targetSection.Text = null;
            // Console.WriteLine(path);
        }
    
        e.Handled = true;
    }

    private void OnSectionPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Section section)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ActiveSection = section;
            }
            e.Handled = true;
        }
    }
}