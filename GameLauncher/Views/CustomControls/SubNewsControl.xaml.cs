using GameLauncher.Models;
using System.Windows;
using System.Windows.Controls;

namespace GameLauncher.Controls
{
    /// <summary>
    /// Interaction logic for SubNewsControl.xaml
    /// </summary>
    public partial class SubNewsControl : UserControl
    {
        public SubNewsControl()
        {
            InitializeComponent();
        }

        // Public property to set the URL of the image
        public string ImageURL
        {
            get { return (string)GetValue (ImageURLProperty); }
            set { SetValue (ImageURLProperty, value); }
        }

        public static readonly DependencyProperty ImageURLProperty =
            DependencyProperty.Register ("ImageURL", typeof (string), typeof (SubNewsControl), new PropertyMetadata (null, OnImageURLPropertyChanged));

        private static void OnImageURLPropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SubNewsControl control = d as SubNewsControl;
            if (control != null)
            {
                string imageUrl = e.NewValue as string;
                Utility.LoadImage (imageUrl, control.Image);
            }
        }

        // Public property to expose the value of the title Content_Title
        public string Title
        {
            get { return Content_Title.Text; }
            set { Content_Title.Text = value; }
        }

        // Public property to expose the value of the content Content_Text
        public string Text
        {
            get { return Content_Text.Text; }
            set { Content_Text.Text = value; }
        }

        // Public property to expose the value of the date Content_Date
        public string Date
        {
            get { return Content_Date.Text; }
            set { Content_Date.Text = value; }
        }

        // Propiedad pública para exponer la visibilidad del título Content_Title
        public bool IsContentVisible
        {
            get { return TextContainer.Visibility == Visibility.Visible; }
            set { TextContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        // Propiedad pública para exponer la visibilidad del título Content_Title
        public bool IsTitleVisible
        {
            get { return Content_Title.Visibility == Visibility.Visible; }
            set { Content_Title.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        // Propiedad pública para exponer la visibilidad del subtitulo Content_Text
        public bool IsSubtitleVisible
        {
            get { return Content_Text.Visibility == Visibility.Visible; }
            set { Content_Text.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        // Propiedad pública para exponer la visibilidad del contenido Content_Text
        public bool IsTextVisible
        {
            get { return Content_Text.Visibility == Visibility.Visible; }
            set { Content_Text.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        // Propiedad pública para exponer la visibilidad de la fecha Content_Date
        public bool IsDateVisible
        {
            get { return Content_Date.Visibility == Visibility.Visible; }
            set { Content_Date.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
