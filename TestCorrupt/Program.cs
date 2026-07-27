using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Program {
    static void Main() {
        try {
            var info = Image.Identify("nonexistent.jpg");
            Console.WriteLine("Identified!");
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        File.WriteAllText("test_text.jpg", "hello world");
        try {
            var info = Image.Identify("test_text.jpg");
            Console.WriteLine("Identified!");
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        using (var image = new Image<Rgba32>(100, 50))
        {
            image.SaveAsJpeg("test_corrupted.jpg");
        }
        using (var fs = new FileStream("test_corrupted.jpg", FileMode.Open))
        {
            fs.SetLength(30); // Very short
        }
        try {
            var info = Image.Identify("test_corrupted.jpg");
            Console.WriteLine("Identified!");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }
}
