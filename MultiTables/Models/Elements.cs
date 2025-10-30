using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace MultiTables.Models;

public class ListElements : ReactiveObject
{
    private const double VisualScale = 4.5;

    private ObservableCollection<Element> _elementsList = new();
    public ReactiveCommand<Unit, Unit> AddRowCommand { get; }

    public ObservableCollection<Element> ElementsList
    {
        get => _elementsList;
        set
        {
            if (_elementsList == value) return;
            UnsubscribeCollectionEvents(_elementsList);
            this.RaiseAndSetIfChanged(ref _elementsList, value ?? new ObservableCollection<Element>());
            SubscribeCollectionEvents(_elementsList);
            // пересчитать сразу при замене коллекции
            RecalculationSizes();
        }
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

            RecalculationSizes();
        }
    }
    private int _visualHeight;
    public int VisualHeight
    {
        get => _visualHeight;
        set => this.RaiseAndSetIfChanged(ref _visualHeight, value);
    }

    public ListElements()
    {
        const int maxElements = 3;

        // Подписываемся на изменение коллекции
        SubscribeCollectionEvents(_elementsList);

        AddRowCommand = ReactiveCommand.Create(() =>
        {
            if (_elementsList.Count >= maxElements) return;

            const string defaultText = "1";

            var newElement = new Element();
            newElement.RequestRemoveSelf += RemoveElement;
            newElement.Sections.Add(new Section { Text = defaultText });

            _elementsList.Add(newElement);
            // RecalculationSizes будет вызван обработчиком CollectionChanged,
            // но можно вызвать и явно:
            RecalculationSizes();
        });
    }

    private void SubscribeCollectionEvents(ObservableCollection<Element> collection)
    {
        if (collection == null) return;
        collection.CollectionChanged += ElementsList_CollectionChanged;
        // Убедимся что все элементы подписаны на RequestRemoveSelf
        foreach (var el in collection)
        {
            el.RequestRemoveSelf -= RemoveElement; // на всякий
            el.RequestRemoveSelf += RemoveElement;
        }
    }

    private void UnsubscribeCollectionEvents(ObservableCollection<Element> collection)
    {
        if (collection == null) return;
        collection.CollectionChanged -= ElementsList_CollectionChanged;
        foreach (var el in collection)
            el.RequestRemoveSelf -= RemoveElement;
    }

    private void ElementsList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Подпишем новых и отпишем удалённых
        if (e.NewItems != null)
        {
            foreach (Element n in e.NewItems)
            {
                n.RequestRemoveSelf -= RemoveElement;
                n.RequestRemoveSelf += RemoveElement;
            }
        }
        if (e.OldItems != null)
        {
            foreach (Element o in e.OldItems)
            {
                o.RequestRemoveSelf -= RemoveElement;
            }
        }

        RecalculationSizes();
    }

    private void RemoveElement(Element element)
    {
        // вызывается когда Element говорит "удалите меня"
        if (element == null) return;
        if (_elementsList.Contains(element))
        {
            _elementsList.Remove(element);
        }
        RecalculationSizes();
    }

    public void RecalculationSizes()
    {
        var count = _elementsList?.Count ?? 0;
        if (count == 0) return; // нечего пересчитывать

        // целочисленное деление — оставлено, но убедимся что не отрицательное
        var per = Math.Max(0, VisualHeight / count - 2);
        foreach (var element in _elementsList)
        {
            element.VisualHeight = per;
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
    
    private int _visualHeight;
    public int VisualHeight
    {
        get => _visualHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _visualHeight, value);
        }
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
    private string? _imagePath;
    private Bitmap? _imageBitmap;
    private string _fontFamily = "Arial";
    private FontFamily _visualFontFamily = new FontFamily("Arial");
    private double _fontSize = 14;
    private bool _isActive;
    public string? Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    
    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _imagePath, value);
            
            // Загружаем bitmap при установке пути
            if (!string.IsNullOrEmpty(value) && File.Exists(value))
            {
                try
                {
                    ImageBitmap = new Bitmap(value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                    ImageBitmap = null;
                }
            }
            else
            {
                ImageBitmap = null;
            }
        }
    }

    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        private set => this.RaiseAndSetIfChanged(ref _imageBitmap, value);
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
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                this.RaiseAndSetIfChanged(ref _isActive, value);
            }
        }
    }
}