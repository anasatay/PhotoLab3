using System.Drawing;
using PhotoLab3;
using Xunit;

namespace PhotoLab3.Tests;

// Модульные тесты для проверки обработки фотографий и сохранения проекта
public class PhotoEditorTests
{
    private readonly PhotoEditor editor = new PhotoEditor();

    // 1. Проверка обрезки фотографии
    [Fact]
    public void TestCrop()
    {
        using Bitmap image = new Bitmap(5, 4);
        image.SetPixel(2, 1, Color.Red);

        using Bitmap result = editor.Crop(image, 2, 1, 2, 2);

        Assert.Equal(2, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal(Color.Red.ToArgb(),
            result.GetPixel(0, 0).ToArgb());
    }

    // 2. Проверка неправильных координат обрезки
    [Fact]
    public void TestCropError()
    {
        using Bitmap image = new Bitmap(4, 4);

        Assert.Throws<ArgumentException>(() =>
            editor.Crop(image, 3, 3, 4, 4));
    }

    // 3. Проверка поворота фотографии
    [Fact]
    public void TestRotate()
    {
        using Bitmap image = new Bitmap(5, 3);
        using Bitmap result = editor.Rotate(image, 90);

        Assert.Equal(3, result.Width);
        Assert.Equal(5, result.Height);
    }

    // 4. Проверка неправильного угла поворота
    [Fact]
    public void TestRotateError()
    {
        using Bitmap image = new Bitmap(3, 2);

        Assert.Throws<ArgumentException>(() =>
            editor.Rotate(image, 45));
    }

    // 5. Проверка изменения цвета пикселя
    [Fact]
    public void TestPixel()
    {
        using Bitmap image = new Bitmap(2, 2);

        editor.ChangePixel(image, 1, 1, Color.Red);

        Assert.Equal(Color.Red.ToArgb(),
            image.GetPixel(1, 1).ToArgb());
    }

    // 6. Проверка неправильных координат пикселя
    [Fact]
    public void TestPixelError()
    {
        using Bitmap image = new Bitmap(2, 2);

        Assert.Throws<ArgumentException>(() =>
            editor.ChangePixel(image, 2, 0, Color.Red));
    }

    // 7. Проверка увеличения яркости
    [Fact]
    public void TestBrightness()
    {
        using Bitmap image = new Bitmap(1, 1);
        image.SetPixel(0, 0,
            Color.FromArgb(100, 120, 140));

        editor.Brightness(image, 30);

        Assert.Equal(
            Color.FromArgb(130, 150, 170).ToArgb(),
            image.GetPixel(0, 0).ToArgb());
    }

    // 8. Проверка максимальной яркости
    [Fact]
    public void TestBrightnessLimit()
    {
        using Bitmap image = new Bitmap(1, 1);
        image.SetPixel(0, 0,
            Color.FromArgb(240, 250, 255));

        editor.Brightness(image, 30);

        Assert.Equal(Color.White.ToArgb(),
            image.GetPixel(0, 0).ToArgb());
    }

    // 9. Проверка негатива
    [Fact]
    public void TestNegative()
    {
        using Bitmap image = new Bitmap(1, 1);
        image.SetPixel(0, 0,
            Color.FromArgb(100, 50, 20));

        editor.Negative(image);

        Assert.Equal(
            Color.FromArgb(155, 205, 235).ToArgb(),
            image.GetPixel(0, 0).ToArgb());
    }

    // 10. Проверка чёрно-белого фильтра
    [Fact]
    public void TestGray()
    {
        using Bitmap image = new Bitmap(1, 1);
        image.SetPixel(0, 0,
            Color.FromArgb(30, 60, 90));

        editor.Gray(image);

        Assert.Equal(
            Color.FromArgb(60, 60, 60).ToArgb(),
            image.GetPixel(0, 0).ToArgb());
    }

    // 11. Проверка создания коллажа
    [Fact]
    public void TestCollage()
    {
        using Bitmap first = new Bitmap(4, 3);
        using Bitmap second = new Bitmap(2, 5);

        using (Graphics g = Graphics.FromImage(first))
            g.Clear(Color.Red);

        using (Graphics g = Graphics.FromImage(second))
            g.Clear(Color.Blue);

        using Bitmap result = editor.Collage(first, second);

        Assert.Equal(6, result.Width);
        Assert.Equal(5, result.Height);

        Assert.Equal(Color.Red.ToArgb(),
            result.GetPixel(1, 1).ToArgb());

        Assert.Equal(Color.Blue.ToArgb(),
            result.GetPixel(5, 1).ToArgb());

        Assert.Equal(Color.White.ToArgb(),
            result.GetPixel(1, 4).ToArgb());
    }

    // 12. Проверка добавления надписи
    [Fact]
    public void TestText()
    {
        using Bitmap image = new Bitmap(300, 100);

        using (Graphics g = Graphics.FromImage(image))
            g.Clear(Color.White);

        using Bitmap result =
            editor.AddText(image, "Привет", 10, 10);

        bool changed = false;

        for (int x = 0; x < result.Width && !changed; x++)
        {
            for (int y = 0; y < result.Height; y++)
            {
                if (result.GetPixel(x, y).ToArgb()
                    != Color.White.ToArgb())
                {
                    changed = true;
                    break;
                }
            }
        }

        Assert.True(changed);
    }

    // 13. Проверка неправильных координат надписи
    [Fact]
    public void TestTextError()
    {
        using Bitmap image = new Bitmap(100, 100);

        Assert.Throws<ArgumentException>(() =>
            editor.AddText(image, "Текст", 100, 10));
    }

    // 14. Проверка сохранения и загрузки фотографии
    [Fact]
    public void TestSaveLoad()
    {
        string folder = Path.Combine(
            Path.GetTempPath(),
            "PhotoLab3_test_" + Guid.NewGuid());

        PhotoStorage storage = new PhotoStorage();

        using Bitmap image = new Bitmap(3, 2);
        image.SetPixel(0, 0, Color.Red);

        List<Photo> photos = new List<Photo>
        {
            new Photo { Name = "Тест", Image = image }
        };

        try
        {
            storage.Save(folder, photos, 0);

            Assert.True(File.Exists(
                Path.Combine(folder, "project.json")));

            Assert.True(File.Exists(
                Path.Combine(folder, "Images",
                photos[0].FileName)));

            List<Photo> loaded =
                storage.Load(folder, out int index);

            try
            {
                Assert.Single(loaded);
                Assert.Equal("Тест", loaded[0].Name);
                Assert.Equal(0, index);

                Assert.Equal(Color.Red.ToArgb(),
                    loaded[0].Image!
                    .GetPixel(0, 0).ToArgb());
            }
            finally
            {
                foreach (Photo photo in loaded)
                    photo.Image?.Dispose();
            }
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    // 15. Проверка загрузки несуществующего проекта
    [Fact]
    public void TestLoadEmpty()
    {
        string folder = Path.Combine(
            Path.GetTempPath(),
            "PhotoLab3_missing_" + Guid.NewGuid());

        PhotoStorage storage = new PhotoStorage();

        List<Photo> photos =
            storage.Load(folder, out int index);

        Assert.Empty(photos);
        Assert.Equal(-1, index);
    }

    // 16. Проверка сохранения пустого проекта
    [Fact]
    public void TestEmptyProject()
    {
        string folder = Path.Combine(
            Path.GetTempPath(),
            "PhotoLab3_empty_" + Guid.NewGuid());

        PhotoStorage storage = new PhotoStorage();

        try
        {
            storage.Save(folder, new List<Photo>(), -1);

            List<Photo> loaded =
                storage.Load(folder, out int index);

            Assert.Empty(loaded);
            Assert.Equal(-1, index);
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }
}
