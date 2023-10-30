using GameLauncher.Views;
using System.Windows;
using System.Windows.Controls;

namespace GameLauncher.Controls
{

    public partial class GameSettingsDropdown : UserControl
    {
        public GameSettingsDropdown ()
        {
            InitializeComponent ();
            RefreshLanguage ();
        }

        public void RefreshLanguage()
        {
            Option_Repair.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettingsDropdown_Repair);
            Option_ShowInExplorer.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettingsDropdown_ShowInExplorer);
            Option_GameSettings.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettingsDropdown_GameSettings);
            Option_LauncherSettings.Text = GameLauncherCore.Localization.Get (GameLauncherCore.LocalizationID.GameSettingsDropdown_LauncherSettings);
        }

        private void Repair_Click (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.RepairButtonClicked ();
            ClosePopup ();
        }

        private void ShowInExplorer_Click (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ShowGameInExplorerClicked ();

            ClosePopup ();
        }

        private void GameSettings_Click (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ShowGameSettingsClicked ();
            

            ClosePopup ();
        }

        private void ClosePopup ()
        {
            ConfigPopup.IsOpen = false;
        }

        private void AppConfigButton_Click (object sender, RoutedEventArgs e)
        {
            ConfigPopup.IsOpen = true;
        }

        private void LauncherSettings_Click (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.OpenOrCloseSettingsCanvas ();
            ClosePopup ();
        }
    }
}
