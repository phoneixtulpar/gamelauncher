using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace GameLauncher.Models
{
    [Serializable]
    public class ImageCacheEntry
    {
        public string Url { get; set; }
        public byte[] Bytes { get; set; }
    }

    [Serializable]
    public class ImageCacheSettings
    {
        public List<ImageCacheEntry> ImageCache { get; set; }

        public ImageCacheSettings ()
        {
            ImageCache = new List<ImageCacheEntry> ();
        }
    }

    public static class ImageCache
    {
        private static ImageCacheSettings _settings = new ImageCacheSettings ();

        public static void AddImageToCache (string url, byte[] imageBytes)
        {
            var entry = new ImageCacheEntry { Url = url, Bytes = imageBytes };
            _settings.ImageCache.Add (entry);
            SaveCache ();
        }

        public static BitmapImage GetImageFromCache (string url)
        {
            var entry = _settings.ImageCache.FirstOrDefault (e => e.Url == url);
            if (entry != null)
            {
                var image = new BitmapImage ();
                using (var stream = new MemoryStream (entry.Bytes))
                {
                    image.BeginInit ();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit ();
                }
                return image;
            }
            else
            {
                return null;
            }
        }

        private static string GetCacheFilePath ()
        {
            string appDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
            string cacheFolder = Path.Combine (appDataFolder, "GameLauncher", "Cache");
            string cacheFile = Path.Combine (cacheFolder, "ImageCache.xml");
            return cacheFile;
        }

        private static void SaveCache ()
        {
            string cacheFile = GetCacheFilePath ();
            XmlSerializer serializer = new XmlSerializer (typeof (ImageCacheSettings));
            using (TextWriter writer = new StreamWriter (cacheFile))
            {
                serializer.Serialize (writer, _settings);
            }
        }

        private static void LoadCache ()
        {
            string cacheFile = GetCacheFilePath ();
            XmlSerializer serializer = new XmlSerializer (typeof (ImageCacheSettings));
            if (File.Exists (cacheFile))
            {
                using (TextReader reader = new StreamReader (cacheFile))
                {
                    _settings = (ImageCacheSettings)serializer.Deserialize (reader);
                }
            }
        }

        static ImageCache ()
        {
            try
            {
                string cacheFolder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "GameLauncher", "Cache");
                Directory.CreateDirectory (cacheFolder);
                LoadCache ();
            }
            catch (Exception ex)
            {
                Console.WriteLine ($"Error al inicializar la caché de imágenes: {ex.Message}");
            }
        }
    }
}