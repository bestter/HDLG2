using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace HDLG.Tests
{
    public class ImageSetup
    {
        public static void CreateImages()
        {
            using (var image = new Image<Rgba32>(100, 50))
            {
                var profile = new ExifProfile();
                profile.SetValue(ExifTag.Model, "TestCameraModel");
                image.Metadata.ExifProfile = profile;
                image.SaveAsJpeg("test.jpg");
            }

            using (var image = new Image<Rgba32>(100, 50))
            {
                image.SaveAsJpeg("test_no_exif.jpg");
            }

            File.WriteAllText("test_invalid.jpg", "invalid image");
        }
    }
}
