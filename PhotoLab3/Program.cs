using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoLab3;

// Класс консольной программы
internal class Program
{
    static List<Photo> photos = new List<Photo>();
    static int selectedIndex = -1;
    static IPhotoEditor editor = new PhotoEditor();
    static PhotoStorage storage = new PhotoStorage();

    // Папка, в которой будет храниться проект
    static string projectFolder = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.MyDocuments), "PhotoLab3Saved");

    static void Main()
    {

        try
        {
            string file = Path.Combine(projectFolder, "project.json");

            if (File.Exists(file))
            {
                // Восстанавливаем ранее сохранённый проект.
                photos = storage.Load(projectFolder, out selectedIndex);
                Console.WriteLine("Сохранённый проект загружен.");
            }
            else
            {
                // Если сохранения нет, начинаем новый проект.
                Console.WriteLine("Создан новый пустой проект.");
            }
        }
        catch (Exception ex)
        {
            // При ошибке не перезаписываем старое сохранение.
            Console.WriteLine("Ошибка загрузки проекта: " + ex.Message);
            return;
        }

        bool working = true;
        while (working)
        {
            ShowMenu();
            string command = Console.ReadLine() ?? "";
            try
            {
                switch (command)
                {
                    case "1": ImportPhoto(); break;
                    case "2": ListPhotos(); break;
                    case "3": SelectPhoto(); break;
                    case "4": ViewPhoto(); break;
                    case "5": Crop(); break;
                    case "6": Rotate(); break;
                    case "7": Brightness(); break;
                    case "8": Gray(); break;
                    case "9": Negative(); break;
                    case "10": ChangePixel(); break;
                    case "11": AddText(); break;
                    case "12": Collage(); break;
                    case "13": ExportPhoto(); break;
                    case "14": SaveProject(); break;
                    case "0":
                        SaveProject();
                        working = false;
                        break;
                    default: Console.WriteLine("Такой команды нет."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        // Освобождаем память, занятую изображениями
        foreach (Photo photo in photos) photo.Image?.Dispose();
    }

    static void ShowMenu()
    {
        Console.WriteLine("\nФОТОРЕДАКТОР");
        Console.WriteLine("1 - Загрузить фотографию");
        Console.WriteLine("2 - Список фотографий");
        Console.WriteLine("3 - Выбрать фотографию");
        Console.WriteLine("4 - Посмотреть фотографию");
        Console.WriteLine("5 - Обрезать");
        Console.WriteLine("6 - Повернуть");
        Console.WriteLine("7 - Изменить яркость");
        Console.WriteLine("8 - Сделать чёрно-белой");
        Console.WriteLine("9 - Негатив");
        Console.WriteLine("10 - Изменить пиксель");
        Console.WriteLine("11 - Добавить надпись");
        Console.WriteLine("12 - Создать коллаж из двух фото");
        Console.WriteLine("13 - Сохранить фото в PNG");
        Console.WriteLine("14 - Сохранить проект");
        Console.WriteLine("0 - Сохранить и выйти");
        Console.Write("Команда: ");
    }

    // Выводит сообщение и считывает строку пользователя
    static string ReadText(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? "";
    }

    // Считывает целое число с клавиатуры
    static int ReadNumber(string message)
    {
        string text = ReadText(message);
        if (!int.TryParse(text, out int number))
            throw new ArgumentException("Нужно ввести целое число.");
        return number;
    }

    // Возвращает выбранную фотографию из списка
    static Photo CurrentPhoto()
    {
        if (selectedIndex < 0 || selectedIndex >= photos.Count)
            throw new InvalidOperationException("Сначала загрузите и выберите фотографию.");
        return photos[selectedIndex];
    }

    // Заменяет старое изображение новым после обработки
    static void ReplaceImage(Bitmap result)
    {
        Photo photo = CurrentPhoto();
        photo.Image?.Dispose();
        photo.Image = result;
        Console.WriteLine("Готово!");
    }

    // Загружает фотографию с компьютера
    static void ImportPhoto()
    {
        string path = ReadText("Полный путь к JPG или PNG: ").Trim().Trim('"');
        if (!File.Exists(path)) throw new FileNotFoundException("Файл не найден.");
        using (Bitmap original = new Bitmap(path))
        {
            Photo photo = new Photo
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Image = new Bitmap(original)
            };
            photos.Add(photo);
        }
        selectedIndex = photos.Count - 1;
        Console.WriteLine("Фотография загружена и выбрана.");
    }

    // Показывает список загруженных фотографий
    static void ListPhotos()
    {
        if (photos.Count == 0) Console.WriteLine("Фотографий пока нет.");
        for (int i = 0; i < photos.Count; i++)
        {
            string mark = i == selectedIndex ? " <-- выбрана" : "";
            Console.WriteLine($"{i + 1}. {photos[i].Name}{mark}");
        }
    }

    // Позволяет выбрать фотографию по её номеру
    static void SelectPhoto()
    {
        ListPhotos();
        int number = ReadNumber("Номер фотографии: ");
        if (number < 1 || number > photos.Count)
            throw new ArgumentException("Нет фотографии с таким номером.");
        selectedIndex = number - 1;
        Console.WriteLine("Фотография выбрана.");
    }

    // Открывает фотографию в стандартном приложении Windows, которое назначено для просмотра PNG-файлов
    static void ViewPhoto()
    {
        Photo photo = CurrentPhoto();

        // Создаём временный файл для просмотра фотографии
        string path = Path.Combine(Path.GetTempPath(), "PhotoLab3_preview_" + photo.FileName);
        photo.Image!.Save(path, ImageFormat.Png);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    // Получает от пользователя параметры обрезки
    static void Crop()
    {
        Bitmap image = CurrentPhoto().Image!;

        Console.WriteLine($"Размер изображения: {image.Width} x {image.Height}");
        int x = ReadNumber("X: ");
        int y = ReadNumber("Y: ");
        int width = ReadNumber("Ширина: ");
        int height = ReadNumber("Высота: ");

        // Вызываем метод редактора и заменяем изображение
        ReplaceImage(editor.Crop(image, x, y, width, height));
    }

    // Поворачивает выбранную фотографию
    static void Rotate()
    {
        Bitmap image = CurrentPhoto().Image!;
        int degrees = ReadNumber("Угол (90, 180 или 270): ");
        ReplaceImage(editor.Rotate(image, degrees));
    }

    // Изменяет яркость выбранной фотографии
    static void Brightness()
    {
        int value = ReadNumber("Изменение яркости (например, 40 или -40): ");
        editor.Brightness(CurrentPhoto().Image!, value);
        Console.WriteLine("Готово!");
    }

    // Применяет фильтр оттенков серого
    static void Gray()
    {
        editor.Gray(CurrentPhoto().Image!);
        Console.WriteLine("Готово!");
    }

    // Применяет негатив к выбранной фотографии
    static void Negative()
    {
        editor.Negative(CurrentPhoto().Image!);
        Console.WriteLine("Готово!");
    }

    // Изменяет цвет отдельного пикселя
    static void ChangePixel()
    {
        Bitmap image = CurrentPhoto().Image!;

        Console.WriteLine($"Координаты: X от 0 до {image.Width - 1}, Y от 0 до {image.Height - 1}");
        int x = ReadNumber("X: ");
        int y = ReadNumber("Y: ");
        int r = ReadNumber("Красный (0-255): ");
        int g = ReadNumber("Зелёный (0-255): ");
        int b = ReadNumber("Синий (0-255): ");

        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            throw new ArgumentException("Значения цветов должны быть от 0 до 255.");

        editor.ChangePixel(image, x, y, Color.FromArgb(r, g, b));

        // Повторно считываем цвет изменённого пикселя.
        Color pixel = image.GetPixel(x, y);

        // Выводим координаты и цвет для проверки.
        Console.WriteLine("Пиксель успешно изменён!");
        Console.WriteLine($"Координаты: X = {x}, Y = {y}");
        Console.WriteLine(
            $"Новый цвет: R = {pixel.R}, " +
            $"G = {pixel.G}, B = {pixel.B}");
    }

    // Добавляет надпись на выбранную фотографию
    static void AddText()
    {
        Bitmap image = CurrentPhoto().Image!;
        string text = ReadText("Текст: ");
        int x = ReadNumber("X: ");
        int y = ReadNumber("Y: ");

        ReplaceImage(editor.AddText(image, text, x, y));
    }

    // Создаёт коллаж из текущей и второй фотографии
    static void Collage()
    {
        Photo first = CurrentPhoto();
        ListPhotos();
        int number = ReadNumber("Номер второй фотографии: ");
        if (number < 1 || number > photos.Count)
            throw new ArgumentException("Нет фотографии с таким номером.");

        // Создаём новое изображение из двух фотографий
        Bitmap image = editor.Collage(first.Image!, photos[number - 1].Image!);

        // Добавляем коллаж как отдельную фотографию
        photos.Add(new Photo { Name = "Коллаж", Image = image });
        selectedIndex = photos.Count - 1;
        Console.WriteLine("Коллаж добавлен как новая фотография.");
    }

    // Сохраняет выбранную фотографию в отдельный PNG-файл
    static void ExportPhoto()
    {
        string path = ReadText("Куда сохранить PNG (полный путь): ").Trim().Trim('"');

        // Проверяем расширение выходного файла
        if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Укажите имя файла с расширением .png");

        CurrentPhoto().Image!.Save(path, ImageFormat.Png);
        Console.WriteLine("Фотография сохранена: " + path);
    }

    // Сохраняет состояние всего проекта
    static void SaveProject()
    {
        // Передаём хранилищу папку, фотографии и индекс выбранной фотографии
        storage.Save(projectFolder, photos, selectedIndex);
        Console.WriteLine("Проект сохранён: " + projectFolder);
    }
}
