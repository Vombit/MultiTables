using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Media;
using ReactiveUI;

namespace MultiTables.Models;


public class ListElements: ReactiveObject
{
    private const double VisualScale = 4.5;
    
    private ObservableCollection<Element> _elementsList = new();
    public ReactiveCommand<Unit, Unit> AddRowCommand { get; }
    public ObservableCollection<Element> ElementsList
    {
        get => _elementsList;
        set => this.RaiseAndSetIfChanged(ref _elementsList, value);
    }
    
    private int _width = 50;
    private int _visualWidth = 261;
    public int Width
    {
        get => _width;
        set
        {
            this.RaiseAndSetIfChanged(ref _width, value);
            const int marginElement = 36;
            VisualWidth = (int)(Width * VisualScale) + marginElement;
        }
    }
    public int VisualWidth
    {
        get => _visualWidth;
        set => this.RaiseAndSetIfChanged(ref _visualWidth, value);
    }
    
    private int _height;
    public int Height
    {
        get => _height;
        set
        {
            this.RaiseAndSetIfChanged(ref _height, value);
            VisualHeight = (int)(Height * VisualScale);
            Console.WriteLine(VisualHeight);
            Console.WriteLine(VisualHeight / _elementsList.Count);
            Console.WriteLine();
            foreach (var element in _elementsList)
            {
                element.VisualHeight = VisualHeight / _elementsList.Count - 2;
            }
        }
    }
    private int _visualHeight;
    public int VisualHeight
    {
        get => _visualHeight;
        set => this.RaiseAndSetIfChanged(ref _visualHeight, value);
    }

    public ListElements(Action<Element> removeElement)
    {
        const int maxElements = 3;
        
        AddRowCommand = ReactiveCommand.Create(() =>
        {
            if (_elementsList.Count >= maxElements) return;
            
            const string defaultText = "1";
            
            var newElement = new Element();
            newElement.RequestRemoveSelf += removeElement;
            newElement.Sections.Add(new Section { Text = defaultText });

            _elementsList.Add(newElement);
            RecalculationSizes();

        });
    }
    private void RecalculationSizes()
    {
        foreach (var element in _elementsList)
        {
            element.VisualHeight = VisualHeight / _elementsList.Count - 2;
        }
    }

}

public class Element: ReactiveObject
{
    private ObservableCollection<Section> _sections = new();
    public event Action<Element>? RequestRemoveSelf;
    public ObservableCollection<Section> Sections
    {
        get => _sections;
        set => this.RaiseAndSetIfChanged(ref _sections, value);
    }
    
    private readonly ListElements _elements;

    public Element()
    {
        
    }
    
    public int MainWidth { get; set; }
    
    private int _visualHeight;
    public int VisualHeight
    {
        get => _visualHeight;
        set => this.RaiseAndSetIfChanged(ref _visualHeight, value);
    }

    private int _sectionsCount = 1;
    public int SectionsCount
    {
        get => _sectionsCount;
        set
        {
            if (_sectionsCount == value) return;
            this.RaiseAndSetIfChanged(ref _sectionsCount, value);
            UpdateSections(_sectionsCount);
        }
    }
    private void UpdateSections(int targetCount)
    {
        while (Sections.Count < targetCount) 
            Sections.Add(new Section { Text = (Sections.Count + 1).ToString() });

        while (Sections.Count > targetCount) 
            Sections.RemoveAt(Sections.Count - 1);

        if (Sections.Count == 0) 
            RequestRemoveSelf?.Invoke(this);
    }
}


public class Section : ReactiveObject
{
    private string? _text;
    private string _fontFamily = "Arial";
    private FontFamily _visualFontFamily = new FontFamily("Arial");
    private double _fontSize = 14;
    public string? Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontFamily, value);
            VisualFontFamily = new FontFamily(value);

        }
}

    public FontFamily VisualFontFamily
    {
        get => _visualFontFamily;
        set => this.RaiseAndSetIfChanged(ref _visualFontFamily, value);
    }
    public double FontSize
    {
        get => _fontSize;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontSize, value);
        }
    }

    private double _visualHeight;
    public double VisualHeight
    {
        get => _visualHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _visualHeight, value);
        }
    }
}