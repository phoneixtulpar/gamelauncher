using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace GameLauncher.Models
{
    public static class Utility
    {
        public static async Task<string> DownloadTextFileAsync (string url)
        {
            Uri uri = new Uri (url);
            string result = null;
            try
            {
                using (WebClient client = new WebClient ())
                {
                    result = await client.DownloadStringTaskAsync (uri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Error downloading file: " + ex.Message);
            }

            return result;
        }

        public static void CreateOpacityAnimation (this UIElement panel, double fromValue, double ToValue, double durationSeconds = 0.5f)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation ();
            opacityAnimation.Duration = TimeSpan.FromSeconds (durationSeconds);
            opacityAnimation.From = fromValue;
            opacityAnimation.To = ToValue;
            panel.BeginAnimation (UIElement.OpacityProperty, opacityAnimation);

            panel.IsHitTestVisible = ToValue > 0;
        }

        public static void FadeOut (this UIElement panel, double durationSeconds = 0.15f)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation ();
            opacityAnimation.Duration = TimeSpan.FromSeconds (durationSeconds);
            opacityAnimation.From = panel.Opacity;
            opacityAnimation.To = 0;
            panel.BeginAnimation (UIElement.OpacityProperty, opacityAnimation);

            panel.IsHitTestVisible = false;
        }

        public static void FadeIn (this UIElement panel, double durationSeconds = 0.5f)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation ();
            opacityAnimation.Duration = TimeSpan.FromSeconds (durationSeconds);
            opacityAnimation.From = panel.Opacity;
            opacityAnimation.To = 1;
            panel.BeginAnimation (UIElement.OpacityProperty, opacityAnimation);

            panel.IsHitTestVisible = true;
        }

        // Check if url exists
        public static bool URLExists (string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        // Check if url exists async
        public static async Task<bool> URLExistsAsync (string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync ();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Open URL in external navigator
        /// </summary>
        /// <param name="URL"></param>
        public static void OpenURL (string URL)
        {
            // Cancel if the URL is null
            if (string.IsNullOrEmpty (URL))
            {
                return;
            }

            // Manage invalid URL's
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo ();
                psi.UseShellExecute = true;
                psi.FileName = URL;
                System.Diagnostics.Process.Start (psi);
            }
            catch
            {

            }
        }

        public static string ExtractIDFromYoutubeURL (string URL)
        {
            var uri = new Uri (URL);

            // you can check host here => uri.Host <= "www.youtube.com"

            var query = HttpUtility.ParseQueryString (uri.Query);
            var videoId = query["v"];
            return videoId;
        }

        public static string GetVideoThumbnail (string ID)
        {

            // Return
            // Maximum Resolution Thumbnail
            return string.Format ("https://img.youtube.com/vi/{0}/maxresdefault.jpg", ID);

        }

        private static void LoadImageCommon (string imageUrl, Action<BitmapImage> setImage)
        {
            if (imageUrl.StartsWith ("pack://application:,,,/"))
            {
                // La imagen es un recurso empaquetado en la aplicación
                setImage (new BitmapImage (new Uri (imageUrl)));
            }
            else
            {
                // La imagen es una URL
                BitmapImage bitmapImage = ImageCache.GetImageFromCache (imageUrl);

                if (bitmapImage != null)
                {
                    setImage (bitmapImage);
                }
                else
                {
                    WebClient client = new WebClient ();
                    client.DownloadDataCompleted += (s, args) =>
                    {
                        if (args.Error != null)
                        {
                            return;
                        }

                        byte[] imageBytes = args.Result;
                        BitmapImage image = new BitmapImage ();
                        using (MemoryStream stream = new MemoryStream (imageBytes))
                        {
                            image.BeginInit ();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = stream;
                            image.EndInit ();
                        }
                        setImage (image);
                        ImageCache.AddImageToCache (imageUrl, imageBytes);
                    };
                    client.DownloadDataAsync (new Uri (imageUrl));
                }
            }
        }

        private static BitmapImage LoadImageCommon (string imageUrl)
        {
            BitmapImage image = null;

            if (imageUrl.StartsWith ("pack://application:,,,/"))
            {
                // La imagen es un recurso empaquetado en la aplicación
                image = new BitmapImage (new Uri (imageUrl));
            }
            else
            {
                // La imagen es una URL
                BitmapImage bitmapImage = ImageCache.GetImageFromCache (imageUrl);

                if (bitmapImage != null)
                {
                    image = bitmapImage;
                }
                else
                {
                    WebClient client = new WebClient ();
                    client.DownloadDataCompleted += (s, args) =>
                    {
                        byte[] imageBytes = args.Result;
                        BitmapImage downloadedImage = new BitmapImage ();
                        using (MemoryStream stream = new MemoryStream (imageBytes))
                        {
                            downloadedImage.BeginInit ();
                            downloadedImage.CacheOption = BitmapCacheOption.OnLoad;
                            downloadedImage.StreamSource = stream;
                            downloadedImage.EndInit ();
                        }
                        ImageCache.AddImageToCache (imageUrl, imageBytes);
                    };
                    client.DownloadDataAsync (new Uri (imageUrl));
                }
            }

            return image;
        }


        /// <summary>
        /// Load Image first from Cache then URL or Resource
        /// </summary>
        /// <param name="imageUrl"></param>
        public static BitmapImage LoadImage (string imageUrl)
        {
            return LoadImageCommon (imageUrl);
        }

        /// <summary>
        /// Load Image first from Cache then URL or Resource then assign it to a Image
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="imageControl"></param>
        public static void LoadImage (string imageUrl, Image imageControl)
        {
            LoadImageCommon (imageUrl, (image) => {
                imageControl.Source = image;
            });
        }

        /// <summary>
        /// Load Image first from Cache then URL or Resource then assign it to a Border
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="imageControl"></param>
        public static void LoadImage (string imageUrl, Border borderControl)
        {
            LoadImageCommon (imageUrl, (image) => {
                borderControl.Background = new ImageBrush (image);
            });
        }

        /// <summary>
        /// Load Image first from Cache then URL or Resource then assign it to a ImageBrush
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="imageControl"></param>
        public static void LoadImage (string imageUrl, ImageBrush imageBrush)
        {
            LoadImageCommon (imageUrl, (image) => {
                imageBrush.ImageSource = image;
            });
        }
        public static string FormatBytes (long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

        public static long GetFreeDiskSpace (string gamePath)
        {
            DriveInfo drive = new DriveInfo (Path.GetPathRoot (gamePath));
            return drive.AvailableFreeSpace;
        }

        /// <summary>
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        public static void RemoveRoutedEventHandlers (UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof (UIElement).GetProperty (
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue (element, null);

            if (eventHandlersStore == null) return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType ().GetMethod (
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke (
                eventHandlersStore, new object[] { routedEvent });

            if (routedEventHandlers == null) return;

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
                element.RemoveHandler (routedEvent, routedEventHandler.Handler);
        }

        public static IEnumerable<T> FindVisualChildren<T> (DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount (depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild (depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T> (child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void RemoveControlFromParent (UIElement control)
        {
            DependencyObject parent = VisualTreeHelper.GetParent (control);

            if (parent == null)
            {
                return;
            }

            if (parent is Panel panel)
            {
                panel.Children.Remove (control);
            }
            else if (parent is Decorator decorator)
            {
                decorator.Child = null;
            }
            else if (parent is ContentPresenter contentPresenter)
            {
                contentPresenter.Content = null;
            }
            else if (parent is ContentControl contentControl)
            {
                contentControl.Content = null;
            }
            else
            {
                RemoveControlFromParent (parent as UIElement);
            }
        }
    }
}
