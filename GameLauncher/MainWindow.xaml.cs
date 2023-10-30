using GameLauncher.Controls;
using GameLauncher.Models;
using GameLauncher.NewsInformation;
using GameLauncher.Views;
using GameLauncherCore;
using MyToolkit.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Localization = GameLauncherCore.Localization;

namespace GameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // UPDATE THIS

        /// <summary>
        /// The HOST_URL is where your launcher, news, and app data and patches are located.
        /// It should be public available, and must end with '/'
        /// Remote host starts with 'https://' or 'http://'
        /// Local host must start with 'file:///'
        /// </summary>
        const string HOST_URL = "https://game-launcher.net/GameLauncher/"; // Already working HOST_URL: https://game-launcher.net/GameLauncher/

        // This is the name of the folder where your app is located
        public static string MAINAPP_SUBDIRECTORY = "App";

        // This is the name of the app to execute
        public string CURRENT_MAINAPP_EXECUTABLE = "MyApp.exe";
        /* Main app executable will be located at '{APPLICATION_DIRECTORY}/{CurrentSelectedEnvironment}/{MAINAPP_EXECUTABLE}' */

        // Show Release environment only?
        bool DisableBetaEnvironment = true;

        // Show Server Status? Set to 'false' if you don't need this
        readonly bool ShowServerStatus = true;

        // Users can play if the server status is offline?
        bool PlayIfServerIsOffline = true;

        // Allow multiple instances of the Launcher?
        bool AllowMultiplesInstances = false;

        /// <summary>
        /// Check for the version file only? | Or check all files if they are different?
        /// | true (default): only version number (e.g. 1.0) is compared against VersionInfo to see if there is an update
        /// | false: hashes and sizes of the local files are compared against VersionInfo (if there are any different/missing files, we'll patch the app)
        /// </summary>
        bool CheckVersionOnly = true;

        // The Launcher will check for updates automatically every X minutes
        int CheckForUpdatesInterval = 60; // Minutes

        #region PATCHER INIT - DO NOT CHANGE THIS
        // #WARNING - The information below this is updated automatically, DO NOT CHANGE THEM //
        // Patcher initialization 
        GameLauncherPatcher patcher;

        const string SELF_PATCHER_EXECUTABLE = "SelfPatcher.exe";
        /* Self patcher executable will be located at 
           '{APPLICATION_DIRECTORY}/{PatchParameters.SELF_PATCHER_DIRECTORY}/SelfPatcher.exe' */

        public string launcherDirectory;
        public string mainAppDirectory;
        public string selfPatcherPath;

        private PatcherAsyncListener patcherListener;

        private bool isPatchingLauncher;

        private delegate void InvokeDelegate ();

        private string rootPath;

        public string MY_ACCOUNT_URL = "";
        public string WEBPAGE_URL = "";
        public string PATCH_NOTES_URL = "";
        public string REPORT_BUG_URL = "";
        public string TERMS_OF_SERVICE_URL = "";
        public string PRIVACY_POLICY_URL = "";

        private NewsContent newsContent;
        public string NEWS_CURRENT_URL = "";

        // Get MainApp VersionInfo -> It will download a 'VersionInfo.info' file.
        public string MAINAPP_VERSIONINFO_URL = "URL";

        // Get Launcher VersionInfo -> It will download a 'VersionInfo.info' file.
        public string LAUNCHER_VERSIONINFO_URL = "URL";

        public string currentRegion = "-";
        public string currentAppVersion = "-";

        string currentLauncherVersion = "-";

        readonly List<string> ENVIRONMENT = new List<string> { "Release", "Beta" }; // Do not change the values

        public string CurrentSelectedEnvironment = "Release"; // This value represent a folder

        //CefSharp.Wpf.ChromiumWebBrowser _browser;
        // IntPtr _hWnd = IntPtr.Zero;

        List<Rectangle> slideShowRectanglesList = new List<Rectangle> ();

        bool isServerOnline = false;

        private Timer timer;
        private bool isFirstTimeCheckForUpdates = true;

        // BUG Fix
        bool isFirstTimeLoadEnvironment = false;

        MouseButtonEventHandler mouseButtonEventHandler;

        // News elements
        int currentNewsElement = 0;

        bool isSlideshowPlaying = true;
        DispatcherTimer timerSlideShow;

        // Sub element items
        int createdSubNewsElements = 0;
        const int columns = 3;
        int actualColumn = 0;

        int actualRow = 0;

        List<SubNewsControl> subNewsControlList = new List<SubNewsControl> ();

        public string currentWindowTab = "Game";

        private LauncherStatus CurrentLauncherStatus;

        internal LauncherStatus Status
        {
            get => CurrentLauncherStatus;
            set
            {
                CurrentLauncherStatus = value;

            }
        }

        private void OnTimerCallback (object state)
        {
            RunOnMainThread (() => CheckForUpdates (true)); // Call automatically
        }

        private void RunOnMainThread (InvokeDelegate function)
        {
            // #FixedBug when closed
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { function(); }));
            }
        }

        public void SingleInstanceCheck ()
        {
            Process aProcess = Process.GetCurrentProcess ();
            string aProcName = aProcess.ProcessName;

            if (Process.GetProcessesByName (aProcName).Length > 1)
            {
                Close ();
            }
        }

        #endregion

        #region LAUNCHER INITIALIZATION
        public MainWindow ()
        {
            // Show splash screen for 0.25 seconds more after loading
            //System.Threading.Thread.Sleep (250);

            // Assign this window as the main window
            Application.Current.MainWindow = this;

            // Check if there is another instance of the launcher running
            if (AllowMultiplesInstances == false)
            {
                SingleInstanceCheck ();
            }

            // Initialize Program
            InitializeComponent ();

            Closed += Window_Closed;

            // Ensure images high quality
            RenderOptions.SetBitmapScalingMode (this, BitmapScalingMode.Fant);

            // Load last language
            Localization.SetLanguage (SettingsManager.Settings.Language);

            // Refresh UI Language
            RefreshUILanguage ();

            //// Get current window handle;
            //{
            //    _hWnd = new WindowInteropHelper (this).Handle;
            //}

            //// Create chrome browser.
            //// need to do this programatically because it's 
            //// bugging up when directly added in XAML
            //{
            //    _browser = new CefSharp.Wpf.ChromiumWebBrowser ();
            //    NewsSlideshow.News_Video.Children.Add (_browser);
            //}

            rootPath = Directory.GetCurrentDirectory ();

            // Assign on click interactions
            //ReportBug_Button.Click += (s, e) => ReportBugButtonClicked ();
            NewsSlideshow.imageElement.MouseDown += (s, e) => PlaySlideShow (true);
            // _browser.PreviewMouseDown += (s, e) => PlaySlideShow (true);

            InitializeLauncher ();

        }

        public void InitializeLauncher ()
        {

            // Set Launcher Main URL
            LAUNCHER_VERSIONINFO_URL = HOST_URL + "Launcher/" + "VersionInfo.info";

            // PATCHER //

            // Initialize GameLauncher
            patcher = new GameLauncherPatcher (rootPath, LAUNCHER_VERSIONINFO_URL);

            // Assign directorys
            launcherDirectory = System.IO.Path.GetDirectoryName (PatchUtils.GetCurrentExecutablePath ());
            selfPatcherPath = PatchUtils.GetDefaultSelfPatcherExecutablePath (SELF_PATCHER_EXECUTABLE);

            // Hide menus
            LoadingContent_ScrollViewer.Visibility = Visibility.Visible;

            Storyboard sb = FindResource ("NewsLoadingAnimation") as Storyboard;
            sb.Begin ();

            Right_Panel.Visibility = Visibility.Hidden;
            Alert_Menu.Visibility = Visibility.Hidden;
            //Settings_Canvas.Visibility = Visibility.Collapsed;

            // Hide default subnews
            NewsSlideshow.SlideShow_News1.Visibility = Visibility.Collapsed;
            NewsSlideshow.SlideShow_News2.Visibility = Visibility.Collapsed;
            NewsSlideshow.SlideShow_News3.Visibility = Visibility.Collapsed;

            if (DisableBetaEnvironment)
            {
                Dropdown_Environment.Items.RemoveAt (1);
            }

            // Hide/Clear Texts
            ClearTexts ();

            currentLauncherVersion = PatchUtils.GetCurrentAppVersion ();
            //launcherVersionLabel.Text = string.IsNullOrEmpty (currentLauncherVersion) ? "1.0.0" : (currentLauncherVersion);
            //LAUNCHER_VERSION_2.Content = string.IsNullOrEmpty (currentLauncherVersion) ? "1.0.0" : (currentLauncherVersion);

            currentAppVersion = patcher.NewVersion;
            UpdateRegionVersion ();

            // Get News / Changelog 
            // Refresh UI Language
            RefreshUILanguage ();

            // Set callbacks for listening patcher changes
            patcherListener = new PatcherAsyncListener ();

            // See the another status log in the Launcher
            // Uncomment this to get more details in StatusText
            patcherListener.OnLogReceived += (log) => {
                // Show every log
                // UpdateLabel (statusText, log);

                // Show only errors
                if (log.Contains ("ERROR"))
                {
                    UpdateLabel (statusText, log);
                    // Show message box 
                    //MessageBox.Show (log, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            };

            patcherListener.OnProgressChanged += (progress) =>
            {
                UpdateLabel (progressTextDetail, progress.ProgressInfo); // Use progress.ProgressNoFileInfo for less details
                //UpdateLabel (progressText, string.Format ("{0}%", progress.Percentage));
                UpdateProgressbar (singleProcessProgressBar, progress.Percentage);
                RunOnMainThread (() => SetLauncherStatus (LauncherStatus.downloadingUpdate));
            };
            patcherListener.OnOverallProgressChanged += (progress) =>
            {
                UpdateLabel (progressText, string.Format ("{0}%", progress.Percentage));
                UpdateProgressbar (overallProgressBar, progress.Percentage);
                RunOnMainThread (() => SetLauncherStatus (LauncherStatus.downloadingUpdate));
            };

            patcherListener.OnVersionInfoFetched += (versionInfo) =>
            {
                if (isPatchingLauncher)
                {
                    versionInfo.AddIgnoredPath (MAINAPP_SUBDIRECTORY + "/");
                }
            };
            patcherListener.OnVersionFetched += (currVersion, newVersion) =>
            {
                //if (isPatchingLauncher)
                    //UpdateLabel (launcherVersionLabel, currVersion);
            };

            patcherListener.OnFinish += () =>
            {
                if (patcher.Operation == PatchOperation.CheckingForUpdates)
                    CheckForUpdatesFinished ();
                else
                    PatchFinished ();
            };

            LoadGameInfo ();        
        }

        private void UpdateVisibility (UIElement element, Visibility visibility)
        {
            RunOnMainThread (() => element.Visibility = visibility);
        }

        private void UpdateLabel (TextBlock label, string text, bool updateTooltip = false)
        {
            //MessageBox.Show($"{text}");
            if (updateTooltip)
            {
                RunOnMainThread (() => label.ToolTip = text);
            }

            RunOnMainThread (() => label.Text = text);
        }

        #endregion

        #region WINDOW LOGIC
        private void ButtonMinimize_Click (object sender = null, RoutedEventArgs e = null)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void WindowStateButton_Click (object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        public void Window_Closed (object sender, EventArgs e)
        {
            SettingsManager.SaveSettings ();

            // Delete WebView2 cache data before application exits
            try
            {
                var webView = NewsSlideshow.WebView;

                string? webViewCacheDir = webView.CoreWebView2.Environment.UserDataFolder;
                var webViewProcessId = Convert.ToInt32(webView.CoreWebView2.BrowserProcessId);
                var webViewProcess = Process.GetProcessById(webViewProcessId);

                // Shutdown browser with Dispose, and wait for process to exit
                webView.Dispose();
                webViewProcess.WaitForExit(3000);

                Directory.Delete(webViewCacheDir, true);
            }
            catch (Exception ex)
            {
                // Log warning
            }
        }

        private void ButtonClose_Click (object sender, RoutedEventArgs e)
        {
            Close ();
        }

        #endregion

        #region LOAD GAMES
        public GameInfo CurrentGame;

        private async void LoadGameInfo ()
        {

            CurrentGame = new GameInfo (); // Edit the details in the class

            // Load Game Local Info DB
            var loadedGameInfo = SettingsManager.LoadGameInfo ();

            if (loadedGameInfo != null)
            {
                // Load Launcher Info
                CurrentGame.IsInstalled = loadedGameInfo.IsInstalled;
                CurrentGame.InstallDate = loadedGameInfo.InstallDate;
                CurrentGame.LastPlayedDate = loadedGameInfo.LastPlayedDate;
                CurrentGame.AutomaticUpdates = loadedGameInfo.AutomaticUpdates;
                CurrentGame.DefaultLanguage = loadedGameInfo.DefaultLanguage;
                CurrentGame.IsFavorite = loadedGameInfo.IsFavorite;
                CurrentGame.AdditionalLaunchArgs = loadedGameInfo.AdditionalLaunchArgs;
            }

            // If the game is not link only, then check if it's installed
            if (CurrentGame.LinkOnly == false)
            {
                // Check if game is installed on 
                if (Directory.Exists (CurrentGame.InstallPath) || Directory.Exists (CurrentGame.DefaultInstallPath))
                {
                    CurrentGame.IsInstalled = true;
                }
                else
                {
                    CurrentGame.IsInstalled = false;
                }
            }

            OnSelectGameChanged (CurrentGame);
            StartLauncherPatch ();
        }

        /// <summary>
        /// OnSelectGameChange action. Changes the background, icons, colors. And the patcher points to this game.
        /// </summary>
        public async void OnSelectGameChanged (GameInfo gameInfo)
        {
            // Cancel any patch actions if not patching launcher
            if (!isPatchingLauncher)
            {
                patcher.Cancel ();
            }

            // Get install in default location
            string defaultGamePath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), MAINAPP_SUBDIRECTORY);

            // Check if a path / default is assigned
            if (string.IsNullOrEmpty (gameInfo.InstallPath) || gameInfo.InstallPath == defaultGamePath)
            {
                mainAppDirectory = System.IO.Path.Combine (launcherDirectory, MAINAPP_SUBDIRECTORY, CurrentSelectedEnvironment);
            }
            else
            {
                mainAppDirectory = gameInfo.InstallPath;
            }

            // Assign new game Host URL to check his updates
            MAINAPP_VERSIONINFO_URL = $"{HOST_URL}App/{CurrentSelectedEnvironment}/VersionInfo.info";

            // Change selected game
            CurrentGame = gameInfo;

            // UNCOMMENT IF YOU WANT TO LOAD THE DATA USING THE GAME INFO
            //// Load logo image
            //if (!string.IsNullOrEmpty (CurrentGame.LogoURL))
            //{
            //    Logo.Opacity = 0;
            //    Icon_Blur.Opacity = 0;

            //    BitmapImage bitmapImageLogo = new BitmapImage (new Uri (CurrentGame.LogoURL));
            //    bitmapImageLogo.DownloadCompleted += (sender, e) =>
            //    {
            //        Logo.CreateOpacityAnimation (0, 1, 1);
            //    };

            //    Logo.Source = bitmapImageLogo;

            //    if (!string.IsNullOrEmpty (CurrentGame.IconURL))
            //    {
            //        BitmapImage bitmapIconImage = new BitmapImage (new Uri (CurrentGame.IconURL));
            //        bitmapIconImage.DownloadCompleted += (sender, e) =>
            //        {
            //            Icon_Blur.CreateOpacityAnimation (0, 0.15f, 1);
            //        };

            //        Icon_Blur.Source = bitmapIconImage;
            //    }
            //    else
            //    {
            //        Icon_Blur.Source = bitmapImageLogo;
            //    }

            //    // Obtener el color principal desde SelectedGame.ColorHEX
            //    Color mainColor = (Color)ColorConverter.ConvertFromString (CurrentGame.ColorHEX);
            //    mainColor = Color.FromArgb (90, mainColor.R, mainColor.G, mainColor.B);

            //    // Crear los colores secundarios con 10% y 0% de opacidad
            //    Color secondColor = Color.FromArgb (26, mainColor.R, mainColor.G, mainColor.B);
            //    Color thirdColor = Color.FromArgb (0, mainColor.R, mainColor.G, mainColor.B);

            //    // Crear el RadialGradientBrush con los GradientStop
            //    RadialGradientBrush gradientBrush = new RadialGradientBrush ();
            //    gradientBrush.GradientOrigin = new Point (0.125, 0.05);
            //    gradientBrush.Center = new Point (0, 1);
            //    gradientBrush.RadiusX = 0.7;
            //    gradientBrush.RadiusY = 0.7;
            //    gradientBrush.GradientStops.Add (new GradientStop (Colors.Black, 0));
            //    gradientBrush.GradientStops.Add (new GradientStop (mainColor, 0));
            //    gradientBrush.GradientStops.Add (new GradientStop (secondColor, 0.35));
            //    gradientBrush.GradientStops.Add (new GradientStop (thirdColor, 1));

            //    Blur.Opacity = 1;
            //    Background_Blur.CreateOpacityAnimation (0, 1, 1);

            //    Background_Blur.Fill = gradientBrush;
            //}

            // Save Settings
            SettingsManager.Settings.LastSelectedGame = CurrentGame.Title;
            SettingsManager.SaveSettings ();

            // Fetch News
            FetchNews ();

            // Apply the new launcher status only if we are not patching the launcher
            UpdateSelectedGameStatus (); 
        }

        /// <summary>
        /// Updates the selected game status play button
        /// </summary>
        public void UpdateSelectedGameStatus ()
        {

            if (isPatchingLauncher || CurrentGame == null)
            {
                return;
            }


            if (CurrentGame.LinkOnly == true)
            {
                SetLauncherStatus (LauncherStatus.isLinkOnly);
                return;
            }

            if (CurrentGame.IsInstalled && (patcher.Result == PatchResult.AlreadyUpToDate || patcher.Result == PatchResult.Success))
            {
                SetLauncherStatus (LauncherStatus.play);

            }
            else if (!CurrentGame.IsInstalled)
            {
                SetLauncherStatus (LauncherStatus.requireInstall);
            }
            else
            {
                SetLauncherStatus (LauncherStatus.readyToUpdate);
            }

        }

        #endregion

        void UpdateRegionVersion ()
        {
            if (currentRegion == "-")
            {
                UpdateLabel(lbl_RegionVersion, string.Format (GameLauncherCore.Localization.Get (LocalizationID.MainUI_Version), currentAppVersion));
            }
            else
            {
                UpdateLabel (lbl_RegionVersion, string.Format (GameLauncherCore.Localization.Get (LocalizationID.MainUI_RegionVersion), currentRegion, currentAppVersion));
            }
        }

        /// <summary>
        /// Refresh the UI Language and all his texts
        /// </summary>
        public void RefreshUILanguage ()
        {
            try
            {
                /// Main UI //
                // TopSide Options
                TopButton_MyAccount.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_TopOptions_MyAccount);
                TopButton_Forum.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_TopOptions_Forum);

                UpdateRegionVersion ();

                // Main Button
                // Bug Fix #2 - Commented because causes wrong Launcher Status
                //SetLauncherStatus (_status);
                if (CurrentLauncherStatus == LauncherStatus.play)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Play);
                }
                else if (CurrentLauncherStatus == LauncherStatus.failed)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Retry);

                }
                else if (CurrentLauncherStatus == LauncherStatus.downloadingUpdate)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Downloading);

                }
                else if (CurrentLauncherStatus == LauncherStatus.patching)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Patching);

                }
                else if (CurrentLauncherStatus == LauncherStatus.checking)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Checking);
                }
                else if (CurrentLauncherStatus == LauncherStatus.readyToUpdate)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_ReadyToUpdate);
                }
                else if (CurrentLauncherStatus == LauncherStatus.requireInstall)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Install);
                }
                else if (CurrentLauncherStatus == LauncherStatus.isLinkOnly)
                {
                    PlayButtonText.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_IsLinkOnly);
                }

                // Links
                // Webpage
                lbl_Webpage.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_Links_Webpage);
                lbl_PatchNotes.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_Links_PatchNotes);

                // Environment
                lbl_EnvironmentTitle.Text = GameLauncherCore.Localization.Get (LocalizationID.MainUI_EnvironmentTitle);

                // Error on Load News
                ReloadAll_ErrorLabel.Content = Localization.Get (LocalizationID.ErrorAtGetNews);

                // Refresh
                RefreshAll_lbl_Reload.Content = Localization.Get (LocalizationID.Refresh);

                GameSettingsDropdownButton.RefreshLanguage ();

                FetchNews ();
            }
            catch { }
        }

        /// <summary>
        /// Sets the current launcher status, and hides other variables or content depending on the status
        /// </summary>
        /// <param name="status"></param>
        void SetLauncherStatus (LauncherStatus status)
        {

            CurrentLauncherStatus = status;

            // Remove Previous Click Events
            Utility.RemoveRoutedEventHandlers (PlayButton, Button.ClickEvent);
            UpdateVisibility (lbl_RegionVersion, Visibility.Collapsed);

            switch (status)
            {
                case LauncherStatus.play:
                    UpdateLabel(statusText, "");
                    UpdateLabel (progressTextDetail, "");
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Play));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);

                    UpdateRegionVersion ();

                    UpdateVisibility (lbl_RegionVersion, Visibility.Visible);

                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Visible);

                    UpdateVisibility(singleProcessProgressBar, Visibility.Hidden);
                    UpdateVisibility (singleProcessProgressBar_Background, Visibility.Hidden);


                    UpdateVisibility (overallProgressBar, Visibility.Hidden);
                    UpdateVisibility (overallProgressBar_Background, Visibility.Hidden);

                    UpdateVisibility (progressText, Visibility.Hidden);

                    UpdateProgressbar (singleProcessProgressBar, 0);
                    UpdateProgressbar (overallProgressBar, 0);
                    UpdateLabel (progressText, "");

                    PlayButton.Click += (s, e) => PlayButtonClicked ();
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 0, 194, 203)));

                    if (PlayIfServerIsOffline)
                    {
                        RunOnMainThread (() => PlayButton.IsEnabled = true);
                    }
                    else
                    {
                        RunOnMainThread (() => PlayButton.IsEnabled = false);
                    }
                    currentAppVersion = patcher.NewVersion;
                    UpdateRegionVersion ();

                    break;
                case LauncherStatus.failed:
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Retry));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Visible);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    PlayButton.Click += (s, e) => PatchNow ();
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 120, 0, 0)));
                    RunOnMainThread (() => PlayButton.IsEnabled = true);
                    break;
                case LauncherStatus.downloadingUpdate:
                    UpdateLabel(statusText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_DownloadingUpdate));
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Downloading));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Collapsed);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromRgb (0, 79, 120)));

                    RunOnMainThread (() => PlayButton.IsEnabled = false);
                    break;
                case LauncherStatus.patching:
                    UpdateLabel (statusText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Patching));
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Patching));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Collapsed);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (32, 204, 0, 204)));
                    RunOnMainThread (() => PlayButton.IsEnabled = false);
                    break;
                case LauncherStatus.checking:
                    UpdateLabel (statusText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Checking));
                    UpdateLabel(PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Checking));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Collapsed);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (32, 201, 192, 0)));
                    RunOnMainThread (() => PlayButton.IsEnabled = false);
                    break;
                case LauncherStatus.readyToUpdate:
                    UpdateLabel (statusText, "");
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_ReadyToUpdate));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Visible);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);
                    UpdateRegionVersion ();


                    UpdateVisibility (lbl_RegionVersion, Visibility.Collapsed);

                    PlayButton.Click += (s, e) => PatchNow ();
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 0, 194, 203)));
                    RunOnMainThread (() => PlayButton.IsEnabled = true);
                    break;
                case LauncherStatus.requireInstall:
                    UpdateLabel (statusText, "");
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Install));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Visible);

                    PlayButton.Click += (s, e) => { RepairButtonClicked (); };
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 0, 194, 203)));
                    RunOnMainThread (() => PlayButton.IsEnabled = true);
                    break;
                case LauncherStatus.isLinkOnly:
                    UpdateLabel (statusText, "");
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_IsLinkOnly));
                    UpdateVisibility (PlayButtonImage, Visibility.Visible);
                    UpdateVisibility (lbl_RegionVersion, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Collapsed);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    PlayButton.Click += (s, e) => Utility.OpenURL(WEBPAGE_URL);
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 0, 194, 203)));
                    RunOnMainThread (() => PlayButton.IsEnabled = true);
                    break;
                default:
                    UpdateLabel (PlayButtonText, GameLauncherCore.Localization.Get (LocalizationID.MainUI_MainButtonState_Retry));
                    UpdateVisibility (PlayButtonImage, Visibility.Collapsed);
                    UpdateVisibility (GameSettingsDropdownButton, Visibility.Collapsed);
                    UpdateVisibility (AlreadyInstalledContent, Visibility.Collapsed);

                    PlayButton.Click += (s, e) => PatchNow ();
                    RunOnMainThread (() => PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 120, 0, 0)));
                    RunOnMainThread (() => PlayButton.IsEnabled = true);
                    break;
            }
        }

        /// <summary>
        /// Patch button action (PlayButton uses this and PlayButtonAction)
        /// </summary>
        public async void PatchNow ()
        {
            if (patcher != null && !patcher.IsRunning)
            {

                // Initalize patcher

                if (StartLauncherPatch ())
                {
                    StartMainAppPatch (false);
                }
            }



            //if (patcher != null && !patcher.IsRunning)
            //    ExecutePatch ();
        }

        /// <summary>
        /// Repair button action
        /// </summary>
        public void RepairButtonClicked (object sender = null, MouseButtonEventArgs e = null)
        {
            StartMainAppPatch (false);
        }

        /// <summary>
        /// Play button action (PlayButton uses this and PlayButtonAction)
        /// </summary>
        public void PlayButtonClicked ()
        {
            if (patcher != null && patcher.IsRunning && patcher.Operation != PatchOperation.CheckingForUpdates)
                return;


            // Get the current game app to execute
            FileInfo mainApp = new FileInfo (System.IO.Path.Combine (mainAppDirectory, CURRENT_MAINAPP_EXECUTABLE));

            // Check if exists
            if (mainApp.Exists)
            {

                // Use default launch args
                string launchArgs = CurrentGame.DefaultLaunchArgs;

                // Add additional launch args
                if (CurrentGame.UseAdditionalLaunchArgs) {
                    launchArgs += " " + CurrentGame.AdditionalLaunchArgs;
                }

                // Execute app
                Process.Start (new ProcessStartInfo (mainApp.FullName)
                { WorkingDirectory = mainApp.DirectoryName,
                  Arguments = launchArgs
                });

                // Stop slideshow
                PlaySlideShow (false);

                // Save the last played time
                CurrentGame.LastPlayedDate = DateTime.Now;

                // Save games info
                SaveGamesInfo ();

                // On AppLaunch Action
                switch (SettingsManager.Settings.OnAppLaunchAction)
                {
                    case 1:
                        ButtonMinimize_Click ();
                        break;
                    case 2:
                        Close ();
                        break;
                    default:
                        // Keep open
                        break;
                }



  
            }
            else // The file to open doesn't exists
            {
                UpdateLabel (statusText, GameLauncherCore.Localization.Get (LocalizationID.E_XDoesNotExist, CURRENT_MAINAPP_EXECUTABLE));
                SetLauncherStatus (LauncherStatus.failed);
            }
        }

        /// <summary>
        /// Show current game in explorer
        /// </summary>
        public void ShowGameInExplorerClicked (object sender = null, MouseButtonEventArgs e = null)
        {
            // Try to open the install path of the game
            
            // Check if install path exists
            if (Directory.Exists (CurrentGame.InstallPath))
            {
                Utility.OpenURL (CurrentGame.InstallPath);

            } else if (Directory.Exists (System.IO.Path.Combine (Directory.GetCurrentDirectory(), MAINAPP_SUBDIRECTORY)))
            {
                // Open default game path
                string defaultGamePath = System.IO.Path.Combine (Directory.GetCurrentDirectory(), MAINAPP_SUBDIRECTORY);
                Utility.OpenURL (defaultGamePath);
            } else // Just open the ap directory
            {
                // Open launcher path
                Utility.OpenURL (Directory.GetCurrentDirectory());
            }

        }

        /// <summary>
        /// Show current game settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowGameSettingsClicked (object sender = null, EventArgs e = null)
        {
            var gameSettings = new GameSettings (ref CurrentGame);
            gameSettings.ShowDialog ();
        }

        /// <summary>
        /// Report button action
        /// </summary>
        public void ReportBugButtonClicked ()
        {
            // Open link
            Utility.OpenURL (REPORT_BUG_URL);
        }

        /// <summary>
        /// Starts the launcher patch action
        /// </summary>
        /// <returns>Returns true if the action was completed correctly</returns>
        public bool StartLauncherPatch ()
        {
            if (string.IsNullOrEmpty (LAUNCHER_VERSIONINFO_URL) || LAUNCHER_VERSIONINFO_URL == "URL")
            {
                UpdateLabel (statusText, "Please set a valid URL to LAUNCHER_VERSIONINFO_URL");

                SetLauncherStatus (LauncherStatus.failed);

                return false;
            }

            if (patcher != null && patcher.IsRunning)
                return false;

            isPatchingLauncher = true;

            patcher = new GameLauncherPatcher (launcherDirectory, LAUNCHER_VERSIONINFO_URL).SetListener (patcherListener);
            CheckForUpdates (true);

            return true;
        }

        /// <summary>
        /// Starts the app patch action
        /// </summary>
        /// <param name="checkForUpdates"></param>
        /// <returns>Returns true if the action was completed correctly</returns>
        public async Task<bool> StartMainAppPatch (bool checkForUpdates)
        {
            if (string.IsNullOrEmpty (MAINAPP_VERSIONINFO_URL) || MAINAPP_VERSIONINFO_URL == "URL")
            {
                UpdateLabel (statusText, "Please set a valid URL to MAINAPP_VERSIONINFO_URL");
                SetLauncherStatus (LauncherStatus.failed);
                return false;
            }

            if (patcher != null && patcher.IsRunning)
                return false;

            isPatchingLauncher = false;

            patcher = new GameLauncherPatcher (mainAppDirectory, MAINAPP_VERSIONINFO_URL).SetListener (patcherListener);

            if (checkForUpdates)
                CheckForUpdates (CheckVersionOnly);
            else
                ExecutePatch ();

            return true;
        }


        /// <summary>
        /// Check for updates action.
        /// | true (default): only version number (e.g. 1.0) is compared against VersionInfo to see if there is an update
        /// | false: hashes and sizes of the local files are compared against VersionInfo (if there are any different/missing files, we'll patch the app)
        /// </summary>
        /// <param name="checkVersionOnly"></param>
        private void CheckForUpdates (bool checkVersionOnly)
        {
            if (patcher.CheckForUpdates (checkVersionOnly))
            {
                SetLauncherStatus (LauncherStatus.checking);
            }

            if (isFirstTimeCheckForUpdates)
            {
                isFirstTimeCheckForUpdates = false;
                timer = new Timer (OnTimerCallback, null, TimeSpan.FromMinutes (CheckForUpdatesInterval), TimeSpan.FromMinutes (CheckForUpdatesInterval));
            }
        }

        /// <summary>
        /// Execute patch action
        /// </summary>
        private void ExecutePatch ()
        {
            if (patcher.Operation == PatchOperation.ApplyingSelfPatch)
            {
                RunOnMainThread (() => SetLauncherStatus (LauncherStatus.patching));
                ApplySelfPatch ();
            }
            else if (patcher.Run (isPatchingLauncher))
            {
                RunOnMainThread (() => SetLauncherStatus (LauncherStatus.patching));
            }
        }

        /// <summary>
        /// Apply self patch action
        /// </summary>
        private void ApplySelfPatch ()
        {
            patcher.ApplySelfPatch (selfPatcherPath, PatchUtils.GetCurrentExecutablePath ());
        }

        /// <summary>
        /// Check for updates. If the launcher is already up to date, check for updates in the app.
        /// </summary>
        private void CheckForUpdatesFinished ()
        {
            // MessageBox.Show ($"Failed: {patcher.Result.ToString()}");

            if (patcher.Result == PatchResult.AlreadyUpToDate)
            {
        
                // If the launcher is already up to date, check for updates in the app
                if (isPatchingLauncher)
                {
                    isPatchingLauncher = false;

                    if (CurrentGame == null)
                    {
                        return;
                    }

                    if (CurrentGame.IsInstalled && CurrentGame.AutomaticUpdates)
                    {
                        StartMainAppPatch (false);
                    }
                    else if (CurrentGame.IsInstalled)
                    {
                        RunOnMainThread (() =>
                        {
                            StartMainAppPatch (true);
                            //SetLauncherStatus (LauncherStatus.readyToUpdate);
                        });
                    } else
                    {
                        RunOnMainThread (() =>
                        {
                            SetLauncherStatus (LauncherStatus.requireInstall);
                        });
                    }
                } else // If app is up to date, change the button status
                {
                    RunOnMainThread (() =>
                    {
                        // SetLauncherStatus (LauncherStatus.play);
                        UpdateSelectedGameStatus ();

                        // Create a desktop icon?
                        //CreateDesktopIcon ();
                    });
                }

            }
            else if (patcher.Result == PatchResult.Success) // Update available
            {

                if (isPatchingLauncher) {
                    ExecutePatch ();
                    return;
                }

                if (CurrentGame == null)
                {
                    return;
                }

                // Apply update automatically?
                //if (UpdateAutomatically || !SelectedGame.IsInstalled) // Using global settings
                if (CurrentGame.AutomaticUpdates && CurrentGame.IsInstalled) // Using game settings
                {
                    PatchNow ();
                }
                else
                {

                    RunOnMainThread (() =>
                    {
                        // SetLauncherStatus (LauncherStatus.play);
                        UpdateSelectedGameStatus ();

                        // Create a desktop icon?
                        //CreateDesktopIcon ();
                    });
                }

            }
            else
            {
                // An error occurred, user can click the Patch button to try again
                //ButtonSetEnabled (patchButton, true);
                //MessageBox.Show ("Failed");
                RunOnMainThread (() => SetLauncherStatus (LauncherStatus.failed));
                //PatchButtonClicked ();
            }
        }

        /// <summary>
        /// Patch finished actions.
        /// </summary>
        private void PatchFinished ()
        {
            //ButtonSetEnabled (PlayButton, true);
            //MessageBox.Show ("Failed");
            if (patcher.Result == PatchResult.AlreadyUpToDate)
            {
                // If launcher is already up-to-date
                if (isPatchingLauncher)
                {
                    isPatchingLauncher = false;
                }

                RunOnMainThread (() =>
                {
                    // SetLauncherStatus (LauncherStatus.play);
                    UpdateSelectedGameStatus ();

                    // Create a desktop icon?
                    //CreateDesktopIcon ();
                });
            }
            else if (patcher.Result == PatchResult.Success) // Update success
            {
                // If patcher was self patching the launcher, start the self patcher executable
                // Otherwise, we have just updated the main app successfully
                if (patcher.Operation == PatchOperation.SelfPatching)
                {
                    ApplySelfPatch ();
                }
                else
                {

                    CurrentGame.IsInstalled = true;

                    RunOnMainThread (() =>
                    {
                        //SetLauncherStatus (LauncherStatus.play);
                        UpdateSelectedGameStatus ();

                        // Create a desktop icon?
                        //CreateDesktopIcon ();
                    });
                }
            }
            else
            {
                // An error occurred, user can click the Patch button to try again
                // ButtonSetEnabled (patchButton, true);
                //PlayButton.Background = new SolidColorBrush (System.Windows.Media.Color.FromArgb (255, 0, 120, 49));
                //MessageBox.Show ("Failed");
                RunOnMainThread (() =>
                {
                    SetLauncherStatus (LauncherStatus.failed);
                });

            }
        }

        /// <summary>
        /// Creates a desktop icon
        /// </summary>
        public void CreateDesktopIcon ()
        {
            // File path to the game executable
            string filePath = CurrentGame.InstallPath;

            // Get the path to the public desktop folder
            string publicDesktopPath = Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory);

            // Create a FileInfo object for the shortcut file
            FileInfo shortcutFileInfo = new FileInfo (System.IO.Path.Combine (publicDesktopPath, $"{CurrentGame.Title}.lnk"));

            // Download the icon from the URL and save it to a temporary file
            string iconFilePath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), $"{CurrentGame.Title}.ico");
            using (WebClient client = new WebClient ())
            {
                client.DownloadFile (CurrentGame.IconURL, iconFilePath);
            }


            // Create a StreamWriter to write to the shortcut file
            using (StreamWriter writer = new StreamWriter (shortcutFileInfo.FullName))
            {
                // Write the contents of the shortcut file
                writer.WriteLine ("[InternetShortcut]");
                writer.WriteLine ($"URL=file:///{filePath.Replace ('\\', '/')}");
                writer.WriteLine ($"IconIndex=0");
                writer.WriteLine ($"IconFile={iconFilePath}");
                writer.Flush ();
            }

            //MessageBox.Show ("Created Desktop Icon");
        }

        /// <summary>
        /// Fetch the current news based in the current language and selected environment
        /// </summary>
        private async void FetchNews ()
        {

            if (CurrentGame == null)
            {
                // No valid selected game
                //MessageBox.Show (en_US_RELEASE_NEWS_URL, "SELECTED GAME NOT VALID");
                return;
            }

            LoadingContent_ScrollViewer.Opacity = 0;
            //LoadingContent_ScrollViewer.BeginAnimation(OpacityProperty, Utility.CreateOpacityAnimation (0, 0.75f, 1, 0.5f));

            Right_Panel.Visibility = Visibility.Visible;

            // Begin news skeleton animation
            Storyboard sb = FindResource ("NewsLoadingAnimation") as Storyboard;
            sb.Begin ();

            // Hide news containers
            //NewsContent_ScrollViewer.Visibility = Visibility.Collapsed;
            LoadingContent_ScrollViewer.Visibility = Visibility.Visible;
            RefreshAll_Container.Visibility = Visibility.Collapsed;

            // Clear current lists
            slideShowRectanglesList.Clear ();
            subNewsControlList.Clear ();

            if (NewsSlideshow.SlideShow_MiniButtonList != null)
            {
                NewsSlideshow.SlideShow_MiniButtonList.Children.RemoveRange (0, NewsSlideshow.SlideShow_MiniButtonList.Children.Count);
            }
            if (Content_Inferior != null)
            {
                Content_Inferior.Children.RemoveRange (0, Content_Inferior.Children.Count);
            }

            // Get Selected Game News by language and environment
            NEWS_CURRENT_URL = HOST_URL + $"News/{CurrentSelectedEnvironment}/{Localization.CurrentLanguageISOCode}_{CurrentSelectedEnvironment}_News.txt";

            Debug.WriteLine (new Uri (NEWS_CURRENT_URL).AbsoluteUri);

            string url = NEWS_CURRENT_URL;

            string content = await Utility.DownloadTextFileAsync (NEWS_CURRENT_URL);

            // Deserialize JSON
            if (!string.IsNullOrEmpty (content))
            {
                newsContent = JsonSerializer.Deserialize<NewsContent> (content, new JsonSerializerOptions () { PropertyNameCaseInsensitive = true });

                // Update SlideShow
                currentNewsElement = 0;

                // Load / Update News
                if (newsContent.News.Count == 0)
                {
                    // No news available
                    NewsSlideshow.Content_Superior.Visibility = Visibility.Collapsed;
                    NewsSlideshow.SlideShow_Content.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NewsSlideshow.Content_Superior.Visibility = Visibility.Visible;
                    NewsSlideshow.SlideShow_Content.Visibility = Visibility.Visible;

                    UpdateNewsElement (newsContent.News[currentNewsElement]);

                    // Create SlideShow Button List
                    for (int i = 0; i < newsContent.News.Count; i++)
                    {
                        //CreateNewsElement (newsContent.News[i]);
                        Rectangle slideShowButton = new Rectangle
                        {
                            Fill = new SolidColorBrush ((Color)ColorConverter.ConvertFromString ("#FFA4A4A4")),

                            RadiusX = 10,
                            RadiusY = 10,

                            Width = Math.Min (Math.Max (200 / newsContent.News.Count, 25), 100),
                            Height = 15,

                            Margin = new Thickness (0, 0, 5, 0),

                            Cursor = Cursors.Hand,

                            Style = FindResource ("SlideShowButtonsStyle") as Style
                        };

                        int index = i;

                        slideShowButton.MouseDown += (s, e) =>
                        {
                            if (newsContent.News.Count > 0)
                            {
                                UpdateNewsElement (newsContent.News[index]);
                            }

                            currentNewsElement = index;
                            SlideShow_RefillColor ();
                            PlaySlideShow (false);
                        };

                        //#FF0085FF
                        //#FFA4A4A4

                        NewsSlideshow.SlideShow_MiniButtonList.Children.Add (slideShowButton);

                        slideShowRectanglesList.Add (slideShowButton);
                    }

                    SlideShow_RefillColor ();

                    NewsSlideshow.SlideShow_Button.Click += SlideShow_Button_Click;

                    // Start SlideShow
                    SlideShow_RefillColor ();
                    PlaySlideShow (true);
                }

                // Update server status
                FetchServerStatus (newsContent);

                // Update alert
                FetchAlertNotification (newsContent);

                // Update URLS
                MY_ACCOUNT_URL = newsContent.MyAccountURL;
                WEBPAGE_URL = newsContent.WebpageURL;
                PATCH_NOTES_URL = newsContent.PatchNotesURL;
                TERMS_OF_SERVICE_URL = newsContent.TermsAndConditionsURL;
                PRIVACY_POLICY_URL = newsContent.PrivacyPolicyURL;
                MY_ACCOUNT_URL = newsContent.MyAccountURL;

                REPORT_BUG_URL = newsContent.ReportBugURL;

                // Create SubNews
                for (int i = 0; i < newsContent.SubNews.Count; i++)
                {
                    CreateSubNewsElement (newsContent.SubNews[i]);
                }

                LoadingContent_ScrollViewer.Visibility = Visibility.Hidden;
                Right_Panel.Visibility = Visibility.Visible;
                sb.Stop ();
            }
            else // Error
            {
                //RefreshAll_Container.Visibility = Visibility.Visible;
                //MessageBox.Show ("Error loading News JSON");
            }
        }

        /// <summary>
        /// Slideshow button play/pause action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlideShow_Button_Click (object sender, RoutedEventArgs e)
        {
            PlaySlideShow (!isSlideshowPlaying);
        }

        /// <summary>
        /// Set Slideshow play or pause state
        /// </summary>
        /// <param name="play"></param>
        private void PlaySlideShow (bool play)
        {
            if (timerSlideShow != null)
            {
                timerSlideShow.Stop ();
            }

            isSlideshowPlaying = play;

            if (!play)
            {
                NewsSlideshow.SlideShow_Button_Image.Source = new BitmapImage (new Uri (@"pack://application:,,,/Resources/Icons/Actions/action_play_white.png"));
            }
            else
            {
                NewsSlideshow.SlideShow_Button_Image.Source = new BitmapImage (new Uri (@"pack://application:,,,/Resources/Icons/Actions/action_pause_white.png"));

                timerSlideShow = new DispatcherTimer { Interval = TimeSpan.FromSeconds (10) };
                timerSlideShow.Start ();
                timerSlideShow.Tick += (sender, args) =>
                {
                    NextNewsElement ();
                };
            }
        }

        /// <summary>
        /// Refills the color in the slideshow
        /// </summary>
        private void SlideShow_RefillColor ()
        {
            foreach (Rectangle rectangle in slideShowRectanglesList)
            {
                rectangle.Fill = new SolidColorBrush ((Color)ColorConverter.ConvertFromString ("#FFA4A4A4"));
            }

            // Fill rectangle color
            if (currentNewsElement < slideShowRectanglesList.Count)
            {
                slideShowRectanglesList[currentNewsElement].Fill = new SolidColorBrush ((Color)ColorConverter.ConvertFromString ("#FF0085FF"));
            }

        }

        /// <summary>
        /// Show the next or previous element in the news slideshow. Next = false is equal to previous
        /// </summary>
        /// <param name="next"></param>
        private void NextNewsElement (bool next = true)
        {
            var exists = slideShowRectanglesList.ElementAtOrDefault (currentNewsElement) != null;

            if (!exists || newsContent.News.Count == 1)
            { return; }

            // Take out the color
            slideShowRectanglesList[currentNewsElement].Fill = new SolidColorBrush ((Color)ColorConverter.ConvertFromString ("#FFA4A4A4"));

            if (next)
            {
                // Next Element
                currentNewsElement++;

                // Set first element if the count was surpassed
                if (currentNewsElement >= newsContent.News.Count)
                {
                    currentNewsElement = 0;
                }
            }
            else
            {
                // Previous Element
                currentNewsElement--;

                // If less than zero, then go to the last element
                if (currentNewsElement < 0)
                {
                    currentNewsElement = newsContent.News.Count - 1;
                }
            }

            // Fill rectangle color
            slideShowRectanglesList[currentNewsElement].Fill = new SolidColorBrush ((Color)ColorConverter.ConvertFromString ("#FF0085FF"));

            // Update element
            UpdateNewsElement (newsContent.News[currentNewsElement]);
        }

        private async void UpdateNewsElement (News news)
        {

            Utility.CreateOpacityAnimation (NewsSlideshow.Content_Superior, 1, 0, 0.5f);

            await Task.Delay (500);

            // Show news content?
            if (news != null && news.showContent == true)
            {
                // Show video?
                if (!string.IsNullOrEmpty(news.videoURL) && news.showVideo == true)
                {
                    // Set the thumbnail image from the video
                    RunOnMainThread(() =>
                        {
                            var uri = new Uri(news.imagesURL[0], UriKind.Absolute);
                            var videoId = Utility.ExtractIDFromYoutubeURL(news.videoURL);

                            NewsSlideshow.ImageURL = Utility.GetVideoThumbnail(videoId);

                            // Load and show the video
                            ShowVideo(news.videoURL);
                        });


                } else // Show the default image and hide the video
                {
                    NewsSlideshow.ImageURL = news.imagesURL[0];
                    NewsSlideshow.WebView.Visibility = Visibility.Collapsed;
                }

                // Set interaction URL to the image if we have one
                if (!string.IsNullOrEmpty(news.interactionURL))
                {

                    // Only add one time
                    if (mouseButtonEventHandler == null)
                    {
                        mouseButtonEventHandler = (s, e) => { Utility.OpenURL(news.interactionURL); };
                        NewsSlideshow.imageElement.MouseDown += mouseButtonEventHandler;
                    }
                    NewsSlideshow.imageElement.Cursor = Cursors.Hand;
                }
                else
                {

                    // Disable interaction URL
                    if (mouseButtonEventHandler != null)
                    {
                        NewsSlideshow.imageElement.MouseDown -= mouseButtonEventHandler;
                    }

                    NewsSlideshow.imageElement.Cursor = null;
                }

                // Show Header
                if (news.showHeader == true)
                {
                    NewsSlideshow.News_Header.Visibility = Visibility.Visible;
                    UpdateLabel (NewsSlideshow.News_Header, news.header);
                }
                else
                {
                    NewsSlideshow.News_Header.Visibility = Visibility.Hidden;
                }

                // Show Title
                if (news.showTitle == true)
                {
                    NewsSlideshow.News_Actual_Title.Visibility = Visibility.Visible;
                    UpdateLabel (NewsSlideshow.News_Actual_Title, news.title);
                }
                else
                {
                    NewsSlideshow.News_Actual_Title.Visibility = Visibility.Collapsed;
                }

                // Show SubTitle
                if (news.showSubTitle == true)
                {
                    NewsSlideshow.News_Actual_SubTitle.Visibility = Visibility.Visible;

                    // Default #FF24FF00

                    // Set a color if a color is defined
                    try
                    {
                        if (!string.IsNullOrEmpty (news.subtitleColor))
                        {
                            NewsSlideshow.News_Actual_SubTitle.Foreground = new SolidColorBrush ((Color)ColorConverter.ConvertFromString (news.subtitleColor));
                        }
                    }
                    catch
                    {
                        // Error. The Color HEX code is not valid.
                        // Using default value
                    }

                    UpdateLabel (NewsSlideshow.News_Actual_SubTitle, news.subtitle);
                }
                else
                {
                    NewsSlideshow.News_Actual_SubTitle.Visibility = Visibility.Collapsed;
                }

                // Show Date
                if (news.showDate == true)
                {
                    NewsSlideshow.News_Actual_Date.Visibility = Visibility.Visible;
                    UpdateLabel (NewsSlideshow.News_Actual_Date, news.date.ToString ("MM/dd/yyyy"));
                }
                else
                {
                    NewsSlideshow.News_Actual_Date.Visibility = Visibility.Collapsed;
                }


                // Show Content
                UpdateLabel (NewsSlideshow.News_Actual_Content, news.content);

                // Show Button
                if (news.showButton == true)
                {
                    NewsSlideshow.News_Button.Visibility = Visibility.Visible;
                    UpdateLabel (NewsSlideshow.News_Button_Content, news.buttonContent);
                }
                else
                {
                    NewsSlideshow.News_Button.Visibility = Visibility.Collapsed;
                }

                // Update Actual News URL
                NEWS_CURRENT_URL = news.interactionURL;
            }

            else
            { // Hide content
                UpdateLabel (NewsSlideshow.News_Header, "");
                UpdateLabel (NewsSlideshow.News_Actual_Title, "");
                UpdateLabel (NewsSlideshow.News_Actual_SubTitle, "");
                UpdateLabel (NewsSlideshow.News_Actual_Date, "");
                UpdateLabel (NewsSlideshow.News_Actual_Content, "");
                UpdateLabel (NewsSlideshow.News_Button_Content, "");
                NewsSlideshow.News_LineSeparation.Visibility = Visibility.Collapsed;
                NewsSlideshow.News_Button.Visibility = Visibility.Collapsed;
            }

            Utility.CreateOpacityAnimation (NewsSlideshow.Content_Superior, 0, 1, 0.5f);

        }

        /// <summary>
        /// Create subnews element.
        /// </summary>
        /// <param name="subNews"></param>
        private void CreateSubNewsElement (SubNews subNews)
        {
            createdSubNewsElements++;

            // Create control
            SubNewsControl subNewsControl = new SubNewsControl ();

            // Assign URL and Cursor
            subNewsControl.MouseDown += (s, e) => Utility.OpenURL (subNews.interactionURL);
            subNewsControl.Cursor = Cursors.Hand;

            // Set Image
            subNewsControl.ImageURL = subNews.imageURL;

            // Set content
            subNewsControl.IsContentVisible = subNews.showContent == true ? true : false;

            subNewsControl.Title = subNews.title;
            subNewsControl.IsTitleVisible = subNews.showTitle == true ? true : false;

            subNewsControl.Text = subNews.content;
            subNewsControl.IsTextVisible = subNews.showContent == true ? true : false;

            subNewsControl.Date = subNews.date.ToString ("MM/dd/yyyy");
            subNewsControl.IsDateVisible = subNews.showDate == true ? true : false;

            if (actualRow == 2)
            {
                subNewsControl.Margin = new Thickness (0, 0, 0, 5);
            }

            actualColumn++;

            if (actualColumn > columns - 1)
            {
                actualColumn = 0;
                actualRow++;
                //Content_Inferior.RowDefinitions.Add (new RowDefinition ());
            }

            Content_Inferior.Children.Add (subNewsControl);

            subNewsControlList.Add (subNewsControl);
        }

        /// <summary>
        /// Fetch the server status
        /// </summary>
        /// <param name="newsContent"></param>
        private void FetchServerStatus (NewsContent newsContent)
        {

            if (newsContent == null)
            {
                // No valid News Content
                return;
            }

            if (ShowServerStatus)
            {
                ServerStatusText.Visibility = Visibility.Visible;
            }
            else
            {
                ServerStatusText.Visibility = Visibility.Hidden;
                return;
            }


            isServerOnline = false;

            if (newsContent.ServerStatus == "Online" || newsContent.ServerStatus == "online")
            {
                isServerOnline = true;
            }

            if (isServerOnline)
            {
                ServerStatusText.Text = string.Format ("Online", isServerOnline);
                ServerStatusText.Foreground = new SolidColorBrush (Colors.Lime);
            }
            else
            {
                ServerStatusText.Text = string.Format ("Offline", isServerOnline);
                ServerStatusText.Foreground = new SolidColorBrush (Colors.Red);
            }
        }

        /// <summary>
        /// Fetch the alert notification info.
        /// </summary>
        /// <param name="newsContent"></param>
        private void FetchAlertNotification (NewsContent newsContent)
        {
            // Check if valid
            if ((newsContent == null && newsContent.Alerts != null && newsContent.Alerts.Count > 0) || newsContent.Alerts.Count == 0)
            {
                // No valid News Content
                Alert_Menu.Visibility = Visibility.Hidden;
                return;
            }

            // Show after date?

            if (newsContent.Alerts[0].showAfterDate != null)
            {

                // It's the time to display it?
                if (DateTime.UtcNow < newsContent.Alerts[0].showAfterDate)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty (newsContent.Alerts[0].message))
            {
                AlertNotification_Text.Text = newsContent.Alerts[0].message;

                Uri uri = new Uri ("pack://application:,,,/Resources/Icons/Status/status_warning_black.png");
                Alert_Icon.Background = (SolidColorBrush)new BrushConverter ().ConvertFrom ("#FFFFAF00");

                if (newsContent.Alerts[0].type == "Information")
                {
                    uri = new Uri ("pack://application:,,,/Resources/Icons/Status/status_information_black.png");
                    Alert_Icon.Background = (SolidColorBrush)new BrushConverter ().ConvertFrom ("#FF00DCDC");
                }
                else if (newsContent.Alerts[0].type == "Warning")
                {
                    uri = new Uri ("pack://application:,,,/Resources/Icons/Status/status_warning_black.png");
                    Alert_Icon.Background = (SolidColorBrush)new BrushConverter ().ConvertFrom ("#FFFFAF00");
                }

                //ImageBrush myImageBrush = new ImageBrush (new BitmapImage (uri));

                // Configure the brush so that it
                // doesn't stretch its image to fill
                // the rectangle.
                //myImageBrush.Stretch = Stretch.UniformToFill;

                // Use the ImageBrush to paint the rectangle's background.
                Alert_Image.Source = new BitmapImage (uri);
                Alert_Image.Stretch = Stretch.UniformToFill;

                Alert_Menu.MouseDown += new MouseButtonEventHandler (Alert_Menu_Click);
                Alert_Menu.Cursor = Cursors.Hand;
                Alert_Menu.CreateOpacityAnimation (0, 1, 1);
                Alert_Menu.Visibility = Visibility.Visible;
            }
            else
            {
                AlertNotification_Text.Text = "";
                Alert_Menu.CreateOpacityAnimation (1, 0, 1);

            }
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="progressBar"></param>
        /// <param name="value"></param>
        private void UpdateProgressbar (ProgressBar progressBar, int value)
        {
            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            if (value > 0)
            {
                RunOnMainThread (() => overallProgressBar_Background.Visibility = Visibility.Visible);
                RunOnMainThread (() => singleProcessProgressBar_Background.Visibility = Visibility.Visible);
            }
            RunOnMainThread (() => progressBar.Value = value);
        }

        /// <summary>
        /// Clear Texts of GameLauncher
        /// </summary>
        private void ClearTexts ()
        {
            // Hide Texts
            UpdateLabel (progressTextDetail, string.Empty);
            UpdateLabel (NewsSlideshow.News_Actual_Content, Localization.Get (LocalizationID.MainUI_NoNewsAtTheMoment));
            UpdateLabel (statusText, string.Empty);
            UpdateVisibility (lbl_RegionVersion, Visibility.Collapsed);
            //UpdateLabel (launcherVersionLabel, string.Empty);

            // Progress Bar
            overallProgressBar_Background.Visibility = Visibility.Hidden;
            singleProcessProgressBar_Background.Visibility = Visibility.Hidden;
            UpdateLabel (progressText, string.Empty);
            singleProcessProgressBar.Value = 0;
            overallProgressBar.Value = 0;
        }

        /// <summary>
        /// Show Youtube Video
        /// </summary>
        public void ShowVideo (string URL)
        {

            if (NewsSlideshow.navigationCompleted)
            {
                NewsSlideshow.WebView.Visibility = Visibility.Visible;
            } else
            {
                string videoId = Utility.ExtractIDFromYoutubeURL(URL);

                string url = $"https://www.youtube.com/embed/{videoId}?autoplay=1";
                NewsSlideshow.WebView.Source = new Uri(url);

                // The Webview is set to Visible when the navigation is completed

            }

        }

        /// <summary>
        /// Change Launcher UI Language
        /// </summary>
        /// <param name="language"></param>
        public void ChangeUILanguage (string language)
        {
            switch (language)
            {
                case "en_US":
                    GameLauncherCore.Localization.SetLanguage ("en_US");
                    SettingsManager.Settings.Language = "en_US";
                    break;
                case "es_MX":
                    GameLauncherCore.Localization.SetLanguage ("es_MX");
                    SettingsManager.Settings.Language = "es_MX";
                    break;
                case "tr_TR":
                    GameLauncherCore.Localization.SetLanguage ("tr_TR");
                    SettingsManager.Settings.Language = "tr_TR";
                    break;
                default:
                    GameLauncherCore.Localization.SetLanguage ("en_US");
                    SettingsManager.Settings.Language = "en_US";
                    break;
            }


            RefreshUILanguage ();
        }

        /// <summary>
        /// Opens the explorer with the selected game root path folder
        /// </summary>
        public void ShowSelectedGameInExplorer()
        {
            if (Directory.Exists (rootPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = rootPath,
                    FileName = "explorer.exe"
                };

                Process.Start (startInfo);
            }
        }

        /// <summary>
        /// Save Game Info to Settings
        /// </summary>
        public void SaveGamesInfo ()
        {
            SettingsManager.SaveGameInfo (CurrentGame);
        }


        /// <summary>
        /// Open settings canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        public void OpenOrCloseSettingsCanvas ()
        {
            var settingsWindow = new SettingsWindow ();
            settingsWindow.ShowDialog ();
        }

        /// <summary>
        /// Open install window
        /// </summary>
        public void OpenInstallWindow ()
        {
            // Need integration
            //var installGameWindow = new InstallGame (ref SelectedGame, patcher, MAINAPP_VERSIONINFO_URL);
            //installGameWindow.ShowDialog ();
        }

        /// <summary>
        /// Locate the game using explorer, this will assign the selected game location to the selected location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LocateTheGame (object sender = null, MouseButtonEventArgs e = null)
        {
            // Open browser dialog
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog ();
            dialog.SelectedPath = CurrentGame.InstallPath;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog ();

            // If selection was OK
            if (result == System.Windows.Forms.DialogResult.OK)
            {

                // Check if the version file exists
                string versionFile = System.IO.Path.Combine (dialog.SelectedPath, "App_version.data");

                // If exists
                if (System.IO.File.Exists (versionFile))
                {
                    // Check if we have at least 1 file
                    if (Directory.GetFiles (dialog.SelectedPath).Length > 0)
                    {
                        CurrentGame.InstallPath = dialog.SelectedPath;

                        SaveGamesInfo ();

                        // Try to update the game
                        // mainAppDirectory = System.IO.Path.Combine (SelectedGame.Location, CurrentSelectedEnvironment);
                        mainAppDirectory = CurrentGame.InstallPath;
                        PatchNow ();
                    }
                    else
                    {
                        MessageBox.Show ("There are no files in the selected folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show ("The version file was not found in the selected folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Topside_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove ();
        }

        private void Menu_Logo_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenOrCloseSettingsCanvas ();
        }

        private void TopButton_MyAccount_MouseDown (object sender, MouseButtonEventArgs e)
        {
            // Open URL
            Utility.OpenURL (MY_ACCOUNT_URL);
        }

        private void lbl_Webpage_MouseDown (object sender, MouseButtonEventArgs e)
        {
            // Open URL
            Utility.OpenURL (WEBPAGE_URL);
        }

        private void lbl_PatchNotes_MouseDown (object sender, MouseButtonEventArgs e)
        {
            // Open URL
            Utility.OpenURL (PATCH_NOTES_URL);
        }

        private void Button_RefreshAll_Click (object sender, RoutedEventArgs e)
        {
            FetchNews ();

            if (CurrentLauncherStatus == LauncherStatus.play)
            {
                LoadGameInfo ();
            }
        }

        private void MenuItem_ShowInExplorer_Click (object sender, RoutedEventArgs e)
        {

            ShowSelectedGameInExplorer ();
        }

        private void Dropdown_Environment_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (isFirstTimeLoadEnvironment == false)
            {
                isFirstTimeLoadEnvironment = true;
                return;
            }

            CurrentSelectedEnvironment = ENVIRONMENT[Dropdown_Environment.SelectedIndex];

            InitializeLauncher ();

            //MessageBox.Show (currentEnvironemnt);
        }

        void Alert_Menu_Click (object sender, MouseButtonEventArgs e)
        {
            Utility.OpenURL (newsContent.Alerts[0].interactionURL);
        }
    }

    enum LauncherStatus
    {
        checking,
        play,
        failed,
        downloadingUpdate,
        patching,
        readyToUpdate,
        requireInstall,
        isLinkOnly,
    }
}