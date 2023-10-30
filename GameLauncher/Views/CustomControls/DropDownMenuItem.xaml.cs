using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GameLauncher.Controls
{
    public partial class DropDownMenuItem : UserControl
    {

        public DropDownMenuItem ()
        {
            InitializeComponent ();

            DataContext = this;
            iconImage.Visibility = string.IsNullOrEmpty (Icon) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register ("Text", typeof (string), typeof (DropDownMenuItem), new PropertyMetadata (""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register ("Icon", typeof (string), typeof (DropDownMenuItem), new PropertyMetadata ("", OnIconChanged));

        public string Text
        {
            get { return (string)GetValue (TextProperty); }
            set { SetValue (TextProperty, value); }
        }

        public string Icon
        {
            get { return (string)GetValue (IconProperty); }
            set { SetValue (IconProperty, value); }
        }
        private static void OnIconChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DropDownMenuItem)d;
            control.SetImageSource ((string)e.NewValue);
        }

        private void SetImageSource (string imagePath)
        {
            if (string.IsNullOrEmpty (imagePath))
            {
                iconImage.Visibility = Visibility.Collapsed; // Ocultar la imagen si no se proporciona una ruta de icono
                return;
            }

            try
            {
                var bitmap = new BitmapImage ();
                bitmap.BeginInit ();
                bitmap.UriSource = new Uri (imagePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit ();
                iconImage.Source = bitmap;
                iconImage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // Manejar excepción si ocurre un error al cargar la imagen
            }
        }
    }
}