using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MultiTables.Models;

namespace MultiTables.Services
{
    public class ImageGeneratorService : IImageGeneratorService
    {
        public Bitmap GenerateImage(ObservableCollection<ListElements> stackElements)
        {
            var totalWidth = 0d;
            var totalHeight = 0d;
            if (stackElements.Count == 0)
            {
                totalWidth = 100d;
                totalHeight = 100d;
            }
            if (stackElements.Count > 0)
            {
                foreach (var list in stackElements)
                {
                    totalHeight = Math.Max(totalHeight, list.Height);
                    totalWidth += list.Width;
                }
            }

            const double scale = 10;
            var scaledWidth = totalWidth * scale;
            var scaledHeight = totalHeight * scale;

            var pixelSize = new PixelSize((int)scaledWidth, (int)scaledHeight);
            var dpi = new Vector(96, 96);
            var renderBitmap = new RenderTargetBitmap(pixelSize, dpi);

            using (var ctx = renderBitmap.CreateDrawingContext(true))
            {
                double currentX = 0;
                for (int i = 0; i < stackElements.Count; i++)
                {
                    var listElements = stackElements[i];
                    double listWidth;

                    // Для последнего элемента используем оставшуюся ширину
                    if (i == stackElements.Count - 1)
                    {
                        listWidth = totalWidth - currentX;
                    }
                    else
                    {
                        listWidth = listElements.Width;
                    }

                    // Высота одного элемента внутри листа (вертикально)
                    double elementHeight = totalHeight / listElements.ElementsList.Count;
                    for (int j = 0; j < listElements.ElementsList.Count; j++)
                    {
                        var element = listElements.ElementsList[j];
                        double elementY = j * elementHeight;
                        // Ширина одной секции (горизонтальные элементы)
                        double sectionWidth = listWidth / element.Sections.Count;
                        for (int k = 0; k < element.Sections.Count; k++)
                        {
                            var section = element.Sections[k];
                            double sectionX = currentX + k * sectionWidth;
                            
                            var scaledRect = new Rect(
                                sectionX * scale, 
                                elementY * scale, 
                                sectionWidth * scale, 
                                elementHeight * scale
                            );
                            ctx.FillRectangle(Brushes.White, scaledRect);
                            
                            if (section.ImageBitmap != null)
                            {
                                double imageWidth = sectionWidth * scale;
                                double imageHeight = elementHeight * scale;
    
                                // Вычисляем пропорции
                                double imageAspect = (double)section.ImageBitmap.PixelSize.Width / section.ImageBitmap.PixelSize.Height;
                                double containerAspect = imageWidth / imageHeight;
    
                                double drawWidth, drawHeight, drawX, drawY;
    
                                if (imageAspect > containerAspect)
                                {
                                    // Картинка шире - подгоняем по ширине
                                    drawWidth = imageWidth;
                                    drawHeight = imageWidth / imageAspect;
                                    drawX = sectionX * scale;
                                    drawY = elementY * scale + (elementHeight * scale - drawHeight) / 2;
                                }
                                else
                                {
                                    // Картинка выше - подгоняем по высоте
                                    drawHeight = imageHeight;
                                    drawWidth = imageHeight * imageAspect;
                                    drawX = sectionX * scale + (sectionWidth * scale - drawWidth) / 2;
                                    drawY = elementY * scale;
                                }
     
                                var destRect = new Rect(drawX, drawY, drawWidth, drawHeight);
                                var sourceRect = new Rect(0, 0, section.ImageBitmap.PixelSize.Width, section.ImageBitmap.PixelSize.Height);
    
                                ctx.DrawImage(section.ImageBitmap, sourceRect, destRect);
                            }
                            else if (section.Text != null)
                            {
                                // ctx.PushClip(scaledRect);
                                double fontSize = section.FontSize / 4.5 * scale;
                                
                                var formattedText = new FormattedText(
                                    section.Text,
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    new Typeface(section.VisualFontFamily),
                                    fontSize,
                                    Brushes.Black)
                                {
                                    TextAlignment = TextAlignment.Center,
                                    MaxTextWidth = sectionWidth * scale
                                };
                                double textX = sectionX * scale;
                                double textY = elementY * scale + (elementHeight * scale - formattedText.Height) / 2;

                                ctx.DrawText(formattedText, new Point(textX, textY));
                            }
                            
                            
                            
                            // Правая граница
                            if (k < element.Sections.Count - 1 || i < stackElements.Count - 1)
                            {
                                ctx.FillRectangle(Brushes.Black,
                                    new Rect((sectionX + sectionWidth) * scale - 2, elementY * scale, 2, elementHeight * scale));
                            }
                            // Нижняя граница
                            if (j < listElements.ElementsList.Count - 1)
                            {
                                ctx.FillRectangle(Brushes.Black,
                                    new Rect(sectionX * scale, (elementY + elementHeight) * scale - 2, sectionWidth * scale, 2));
                            }
                        }
                    }

                    currentX += listWidth;
                }

                // внешние границы
                ctx.FillRectangle(Brushes.Black, new Rect(0, 0, scaledWidth, 2)); // Верх
                ctx.FillRectangle(Brushes.Black, new Rect(0, 0, 2, scaledHeight)); // Лево
                ctx.FillRectangle(Brushes.Black, new Rect(scaledWidth - 2, 0, 2, scaledHeight)); // Право
                ctx.FillRectangle(Brushes.Black, new Rect(0, scaledHeight - 2, scaledWidth, 2)); // Низ
            }

            return renderBitmap;
        }
    }
}