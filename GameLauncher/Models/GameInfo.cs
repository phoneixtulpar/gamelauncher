using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace GameLauncher.Models
{
    public class GameInfo : INotifyPropertyChanged
    {

        #region GAME INFO

        public string Title { get; set; } = "Untitled Game";
        public string Subtitle { get; set; } = "No subtitle";
        public bool Show { get; set; } = true;
        public string CoverURL { get; set; } = "";
        public string IconURL { get; set; } = "";
        public string LogoURL { get; set; } = "";
        public string ColorHEX { get; set; } = "#FFFFFF";
        public string DefaultLaunchArgs { get; set; } = "";
        public bool LinkOnly { get; set; } = false;
        public bool IsMultiplayer { get; set; } = false;
        public bool IsMobile { get; set; } = false;
        public bool IsWindows { get; set; } = false;
        public bool IsMacOS { get; set; } = false;
        public bool IsFreeToPlay { get; set; } = false;

        #endregion

        #region LAUNCHER DATA
        public bool IsInstalled { get; set; } = false;
        public DateTime InstallDate { get; set; } = DateTime.Today;
        public DateTime LastPlayedDate { get; set; } = DateTime.Today;
        public string InstallPath
        {
            get
            {
                if (string.IsNullOrEmpty (_installPath))
                {
                    return DefaultInstallPath;
                }
                return _installPath;
            }
            set { _installPath = value; }
        }

        private string _installPath = "";
        public bool AutomaticUpdates { get; set; } = false;
        public string DefaultLanguage { get; set; } = "en_US";
        public bool UseAdditionalLaunchArgs { get; set; } = false;
        public string AdditionalLaunchArgs { get; set; } = "";

        public bool IsFavorite { get; set; } = false;


        #endregion

        [JsonIgnore]
        public bool IsSelected { get; set; } = false;
        [JsonIgnore]
        public string DefaultInstallPath
        {
            get
            {
                return System.IO.Path.Combine (Directory.GetCurrentDirectory (), MainWindow.MAINAPP_SUBDIRECTORY);
            }
        }

        #region INotifyPropertyChanged Members  

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged (string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
            }
        }
        #endregion
    }
}
