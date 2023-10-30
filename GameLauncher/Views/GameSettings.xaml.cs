using GameLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for GameSettings.xaml
    /// </summary>
    public partial class GameSettings : Window
    {
        public GameInfo gameInfo;

        public GameSettings (ref GameInfo GameInfo)
        {
            InitializeComponent ();
            Closed += GameSettingsWindow_Closed;

            gameInfo = GameInfo;

            UpdateInfo ();

            RefreshLanguage ();
        }

        public void RefreshLanguage ()
        {
            lbl_InstallLocation.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettings_InstallLocation);
            lbl_ChangeLocation.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.Settings_DownloadsSettings_ChangeLocation);
            checkBox_EnableAutomaticUpdates.Content = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettings_EnableAutomaticUpdates);
            checkBox_UseAdditionalLaunchParameters.Content = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettings_UseAdditionalLaunchParameters);
        }

        public void UpdateInfo ()
        {

            Title.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.Settings);
            checkBox_EnableAutomaticUpdates.IsChecked = gameInfo.AutomaticUpdates;
            checkBox_UseAdditionalLaunchParameters.IsChecked = gameInfo.UseAdditionalLaunchArgs;
            AdditionalLaunchParameters.Text = gameInfo.AdditionalLaunchArgs;

            if (Directory.Exists (gameInfo.InstallPath))
            {
                LocationPath.Text = gameInfo.InstallPath;
            } else
            {
                LocationPath.Text = gameInfo.DefaultInstallPath;
            }
        }

        private void CloseWindow (object sender, RoutedEventArgs e)
        {
            this.Close ();
        }

        private void DoneButton_MouseDown (object sender, RoutedEventArgs e)
        {
            //if (!HasRequiredDiskSpace ())
            //{
            //    MessageBox.Show ("No enought disk space");
            //    return;
            //}


            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            gameInfo.AdditionalLaunchArgs = AdditionalLaunchParameters.Text;
            mainWindow.SaveGamesInfo ();

            this.Close ();
        }

        //private void ChangeLocationButton_MouseDown (object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog ();
        //    System.Windows.Forms.DialogResult result = dialog.ShowDialog ();

        //    if (result == System.Windows.Forms.DialogResult.OK)
        //    {
        //        string gamePath = dialog.SelectedPath;

        //        if (!gamePath.EndsWith (gameInfo.Title))
        //        {
        //            gamePath = System.IO.Path.Combine (dialog.SelectedPath, gameInfo.Title);
        //        }

        //        LocationPath.Text = gamePath;

        //        gameInfo.InstallPath = gamePath;

        //        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        //        mainWindow.SaveGamesInfo ();
        //    }
        //}

        private void GameSettingsWindow_Closed (object sender, EventArgs e)
        {
            gameInfo.AutomaticUpdates = (bool)checkBox_EnableAutomaticUpdates.IsChecked;
            gameInfo.UseAdditionalLaunchArgs = (bool)checkBox_UseAdditionalLaunchParameters.IsChecked;
            gameInfo.AdditionalLaunchArgs = AdditionalLaunchParameters.Text;

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.SaveGamesInfo ();
        }
    }
}
