using System.Drawing;
using System.Text.Json.Serialization;

namespace PhotoLab3;

// Класс для хранения информации об одной фотографии
public class Photo
{
    // Название фотографии, которое видит пользователь в меню
    public string Name { get; set; } = "";

    // Уникальное имя файла для сохранения изображения
    public string FileName { get; set; } = Guid.NewGuid() + ".png";

    // Само изображение хранится в объекте Bitmap, Bitmap не записываем в JSON: картинку сохраняем отдельным PNG-файлом
    [JsonIgnore]
    public Bitmap? Image { get; set; }
}
