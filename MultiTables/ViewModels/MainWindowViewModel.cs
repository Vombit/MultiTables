using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Media;
using Avalonia.Threading;
using MultiTables.Models;
using ReactiveUI;

namespace MultiTables.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Element, Unit> AddNewElement { get; }
    private ObservableCollection<ListElements> _stackElements = new();

    public ObservableCollection<ListElements> StackElements
    {
        get => _stackElements;
        set => this.RaiseAndSetIfChanged(ref _stackElements, value);
    }
    
    private int _lastStateHash = 0;

    private readonly PreviewViewModel _previewViewModel;
    public PreviewViewModel PreviewViewModel => _previewViewModel;

    private int _height = 25;

    public int Height
    {
        get => _height;
        set
        {
            this.RaiseAndSetIfChanged(ref _height, value);
            foreach (var list in _stackElements)
            {
                list.Height = Height;
            }
        }
    }

    private readonly DispatcherTimer _updateTimer;
    private bool _isWindowActive;
    private List<string> _fontFamilies;

    public bool IsWindowActive
    {
        get => _isWindowActive;
        set => this.RaiseAndSetIfChanged(ref _isWindowActive, value);
    }

    private Section _activeSection = new Section();
    
    public Section ActiveSection
    {
        get => _activeSection;
        set => this.RaiseAndSetIfChanged(ref _activeSection, value);
    }
    public List<string> FontFamilies
    {
        get => _fontFamilies;
        set => this.RaiseAndSetIfChanged(ref _fontFamilies, value);
    }
    
    private void UpdatePreview()
    {
        _previewViewModel.Height = Height;
        _previewViewModel.StackElements = StackElements;
    }

    public MainWindowViewModel(PreviewViewModel previewViewModel)
    {
        _previewViewModel = previewViewModel;
        CreateElement();
        FontFamilies = FontManager.Current.SystemFonts
            .OrderBy(f => f.Name)
            .Select(f => f.Name)
            .ToList();
        
        AddNewElement = ReactiveCommand.Create<Element>(_ => { CreateElement();  });
        
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        _updateTimer.Tick += (_, _) =>
        {
            // Console.WriteLine(ActiveSection.FontFamily);
            if (!IsWindowActive)
                return;
            var currentHash = GetCurrentStateHash();
            if (currentHash != _lastStateHash)
            {
                _lastStateHash = currentHash;
                UpdatePreview();
            }
        };
        _updateTimer.Start();
    }

    private void CreateElement()
    {
        var section = new Element().Sections;
        section.Add(new Section() { Text = "1" });

        var el = new ObservableCollection<Element>();
        var element = new Element() { Sections = section };
        element.RequestRemoveSelf += RemoveElement;
        el.Add(element);
        var list = new ListElements() { ElementsList = el };
        list.Height = Height;
        StackElements.Add(list);
    }

        private int GetCurrentStateHash()
        {
            // очень невыгодная функция, пересмотреть реализацию обновления превью
            var hash = new HashCode();

            foreach (var list in StackElements)
            {
                hash.Add(list.Height);
                hash.Add(list.Width);
                foreach (var el in list.ElementsList)
                {
                    foreach (var section in el.Sections)
                    {
                        hash.Add(section.Text);
                        hash.Add(section.FontSize);
                        hash.Add(section.FontFamily);
                    }
                }
            }

            return hash.ToHashCode();
        }
    private void RemoveElement(Element element)
    {
        foreach (var list in _stackElements)
        {
            if (!list.ElementsList.Contains(element)) continue;
            list.ElementsList.Remove(element);
            if (list.ElementsList.Count == 0)
            {
                _stackElements.Remove(list);
            }
            break;
        }
    }
}