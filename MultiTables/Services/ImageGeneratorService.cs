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


            var pixelSize = new PixelSize((int)(totalWidth*10), (int)(totalHeight*10));

            var dpi = new Vector(300, 300);

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
                            // Заливаем белым фон
                            ctx.FillRectangle(Brushes.White, new Rect(sectionX, elementY, sectionWidth, elementHeight));
                            // Рисуем только правую и нижнюю границы (чтобы не дублировались)
                            // Правая граница
                            if (k < element.Sections.Count - 1 || i < stackElements.Count - 1)
                            {
                                ctx.FillRectangle(Brushes.Black,
                                    new Rect(sectionX + sectionWidth - 1, elementY, 1, elementHeight));
                            }

                            // Нижняя граница
                            if (j < listElements.ElementsList.Count - 1)
                            {
                                ctx.FillRectangle(Brushes.Black,
                                    new Rect(sectionX, elementY + elementHeight - 1, sectionWidth, 1));
                            }

                            // Рисуем текст в секции
                            if (section.Text == null) continue;
                            var formattedText = new FormattedText(
                                section.Text,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(section.FontFamily),
                                section.FontSize/4,
                                Brushes.Black);
                            
                            // Центрируем текст
                            double textX = sectionX + (sectionWidth - formattedText.Width) / 2;
                            double textY = elementY + (elementHeight - formattedText.Height) / 2;

                            ctx.DrawText(formattedText, new Point(textX, textY));
                        }
                    }

                    currentX += listWidth; // Смещаемся на ширину текущего листа
                }

                // Рисуем внешние границы (верх и лево)
                ctx.FillRectangle(Brushes.Black, new Rect(0, 0, totalWidth, 1)); // Верх
                ctx.FillRectangle(Brushes.Black, new Rect(0, 0, 1, totalHeight)); // Лево
                ctx.FillRectangle(Brushes.Black, new Rect(totalWidth, 0, 1, totalHeight)); // Право
                ctx.FillRectangle(Brushes.Black, new Rect(0, totalHeight-1, totalWidth+1, 1)); // Низ
            }

            return renderBitmap;
        }
    }
}