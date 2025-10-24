using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using MultiTables.Services;
using MultiTables.Models;
using ReactiveUI;

namespace MultiTables.ViewModels;

public class PreviewViewModel : ViewModelBase
{
    private readonly IImageGeneratorService _imageGenerator;

    private Bitmap _generatedImage;
    public Bitmap GeneratedImage
    {
        get => _generatedImage;
        set => this.RaiseAndSetIfChanged(ref _generatedImage, value);
    }

    private int _height = 25;
    public int Height
    {
        get => _height;
        set
        {
            this.RaiseAndSetIfChanged(ref _height, value);
            VisualHeight = (int)(value * 4.5);
        }
    }
    private int _visualHeight = 112;
    public int VisualHeight
    {
        get => _visualHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _visualHeight, value);

            VisualWidth = 0;
            foreach (var list in StackElements)
            {
                VisualWidth += list.Width;
            }
            VisualWidth = (int)(VisualWidth * 4.5);
        }
    }
    
    private int _visualWidth = 112;
    public int VisualWidth
    {
        get => _visualWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _visualWidth, value);
            UpdateImage();
        }
    }
    
    
    private ObservableCollection<ListElements> _stackElements = new();
    public ObservableCollection<ListElements> StackElements
    {
        get => _stackElements;
        set => this.RaiseAndSetIfChanged(ref _stackElements, value);
    }

    
    public PreviewViewModel(IImageGeneratorService imageGenerator)
    {
        _imageGenerator = imageGenerator;
        UpdateImage();
    }

    private void UpdateImage()
    {
        GeneratedImage = _imageGenerator.GenerateImage(StackElements);
    }
}