using System;
using System.IO;

namespace GameLauncher.Models
{
    public class Settings
    {
        public string Language { get; set; }
        public bool UseDefaultInstallLocation { get; set; }
        public string CustomInstallFolder { get; set; }
        public int OnAppLaunchAction { get; set; }
        public string LastSelectedGame { get; set; }

        public Settings ()
        {
            Language = "en_US";
            UseDefaultInstallLocation = true;
            CustomInstallFolder = Directory.GetCurrentDirectory();
            OnAppLaunchAction = 0; // 0 Keep open // 1 Minimize // 2 Close
            LastSelectedGame = "";
        }
    }
}