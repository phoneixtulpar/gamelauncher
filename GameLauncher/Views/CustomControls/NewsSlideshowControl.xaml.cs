using GameLauncher.Models;
using System.Windows;
using System.Windows.Controls;

namespace GameLauncher.Controls
{
    /// <summary>
    /// Interaction logic for NewsSlideshowControl.xaml
    /// </summary>
    public partial class NewsSlideshowControl : UserControl
    {
        public bool navigationCompleted = false;

        public NewsSlideshowControl ()
        {
            InitializeComponent ();
        }

        // Public property to set the URL of the image
        public string ImageURL
        {
            get { return (string)GetValue (ImageURLProperty); }
            set { SetValue (ImageURLProperty, value); }
        }

        public static readonly DependencyProperty ImageURLProperty =
            DependencyProperty.Register ("ImageURL", typeof (string), typeof (NewsSlideshowControl), new PropertyMetadata (null, OnImageURLPropertyChanged));

        private static void OnImageURLPropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewsSlideshowControl control = d as NewsSlideshowControl;
            if (control != null)
            {
                string imageUrl = e.NewValue as string;
                Utility.LoadImage (imageUrl, control.Image);
            }
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                navigationCompleted = true;
                WebView.Visibility = Visibility.Visible;
            }
            else
            {
                navigationCompleted = false;
            }
        }

        private void News_Button_Click (object sender, RoutedEventArgs e)
        {

            // Get the current news item
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            // Open URL
            Utility.OpenURL (mainWindow.NEWS_CURRENT_URL);
        }
    }
}
