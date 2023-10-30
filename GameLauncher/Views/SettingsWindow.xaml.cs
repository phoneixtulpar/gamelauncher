using GameLauncher.Models;
using GameLauncherCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameLauncher.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow ()
        {
            InitializeComponent ();
            Closed += SettingsWindow_Closed;

            RefreshSettings ();
        }

        public void RefreshSettings ()
        {
            LocationPath.Text = SettingsManager.Settings.CustomInstallFolder;
            ComboBox_Language.SelectedIndex = GetSelectedLanguageIndex (SettingsManager.Settings.Language);
            ComboBox_OnAppLaunchAction.SelectedIndex = SettingsManager.Settings.OnAppLaunchAction;
            checkBox_UseDefaultInstallLocation.IsChecked = SettingsManager.Settings.UseDefaultInstallLocation;

            settings_Downloads_CustomInstallLocation.Visibility = SettingsManager.Settings.UseDefaultInstallLocation ? Visibility.Collapsed : Visibility.Visible;

            RefreshLanguage ();
        }

        // On Open Dialog


        /// <summary>
        /// Refresh this window language
        /// </summary>
        public void RefreshLanguage ()
        {
            /// Launcher Settings Language ///
            lbl_Settings.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings);

            // LeftSide Options //

            // Launcher
            Settings_Option_LAUNCHER.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_MainOptions_Launcher);

            // Downloads
            Settings_Option_DOWNLOADS.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_MainOptions_Downloads);

            // About
            Settings_Option_ABOUT.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_MainOptions_About);

            // Launcher Settings //

            // LauncherSettings/Title
            lbl_LauncherSettingsTitle.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettings_LauncherSettingsTitle);

            // LauncherSettings/UI Language
            lbl_UILanguage.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettings_UILanguage);

            // On App Launch
            lbl_OnAppLaunch.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettings_OnAppLaunch);

            // Downloads Settings //
            lbl_DownloadSettingsTitle.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_DownloadsSettings_DownloadsSettingsTitle);

            // Install location
            checkBox_UseDefaultInstallLocation.Content = GameLauncherCore.Localization.Get (LocalizationID.Settings_DownloadsSettings_UseDefaultInstallLocation);
            lbl_CustomInstallLocation.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_DownloadsSettings_CustomInstallLocation);
            lbl_ChangeLocation.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_DownloadsSettings_ChangeLocation);

            // About Settings //

            // ABOUT/Title
            lbl_AboutSettingsTitle.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_About_AboutSettingsTitle);

            // ABOUT/Get GameLauncher
            lbl_GetGameLauncher.Text = GameLauncherCore.Localization.Get (LocalizationID.Settings_About_GetGameLauncher);

            // Terms and Privacy
            TERMS_OF_SERVICE.Text = GameLauncherCore.Localization.Get (LocalizationID.TERMS_OF_SERVICE);
            PRIVACY_POLICY.Text = GameLauncherCore.Localization.Get (LocalizationID.PRIVACY_POLICY);


            // Update AppLaunchAction ComboBox Language
            ComboBox_OnAppLaunchAction_KeepOpen.Content = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Open);
            ComboBox_OnAppLaunchAction_Minimize.Content = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Minimize);
            ComboBox_OnAppLaunchAction_Close.Content = GameLauncherCore.Localization.Get (LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Close);
        }

        int GetSelectedLanguageIndex(string language)
        {

            switch (language)
            {
                case "en_US":
                    //GameLauncherCore.Localization.SetLanguage ("en_US");
                    return 0;
                case "es_MX":
                    //GameLauncherCore.Localization.SetLanguage ("es_MX");
                    return 1;
                case "tr_TR":
                    //GameLauncherCore.Localization.SetLanguage ("tr_TR");
                    return 2;
                default:
                    //GameLauncherCore.Localization.SetLanguage ("en_US");
                    return 0;
            }
        }

        private void Button_CloseSettings_Click (object sender, RoutedEventArgs e)
        {
            Close ();
        }

        #region TABS LOGIC
        private void RadioButton_Click (object sender, RoutedEventArgs e)
        {
            if (sender == LauncherButton)
            {
                tabControl.SelectedIndex = 0;
            }
            else if (sender == DownloadsButton)
            {
                tabControl.SelectedIndex = 1;
            }
            else if (sender == AboutButton)
            {
                tabControl.SelectedIndex = 2;
            }
        }
        #endregion

        #region Launcher Tab

        #region Downloads Tab
        private void ChangeLocationButton_MouseDown (object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog ();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog ();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string gamePath = dialog.SelectedPath;


                if (!gamePath.EndsWith (MainWindow.MAINAPP_SUBDIRECTORY))
                {
                    gamePath = System.IO.Path.Combine (dialog.SelectedPath, MainWindow.MAINAPP_SUBDIRECTORY);
                }

                LocationPath.Text = gamePath;

                SettingsManager.Settings.CustomInstallFolder = gamePath;

                SettingsManager.SaveSettings ();
            }
        }

        #endregion 

        #endregion

        #region About Tab
        private void TERMS_OF_SERVICE_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            // Open Terms of Service Link
            Utility.OpenURL (mainWindow.TERMS_OF_SERVICE_URL);

        }
        private void PRIVACY_POLICY_MouseDown (object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            // Open Privacy Policy Link
            Utility.OpenURL (mainWindow.PRIVACY_POLICY_URL);
        }
        private void GAME_LAUNCHER_MouseDown (object sender, MouseButtonEventArgs e)
        {
            Utility.OpenURL ("https://assetstore.unity.com/packages/slug/217526");
        }

        #endregion

        private void SetLanguage_en_US_MouseDown (object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ChangeUILanguage ("en_US");
            ComboBox_Language.SelectedIndex = GetSelectedLanguageIndex (SettingsManager.Settings.Language);

            RefreshLanguage ();
        }

        private void SetLanguage_es_MX_MouseDown (object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ChangeUILanguage ("es_MX");
            ComboBox_Language.SelectedIndex = GetSelectedLanguageIndex (SettingsManager.Settings.Language);

            RefreshLanguage ();
        }

        private void checkBox_UseDefaultInstallLocation_Click (object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            SettingsManager.Settings.UseDefaultInstallLocation = (bool)checkBox.IsChecked;
            SettingsManager.SaveSettings ();

            settings_Downloads_CustomInstallLocation.Visibility = (bool)checkBox.IsChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SettingsWindow_Closed (object sender, EventArgs e)
        {
            SettingsManager.Settings.OnAppLaunchAction = ComboBox_OnAppLaunchAction.SelectedIndex;
            SettingsManager.SaveSettings ();
        }
    }
}
