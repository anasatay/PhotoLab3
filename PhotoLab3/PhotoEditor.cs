using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoLab3;

// Класс для обработки фотографий
// Реализует все методы интерфейса IPhotoEditor
public class PhotoEditor : IPhotoEditor
{
    // Обрезает фотографию и возвращает новую картинку
    public Bitmap Crop(Bitmap image, int x, int y, int width, int height)
    {
        // Проверяем, что координаты и размеры допустимы
        if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
            (long)x + width > image.Width || (long)y + height > image.Height)
            throw new ArgumentException("Область обрезки выходит за границы фотографии.");

        Bitmap result = new Bitmap(width, height);
        // Копируем выбранную область исходной фотографии
        using (Graphics graphics = Graphics.FromImage(result))
        {
            graphics.DrawImage(image, new Rectangle(0, 0, width, height),
                new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
        }
        return result;
    }

    // Поворачивает фотографию на 90, 180 или 270 градусов
    public Bitmap Rotate(Bitmap image, int degrees)
    {
        // Проверяем, что пользователь ввёл допустимый угол
        if (degrees != 90 && degrees != 180 && degrees != 270)
            throw new ArgumentException("Можно повернуть на 90, 180 или 270 градусов.");

        Bitmap result = new Bitmap(image);
        // Выполняем поворот в зависимости от угла
        if (degrees == 90) result.RotateFlip(RotateFlipType.Rotate90FlipNone);
        if (degrees == 180) result.RotateFlip(RotateFlipType.Rotate180FlipNone);
        if (degrees == 270) result.RotateFlip(RotateFlipType.Rotate270FlipNone);
        return result;
    }

    // Изменяет цвет одного пикселя по его координатам
    public void ChangePixel(Bitmap image, int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
            throw new ArgumentException("Координаты пикселя неверны.");

        // Устанавливаем новый цвет выбранного пикселя
        image.SetPixel(x, y, color);
    }

    // Увеличивает или уменьшает яркость фотографии
    public void Brightness(Bitmap image, int value)
    {
        // Внешний цикл проходит по ширине изображения
        for (int x = 0; x < image.Width; x++)
        {
            // Внутренний цикл проходит по высоте (циклы перебирают все пиксели)
            for (int y = 0; y < image.Height; y++)
            {
                // Текущий цвет пикселя
                Color p = image.GetPixel(x, y);
                // Добавляем к каждой составляющей цвета значение яркости
                int r = Math.Clamp(p.R + value, 0, 255);
                int g = Math.Clamp(p.G + value, 0, 255);
                int b = Math.Clamp(p.B + value, 0, 255);
                // Записываем новый цвет
                image.SetPixel(x, y, Color.FromArgb(p.A, r, g, b));
            }
        }
    }

    // Преобразует фотографию в оттенки серого
    public void Gray(Bitmap image)
    {
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                // Исходный цвет пикселя
                Color p = image.GetPixel(x, y);
                int gray = (p.R + p.G + p.B) / 3;
                // Если R = G = B, получается оттенок серого
                image.SetPixel(x, y, Color.FromArgb(p.A, gray, gray, gray));
            }
        }
    }

    // Создаёт негатив фотографии (каждый цвет заменяется на противоположный)
    public void Negative(Bitmap image)
    {
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Color p = image.GetPixel(x, y);
                // Для негатива вычитаем значение каждого цветового канала из 255
                image.SetPixel(x, y, Color.FromArgb(p.A, 255 - p.R, 255 - p.G, 255 - p.B));
            }
        }
    }

    // Добавляет надпись на фотографию
    public Bitmap AddText(Bitmap image, string text, int x, int y)
    {
        if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
            throw new ArgumentException("Координаты надписи неверны.");

        Bitmap result = new Bitmap(image);
        // Создаём инструменты для рисования текста
        using (Graphics graphics = Graphics.FromImage(result))
        using (Font font = new Font("Arial", 70, FontStyle.Bold))
        using (Brush brush = new SolidBrush(Color.Red))
        {
            // Рисуем надпись в указанном месте
            graphics.DrawString(text, font, brush, x, y);
        }
            return result;
    }

    // Создаёт коллаж из двух фотографий (изображения располагаются рядом по горизонтали)
    public Bitmap Collage(Bitmap first, Bitmap second)
    {
        // Общая ширина равна сумме ширин фотографий, checked позволяет обнаружить переполнение
        int width = checked(first.Width + second.Width);
        // Высота коллажа равна высоте большей фотографии
        int height = Math.Max(first.Height, second.Height);

        Bitmap result = new Bitmap(width, height);
        using (Graphics graphics = Graphics.FromImage(result))
        {
            // Заливаем фон белым цветом
            graphics.Clear(Color.White);
            // Первую фотографию размещаем слева
            graphics.DrawImage(first, 0, 0, first.Width, first.Height);
            // Вторую размещаем справа от первой
            graphics.DrawImage(second, first.Width, 0, second.Width, second.Height);
        }
        return result;
    }
}
