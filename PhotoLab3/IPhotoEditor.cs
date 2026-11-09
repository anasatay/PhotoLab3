using System.Drawing;
namespace PhotoLab3;

// Перечень операций, которые умеет выполнять редактор.
public interface IPhotoEditor
{
    // Обрезает изображение по заданным координатам и размерам
    Bitmap Crop(Bitmap image, int x, int y,
                int width, int height);

    // Поворачивает изображение на указанный угол
    Bitmap Rotate(Bitmap image, int degrees);

    // Изменяет цвет одного пикселя
    void ChangePixel(Bitmap image, int x, int y, Color color);

    // Изменяет яркость всех пикселей
    void Brightness(Bitmap image, int value);

    // Преобразует изображение в оттенки серого
    void Gray(Bitmap image);

    // Создаёт негатив изображения
    void Negative(Bitmap image);

    // Добавляет текст на изображение
    Bitmap AddText(Bitmap image, string text, int x, int y);

    // Создаёт коллаж из двух фотографий
    Bitmap Collage(Bitmap first, Bitmap second);
}
