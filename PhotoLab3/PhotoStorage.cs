using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace PhotoLab3;

// Класс для сохранения и загрузки проекта
// Сохраняет JSON с описанием проекта и PNG-файлы фотографий
public class PhotoStorage
{
    // Это вспомогательный класс, который записывается в JSON.
    public class SaveData
    {
        // Номер выбранной фотографии в списке
        public int SelectedIndex { get; set; }

        // Список всех фотографий проекта
        public List<Photo> Photos { get; set; } = new List<Photo>();
    }

    /// Сохраняет фотографии и информацию о проекте
    public void Save(string folder, List<Photo> photos, int selectedIndex)
    {
        // Создаём папку проекта, если её ещё нет
        Directory.CreateDirectory(folder);
        string imageFolder = Path.Combine(folder, "Images");
        Directory.CreateDirectory(imageFolder);

        // Сохраняем все фотографии в формате PNG
        foreach (Photo photo in photos)
        {
            if (photo.Image == null)
            {
                throw new InvalidOperationException("Нельзя сохранить фотографию без изображения: " + photo.Name);
            }

            photo.Image.Save(Path.Combine(imageFolder, photo.FileName), ImageFormat.Png);
        }
        // Создаём объект с данными для сохранения
        SaveData data = new SaveData
        {
            SelectedIndex = selectedIndex,
            Photos = photos
        };

        // Сериализация: преобразуем объект data в текст формата JSON
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            // Форматируем JSON для удобного чтения
            WriteIndented = true
        });
        // Записываем JSON в файл project.json
        File.WriteAllText(Path.Combine(folder, "project.json"), json);
    }

    /// Загружает сохранённый проект, Возвращает список фотографий и индекс выбранной
    public List<Photo> Load(string folder, out int selectedIndex)
    {
        // По умолчанию фотография не выбрана
        selectedIndex = -1;
        string file = Path.Combine(folder, "project.json");
        if (!File.Exists(file)) return new List<Photo>();

        string json = File.ReadAllText(file);
        // Десериализация: восстанавливаем объект SaveData из JSON
        SaveData data = JsonSerializer.Deserialize<SaveData>(json)
            ?? throw new InvalidDataException("Не удалось прочитать project.json");

        List<Photo> loaded = new List<Photo>();
        try
        {
            // Восстанавливаем изображения по очереди
            foreach (Photo photo in data.Photos)
            {
                if (string.IsNullOrWhiteSpace(photo.FileName) ||
                    Path.GetFileName(photo.FileName) != photo.FileName ||
                    !photo.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Неверное имя файла в project.json");

                // Определяем расположение PNG-файла
                string imagePath = Path.Combine(folder, "Images", photo.FileName);

                // Проверяем, существует ли файл фотографии.
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException(
                        "Не найдена сохранённая фотография: " +
                        photo.FileName);
                }

                // Загружаем изображение из файла
                using (Bitmap original = new Bitmap(imagePath))
                {
                    photo.Image = new Bitmap(original);
                }
                loaded.Add(photo);
            }
        }
        catch
        {
            // Если загрузка прервалась из-за ошибки, освобождаем уже загруженные изображения
            foreach (Photo photo in loaded) photo.Image?.Dispose();
            throw;
        }

        // Восстанавливаем выбранную фотографию
        selectedIndex = data.SelectedIndex >= 0 && data.SelectedIndex < loaded.Count
            ? data.SelectedIndex : (loaded.Count > 0 ? 0 : -1);
        return loaded;
    }
}
