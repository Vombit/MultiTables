using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using MultiTables.Models;


namespace MultiTables.Services;

public interface IImageGeneratorService
{
    Bitmap GenerateImage(ObservableCollection<ListElements> stackElements);
}
