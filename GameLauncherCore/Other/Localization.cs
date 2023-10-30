using System.Collections.Generic;
using System.Globalization;

namespace GameLauncherCore
{
    public enum LocalizationID
    {

        /// Main UI
        MainUI_MainButtonState_Play,
        MainUI_MainButtonState_Retry,
        MainUI_MainButtonState_DownloadingUpdate,
        MainUI_MainButtonState_Downloading,
        MainUI_MainButtonState_Patching,
        MainUI_MainButtonState_Checking,
        MainUI_MainButtonState_ReadyToUpdate,
        MainUI_MainButtonState_Install,
        MainUI_MainButtonState_IsLinkOnly,
        MainUI_RegionVersion,
        MainUI_Version,

        // Top Options
        MainUI_TopOptions_MyAccount,
        MainUI_TopOptions_Forum,

        // Library topside
        MainUI_Library_Topside_AllGames,
        MainUI_Library_Topside_Favorites,

        // Library count
        MainUI_Library_AllGames,
        MainUI_Library_FreeToPlay,
        MainUI_Library_Multiplayer,
        MainUI_Library_Mobile,
        MainUI_Library_MacOS,
        MainUI_Library_Installed,
        MainUI_Library_Favorites,

        // Search bar
        SearchBar_Placeholder,


        // Links
        MainUI_Links_Webpage,
        MainUI_Links_PatchNotes,

        // Environment
        MainUI_EnvironmentTitle,

        // News
        MainUI_NoNewsAtTheMoment,

        /// Settings
        Settings,
        Settings_MainOptions_Launcher,
        Settings_MainOptions_Downloads,
        Settings_MainOptions_About,
        Settings_LauncherSettings_LauncherSettingsTitle,
        Settings_LauncherSettings_UILanguage,
        Settings_LauncherSettings_OnAppLaunch,
        Settings_LauncherSettingsOnAppLaunchDropdown_Open,
        Settings_LauncherSettingsOnAppLaunchDropdown_Minimize,
        Settings_LauncherSettingsOnAppLaunchDropdown_Close,


        Settings_DownloadsSettings_DownloadsSettingsTitle,
        Settings_DownloadsSettings_DefaultInstallLocation,
        Settings_DownloadsSettings_CustomInstallLocation,
        Settings_DownloadsSettings_UseDefaultInstallLocation,
        Settings_DownloadsSettings_ChangeLocation,
        Settings_About_AboutSettingsTitle,
        Settings_About_GetGameLauncher,
        TERMS_OF_SERVICE,
        PRIVACY_POLICY,

        // Game Settings Dropdown
        GameSettingsDropdown_Repair,
        GameSettingsDropdown_ShowInExplorer,
        GameSettingsDropdown_GameSettings,
        GameSettingsDropdown_LauncherSettings,
        GameSettingsDropdown_Uninstall,


        // Game Settings Window
        GameSettings_InstallLocation,
        GameSettings_EnableAutomaticUpdates,
        GameSettings_UseAdditionalLaunchParameters,
        GameSettings_Installed,

        // Uninstall Window
        Uninstall_UninstallText,
        Uninstall_Yes,
        Uninstall_No,

        /// Error on Reload News
        ErrorAtGetNews,
        Refresh,

        /// Patcher
        AllFilesAreDownloadedInXSeconds,
        AllPatchesCreatedInXSeconds,
        AlreadyUpToDateXthFile,
        ApplyingIncrementalPatch,
        ApplyingInstallerPatch,
        ApplyingRepairPatch,
        AppIsUpToDate,
        CalculatingDiffOfX,
        CalculatingFilesToDownload,
        CalculatingNewOrChangedFiles,
        CalculatingObsoleteFiles,
        Cancelled,
        CheckingForUpdates,
        CheckingIfFilesAreUpToDate,
        CompressingFilesToDestination,
        CompressingPatchIntoOneFile,
        CompressingXToY,
        CompressionFinishedInXSeconds,
        CompressionRatioIsX,
        CopyingXToPatch,
        CreatingIncrementalPatch,
        CreatingIncrementalPatchX,
        CreatingInstallerPatch,
        CreatingRepairPatch,
        CreatingXthFile,
        DecompressingPatchX,
        DeletingX,
        DeletingXObsoleteFiles,
        Done,
        DownloadingPatchX,
        DownloadingXFiles,
        DownloadingXProgressInfo,
        DownloadingXthFile,
        E_AccessToXIsForbiddenRunInAdminMode,
        E_AnotherInstanceOfXIsRunning,
        E_CouldNotDownloadPatchInfoX,
        E_CouldNotReadDownloadLinksFromX,
        E_DiffOfXDoesNotExist,
        E_DirectoryXIsEmpty,
        E_DirectoryXIsNotEmpty,
        E_DirectoryXMissing,
        E_DownloadedFileXIsCorrupt,
        E_DownloadLinkXIsNotValid,
        E_FilesAreNotUpToDateAfterPatch,
        E_FileXDoesNotExistOnServer,
        E_FileXIsNotValidOnServer,
        E_FileXMissing,
        E_InsufficientSpaceXNeededInY,
        E_InvalidPatchInfoX,
        E_NoSuitablePatchMethodFound,
        E_PatchInfoCouldNotBeVerified,
        E_PatchInfoDoesNotExistAtX,
        E_PatchXCouldNotBeDownloaded,
        E_PreviousVersionXIsNotLessThanY,
        E_ProjectInfoCouldNotBeDeserializedFromX,
        E_ProjectInfoOutdated,
        E_SelfPatcherDoesNotExist,
        E_ServersUnderMaintenance,
        E_VersionCodeXIsInvalid,
        E_VersionInfoCouldNotBeDeserializedFromX,
        E_VersionInfoCouldNotBeDownloaded,
        E_VersionInfoCouldNotBeVerified,
        E_VersionInfoInvalid,
        E_XCanNotBeEmpty,
        E_XContainsInvalidCharacters,
        E_XCouldNotBeDownloaded,
        E_XDoesNotExist,
        GeneratingListOfFilesInBuild,
        GotVersionInfoXML,
        IncrementalPatchCreatedInXSeconds,
        NoObsoleteFiles,
        PatchAppliedInXSeconds,
        PatchCompletedInXSeconds,
        PatchCreatedInXSeconds,
        PatchMethodXSizeY,
        ReadyToSelfPatch,
        RenamingXFiles,
        RetrievingVersionInfo,
        SomeFilesAreStillNotUpToDate,
        UpdateAvailable,
        UpdatingX,
        UpdatingXFiles,
        UpdatingXFilesAtY,
        UpdatingXthFile,
        WritingIncrementalPatchInfoToXML,
        WritingVersionInfoToXML,
        XDownloadedInYSeconds,
        XDownloadLinksAreUpdatedSuccessfully,
        XFilesUpdatedSuccessfully
    }

    public static class Localization
    {
        // A custom IEqualityComparer to avoid GC for using enum as key to dictionary
        public class StringIdComparer : IEqualityComparer<LocalizationID>
        {
            public bool Equals( LocalizationID s1, LocalizationID s2 ) { return s1 == s2; }
            public int GetHashCode( LocalizationID s ) { return (int) s; }
        }

        private static Dictionary<LocalizationID, string> Strings;
        public static string CurrentLanguageISOCode { get; private set; }

        static Localization()
        {
            Strings = new Dictionary<LocalizationID, string>( new StringIdComparer() );
            SetCulture( CultureInfo.CurrentCulture );
        }

        public static string Get( LocalizationID key )
        {
            string result;
            if( Strings.TryGetValue( key, out result ) )
                return result;

            return string.Concat( "__", key.ToString(), "__" );
        }

        public static string Get( LocalizationID key, object arg0 )
        {
            string result;
            if( Strings.TryGetValue( key, out result ) )
                return string.Format( result, arg0 );

            return string.Concat( "__", key.ToString(), "__" );
        }

        public static string Get( LocalizationID key, object arg0, object arg1 )
        {
            string result;
            if( Strings.TryGetValue( key, out result ) )
                return string.Format( result, arg0, arg1 );

            return string.Concat( "__", key.ToString(), "__" );
        }

        public static string Get( LocalizationID key, object arg0, object arg1, object arg2 )
        {
            string result;
            if( Strings.TryGetValue( key, out result ) )
                return string.Format( result, arg0, arg1, arg2 );

            return string.Concat( "__", key.ToString(), "__" );
        }

        public static string Get( LocalizationID key, params object[] args )
        {
            string result;
            if( Strings.TryGetValue( key, out result ) )
                return string.Format( result, args );

            return string.Concat( "__", key.ToString(), "__" );
        }

        public static bool SetCulture( CultureInfo culture )
        {
            return SetLanguage( culture.Name);
        }

        public static bool SetLanguage( string languageISOCode )
        {
            if( string.IsNullOrEmpty( languageISOCode ) )
                return false;

            //languageISOCode = languageISOCode.ToLowerInvariant();
            if( CurrentLanguageISOCode == languageISOCode )
                return true;

            CurrentLanguageISOCode = languageISOCode;
            if (languageISOCode == "en_US") {
                SetLanguageEN_US ();
            }
            else if (languageISOCode == "es_MX") {
                SetLanguageES_MX ();
            }
            else if (languageISOCode == "tr_TR") {
                SetLanguageTR_TR ();
            }
            else {
                SetLanguage ("en_US");
                return false;
            }

            return true;
        }

        public static bool SetStrings( Dictionary<LocalizationID, string> strings, string languageISOCode = null )
        {
            if( strings != null && strings.Count > 0 )
            {
                Strings = strings;
                if( !string.IsNullOrEmpty( languageISOCode ) )
                    CurrentLanguageISOCode = languageISOCode.ToLowerInvariant();
                else
                    CurrentLanguageISOCode = null;

                return true;
            }

            return false;
        }

        private static void SetLanguageEN_US()
        {
            Strings.Clear();

            /// Main UI
            // Topside Options
            Strings [LocalizationID.MainUI_TopOptions_MyAccount] = "My Account";
            Strings [LocalizationID.MainUI_TopOptions_Forum] = "Forum";

            // Library topside
            Strings[LocalizationID.MainUI_Library_Topside_AllGames] = "ALL GAMES";
            Strings[LocalizationID.MainUI_Library_Topside_Favorites] = "FAVORITES";

            // Library
            Strings[LocalizationID.MainUI_Library_AllGames] = "All Games";
            Strings[LocalizationID.MainUI_Library_FreeToPlay] = "Free to play";
            Strings[LocalizationID.MainUI_Library_Multiplayer] = "Multiplayer";
            Strings[LocalizationID.MainUI_Library_MacOS] = "MacOS";
            Strings[LocalizationID.MainUI_Library_Mobile] = "Mobile";
            Strings[LocalizationID.MainUI_Library_Installed] = "Installed";
            Strings[LocalizationID.MainUI_Library_Favorites] = "Favorites";

            // Search bar
            Strings[LocalizationID.SearchBar_Placeholder] = "Search...";

            // Main Button States
            Strings[LocalizationID.MainUI_MainButtonState_Play] = "Play";
            Strings [LocalizationID.MainUI_MainButtonState_Retry] = "Retry";
            Strings [LocalizationID.MainUI_MainButtonState_DownloadingUpdate] = "Downloading update...";
            Strings [LocalizationID.MainUI_MainButtonState_Downloading] = "Downloading";
            Strings [LocalizationID.MainUI_MainButtonState_Patching] = "Patching...";
            Strings [LocalizationID.MainUI_MainButtonState_Checking] = "Checking...";
            Strings [LocalizationID.MainUI_MainButtonState_ReadyToUpdate] = "Update";
            Strings [LocalizationID.MainUI_MainButtonState_Install] = "Install";
            Strings [LocalizationID.MainUI_MainButtonState_IsLinkOnly] = "Open";

            // Leftside State
            Strings[LocalizationID.MainUI_RegionVersion] = "Region: {0} | Version: {1}";
            Strings[LocalizationID.MainUI_Version] = "Version: {0}";

            // Links 
            Strings[LocalizationID.MainUI_Links_Webpage] = "🔗 Webpage";
            Strings [LocalizationID.MainUI_Links_PatchNotes] = "📝 Patch Notes";

            // Game Version Title
            Strings [LocalizationID.MainUI_EnvironmentTitle] = "ENVIRONMENT";

            // News
            Strings [LocalizationID.MainUI_NoNewsAtTheMoment] = "No news at the moment";
            Strings [LocalizationID.ErrorAtGetNews] = "An error has been occurred loading news";
            Strings [LocalizationID.Refresh] = "Refresh";

            /// Settings
            Strings [LocalizationID.Settings] = "Settings";
            Strings [LocalizationID.Settings_MainOptions_Launcher] = "Launcher";
            Strings[LocalizationID.Settings_MainOptions_Downloads] = "Downloads";
            Strings [LocalizationID.Settings_MainOptions_About] = "About";
            Strings[LocalizationID.Settings_LauncherSettings_LauncherSettingsTitle] = "Launcher Settings";
            Strings [LocalizationID.Settings_LauncherSettings_UILanguage] = "UI Language";
            Strings[LocalizationID.Settings_LauncherSettings_OnAppLaunch] = "On App Launch";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Open] = "Keep launcher open";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Minimize] = "Minimize launcher";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Close] = "Close launcher";

            Strings[LocalizationID.Settings_DownloadsSettings_DownloadsSettingsTitle] = "Download Settings";
            Strings[LocalizationID.Settings_DownloadsSettings_DefaultInstallLocation] = "Default install location";
            Strings[LocalizationID.Settings_DownloadsSettings_CustomInstallLocation] = "Custom install location";
            Strings[LocalizationID.Settings_DownloadsSettings_UseDefaultInstallLocation] = "Use default install location";
            Strings[LocalizationID.Settings_DownloadsSettings_ChangeLocation] = "Change location";


            Strings[LocalizationID.Settings_About_AboutSettingsTitle] = "About Game Launcher";
            Strings [LocalizationID.Settings_About_GetGameLauncher] = "Get Game Launcher";

            Strings [LocalizationID.TERMS_OF_SERVICE] = "TERMS OF SERVICE";
            Strings [LocalizationID.PRIVACY_POLICY] = "PRIVACY NOTICE";

            // Game Settings Dropdown
            Strings[LocalizationID.GameSettingsDropdown_Repair] = "Repair";
            Strings[LocalizationID.GameSettingsDropdown_ShowInExplorer] = "Show in Explorer";
            Strings[LocalizationID.GameSettingsDropdown_GameSettings] = "Game Settings";
            Strings[LocalizationID.GameSettingsDropdown_LauncherSettings] = "Launcher Settings";
            Strings[LocalizationID.GameSettingsDropdown_Uninstall] = "Uninstall";

            // Game Settings Window
            Strings[LocalizationID.GameSettings_InstallLocation] = "Install Location";
            Strings[LocalizationID.GameSettings_EnableAutomaticUpdates] = "Enable Automatic Updates";
            Strings[LocalizationID.GameSettings_UseAdditionalLaunchParameters] = "Use Additional Launch Parameters";
            Strings[LocalizationID.GameSettings_Installed] = "Installed";

            // Uninstall Window
            Strings[LocalizationID.Uninstall_UninstallText] = "Are you sure you want to uninstall {0}? This will remove all files and components";
            Strings[LocalizationID.Uninstall_Yes] = "Yes, Uninstall";
            Strings[LocalizationID.Uninstall_No] = "No";

            /// Patcher
            Strings[LocalizationID.AllFilesAreDownloadedInXSeconds] = "All files are successfully downloaded in {0} seconds";
            Strings[LocalizationID.AllPatchesCreatedInXSeconds] = "All patches created in {0} seconds...";
            Strings[LocalizationID.AlreadyUpToDateXthFile] = "{0}/{1} Already up-to-date: {2}";
            Strings[LocalizationID.ApplyingIncrementalPatch] = "Applying incremental patch";
            Strings[LocalizationID.ApplyingInstallerPatch] = "Applying installer patch";
            Strings[LocalizationID.ApplyingRepairPatch] = "Applying repair patch";
            Strings[LocalizationID.AppIsUpToDate] = "App is up-to-date...";
            Strings[LocalizationID.CalculatingDiffOfX] = "Calculating diff of {0}";
            Strings[LocalizationID.CalculatingFilesToDownload] = "Calculating files to download...";
            Strings[LocalizationID.CalculatingNewOrChangedFiles] = "Calculating new or changed files...";
            Strings[LocalizationID.CalculatingObsoleteFiles] = "Calculating obsolete files...";
            Strings[LocalizationID.Cancelled] = "Operation cancelled...";
            Strings[LocalizationID.CheckingForUpdates] = "Checking for updates...";
            Strings[LocalizationID.CheckingIfFilesAreUpToDate] = "Checking if files are up-to-date...";
            Strings[LocalizationID.CompressingFilesToDestination] = "Compressing files in build to destination...";
            Strings[LocalizationID.CompressingPatchIntoOneFile] = "Compressing incremental patch into one file...";
            Strings[LocalizationID.CompressingXToY] = "Compressing {0} to {1}";
            Strings[LocalizationID.CompressionFinishedInXSeconds] = "Compression finished in {0} seconds";
            Strings[LocalizationID.CompressionRatioIsX] = "Compression ratio is {0}%";
            Strings[LocalizationID.CopyingXToPatch] = "Copying {0} to patch";
            Strings[LocalizationID.CreatingIncrementalPatch] = "Creating incremental patch...";
            Strings[LocalizationID.CreatingIncrementalPatchX] = "Creating incremental patch: {0}";
            Strings[LocalizationID.CreatingInstallerPatch] = "Creating installer patch...";
            Strings[LocalizationID.CreatingRepairPatch] = "Creating repair patch...";
            Strings[LocalizationID.CreatingXthFile] = "{0}/{1} Creating: {2}";
            Strings[LocalizationID.DecompressingPatchX] = "Decompressing patch {0}...";
            Strings[LocalizationID.DeletingX] = "Deleting {0}";
            Strings[LocalizationID.DeletingXObsoleteFiles] = "Deleting {0} obsolete files...";
            Strings[LocalizationID.Done] = "Done";
            Strings[LocalizationID.DownloadingPatchX] = "Downloading patch: {0}...";
            Strings[LocalizationID.DownloadingXFiles] = "Downloading {0} new or updated file(s)...";
            Strings[LocalizationID.DownloadingXProgressInfo] = "Downloading {0}: {1}/{2}MB ({3})";
            Strings[LocalizationID.DownloadingXthFile] = "{0}/{1} Downloading: {2} ({3}MB)";
            Strings[LocalizationID.E_AccessToXIsForbiddenRunInAdminMode] = "ERROR: access to {0} is forbidden; run patcher in administrator mode";
            Strings[LocalizationID.E_AnotherInstanceOfXIsRunning] = "ERROR: another instance of {0} is running";
            Strings[LocalizationID.E_CouldNotDownloadPatchInfoX] = "ERROR: could not download patch info for {0}";
            Strings[LocalizationID.E_CouldNotReadDownloadLinksFromX] = "ERROR: could not read download links from {0}";
            Strings[LocalizationID.E_DiffOfXDoesNotExist] = "ERROR: patch file for {0} couldn't be found";
            Strings[LocalizationID.E_DirectoryXIsEmpty] = "ERROR: directory {0} is empty";
            Strings[LocalizationID.E_DirectoryXIsNotEmpty] = "ERROR: directory {0} is not empty";
            Strings[LocalizationID.E_DirectoryXMissing] = "ERROR: directory {0} is missing";
            Strings[LocalizationID.E_DownloadedFileXIsCorrupt] = "ERROR: downloaded file {0} is corrupt";
            Strings[LocalizationID.E_DownloadLinkXIsNotValid] = "ERROR: download link {0} is not in form [RelativePathToFile URL]";
            Strings[LocalizationID.E_FilesAreNotUpToDateAfterPatch] = "ERROR: files are not up-to-date after the patch";
            Strings[LocalizationID.E_FileXDoesNotExistOnServer] = "ERROR: file {0} does not exist on server";
            Strings[LocalizationID.E_FileXIsNotValidOnServer] = "ERROR: file {0} is not valid on server";
            Strings[LocalizationID.E_FileXMissing] = "ERROR: file {0} is missing";
            Strings[LocalizationID.E_InsufficientSpaceXNeededInY] = "ERROR: insufficient free space in {1}, at least {0} needed";
            Strings[LocalizationID.E_InvalidPatchInfoX] = "ERROR: patch info for {0} is invalid";
            Strings[LocalizationID.E_NoSuitablePatchMethodFound] = "ERROR: no suitable patch method found";
            Strings[LocalizationID.E_PatchInfoCouldNotBeVerified] = "ERROR: could not verify downloaded patch info";
            Strings[LocalizationID.E_PatchInfoDoesNotExistAtX] = "ERROR: patch info does not exist at {0}";
            Strings[LocalizationID.E_PatchXCouldNotBeDownloaded] = "ERROR: patch {0} could not be downloaded";
            Strings[LocalizationID.E_PreviousVersionXIsNotLessThanY] = "ERROR: previous version ({0}) is greater than or equal to current version ({1})";
            Strings[LocalizationID.E_ProjectInfoCouldNotBeDeserializedFromX] = "ERROR: project info could not be deserialized from {0}";
            Strings[LocalizationID.E_ProjectInfoOutdated] = "ERROR: project info is outdated. Create a new project at a temporary path and examine its Settings.xml file to see the new/removed settings. Add any new settings to your own Settings.xml (also update the value of the 'Surum' attribute)";
            Strings[LocalizationID.E_SelfPatcherDoesNotExist] = "ERROR: self patcher does not exist";
            Strings[LocalizationID.E_ServersUnderMaintenance] = "ERROR: servers are currently under maintenance";
            Strings[LocalizationID.E_VersionCodeXIsInvalid] = "ERROR: version code '{0}' is invalid";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDeserializedFromX] = "ERROR: version info could not be deserialized from {0}";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDownloaded] = "ERROR: could not download version info from server";
            Strings[LocalizationID.E_VersionInfoCouldNotBeVerified] = "ERROR: could not verify downloaded version info";
            Strings[LocalizationID.E_VersionInfoInvalid] = "ERROR: version info is invalid";
            Strings[LocalizationID.E_XCanNotBeEmpty] = "ERROR: {0} can not be empty";
            Strings[LocalizationID.E_XContainsInvalidCharacters] = "ERROR: {0} contains invalid character(s)";
            Strings[LocalizationID.E_XCouldNotBeDownloaded] = "ERROR: {0} could not be downloaded";
            Strings[LocalizationID.E_XDoesNotExist] = "ERROR: {0} does not exist";
            Strings[LocalizationID.GeneratingListOfFilesInBuild] = "Generating list of files in the build...";
            Strings[LocalizationID.GotVersionInfoXML] = "Got Version Info XML";
            Strings[LocalizationID.IncrementalPatchCreatedInXSeconds] = "Incremental patch created in {0} seconds...";
            Strings[LocalizationID.NoObsoleteFiles] = "No obsolete files...";
            Strings[LocalizationID.PatchAppliedInXSeconds] = "Patch applied in {0} seconds...";
            Strings[LocalizationID.PatchCompletedInXSeconds] = "Patch successfully completed in {0} seconds...";
            Strings[LocalizationID.PatchCreatedInXSeconds] = "Patch created in {0} seconds...";
            Strings[LocalizationID.PatchMethodXSizeY] = "Preferred patch method {0}: {1}";
            Strings[LocalizationID.ReadyToSelfPatch] = "Waiting for the self patcher to complete the update...";
            Strings[LocalizationID.RenamingXFiles] = "Renaming {0} files/folders...";
            Strings[LocalizationID.RetrievingVersionInfo] = "Getting version info...";
            Strings[LocalizationID.SomeFilesAreStillNotUpToDate] = "Some files are still not up-to-date, looking for solutions...";
            Strings[LocalizationID.UpdateAvailable] = "New version available";
            Strings[LocalizationID.UpdatingX] = "Updating {0}";
            Strings[LocalizationID.UpdatingXFiles] = "Updating {0} file(s)...";
            Strings[LocalizationID.UpdatingXFilesAtY] = "Updating {0} file(s) at {1}...";
            Strings[LocalizationID.UpdatingXthFile] = "{0}/{1} Updating: {2}";
            Strings[LocalizationID.WritingIncrementalPatchInfoToXML] = "Writing incremental patch info to XML...";
            Strings[LocalizationID.WritingVersionInfoToXML] = "Writing version info to XML...";
            Strings[LocalizationID.XDownloadedInYSeconds] = "{0} downloaded in {1} seconds";
            Strings[LocalizationID.XDownloadLinksAreUpdatedSuccessfully] = "{0}/{1} download links are updated successfully";
            Strings[LocalizationID.XFilesUpdatedSuccessfully] = "{0}/{1} files updated successfully";
        }

        private static void SetLanguageES_MX()
        {
            Strings.Clear();

            /// Main UI
            // Topside Options
            Strings [LocalizationID.MainUI_TopOptions_MyAccount] = "Mi Cuenta";
            Strings [LocalizationID.MainUI_TopOptions_Forum] = "Foro";

            // Library topside
            Strings[LocalizationID.MainUI_Library_Topside_AllGames] = "TODOS LOS JUEGOS";
            Strings[LocalizationID.MainUI_Library_Topside_Favorites] = "FAVORITOS";

            // Library
            Strings[LocalizationID.MainUI_Library_AllGames] = "Todos los juegos";
            Strings[LocalizationID.MainUI_Library_FreeToPlay] = "Gratis";
            Strings[LocalizationID.MainUI_Library_Multiplayer] = "Multijugador";
            Strings[LocalizationID.MainUI_Library_MacOS] = "MacOS";
            Strings[LocalizationID.MainUI_Library_Mobile] = "Móvil";
            Strings[LocalizationID.MainUI_Library_Installed] = "Instalados";
            Strings[LocalizationID.MainUI_Library_Favorites] = "Favoritos";

            // Search bar
            Strings[LocalizationID.SearchBar_Placeholder] = "Buscar...";

            // Links 
            Strings[LocalizationID.MainUI_Links_Webpage] = "🔗 Sitio Web";
            Strings [LocalizationID.MainUI_Links_PatchNotes] = "📝 Notas del Parche";

            // Main ButtonState
            Strings [LocalizationID.MainUI_MainButtonState_Play] = "Jugar";
            Strings [LocalizationID.MainUI_MainButtonState_Retry] = "Reintentar";
            Strings [LocalizationID.MainUI_MainButtonState_DownloadingUpdate] = "Descargando actualización...";
            Strings [LocalizationID.MainUI_MainButtonState_Downloading] = "Descargando...";
            Strings [LocalizationID.MainUI_MainButtonState_Patching] = "Parchando...";
            Strings [LocalizationID.MainUI_MainButtonState_Checking] = "Comprobando...";
            Strings[LocalizationID.MainUI_MainButtonState_ReadyToUpdate] = "Actualizar";
            Strings[LocalizationID.MainUI_MainButtonState_Install] = "Instalar";
            Strings[LocalizationID.MainUI_MainButtonState_IsLinkOnly] = "Abrir";

            // Game Version Title
            Strings [LocalizationID.MainUI_EnvironmentTitle] = "ENTORNO";

            // Leftside State
            Strings [LocalizationID.MainUI_RegionVersion] = "Región: {0} | Versión: {1}";
            Strings [LocalizationID.MainUI_Version] = "Versión: {0}";

            // News
            Strings[LocalizationID.MainUI_NoNewsAtTheMoment] = "Sin noticias por ahora";
            Strings [LocalizationID.ErrorAtGetNews] = "Ha ocurrido un error al obtener las noticias";
            Strings [LocalizationID.Refresh] = "Recargar";

            /// Settings
            Strings [LocalizationID.Settings] = "Ajustes";
            Strings [LocalizationID.Settings_MainOptions_Launcher] = "Launcher";
            Strings[LocalizationID.Settings_MainOptions_Downloads] = "Descargas";
            Strings[LocalizationID.Settings_MainOptions_About] = "Sobre";
            Strings [LocalizationID.Settings_LauncherSettings_LauncherSettingsTitle] = "Configuración del Launcher";
            Strings[LocalizationID.Settings_LauncherSettings_OnAppLaunch] = "Al ejecutar un juego";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Open] = "Mantener el launcher abierto";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Minimize] = "Minimizar el launcher";
            Strings[LocalizationID.Settings_LauncherSettingsOnAppLaunchDropdown_Close] = "Cerrar el launcher";
            Strings[LocalizationID.Settings_LauncherSettings_UILanguage] = "Lenguaje de Interfáz";
            Strings[LocalizationID.Settings_DownloadsSettings_DownloadsSettingsTitle] = "Configuración de descargas";
            Strings[LocalizationID.Settings_DownloadsSettings_DefaultInstallLocation] = "Ubicación de instalación predeterminada";
            Strings[LocalizationID.Settings_DownloadsSettings_CustomInstallLocation] = "Ubicación de instalación personalizada";
            Strings[LocalizationID.Settings_DownloadsSettings_UseDefaultInstallLocation] = "Usar ubicación de instalación predeterminada";
            Strings[LocalizationID.Settings_DownloadsSettings_ChangeLocation] = "Cambiar ubicación";
            Strings[LocalizationID.Settings_About_AboutSettingsTitle] = "Sobre Game Launcher";
            Strings [LocalizationID.Settings_About_GetGameLauncher] = "Obtén Game Launcher";

            Strings [LocalizationID.TERMS_OF_SERVICE] = "TÉRMINOS DE SERVICIO";
            Strings [LocalizationID.PRIVACY_POLICY] = "AVISO DE PRIVACIDAD";

            // Game Settings Dropdown
            Strings[LocalizationID.GameSettingsDropdown_Repair] = "Reparar";
            Strings[LocalizationID.GameSettingsDropdown_ShowInExplorer] = "Mostrar en el Explorador";
            Strings[LocalizationID.GameSettingsDropdown_GameSettings] = "Configuración del Juego";
            Strings[LocalizationID.GameSettingsDropdown_LauncherSettings] = "Configuración del Launcher";
            Strings[LocalizationID.GameSettingsDropdown_Uninstall] = "Desinstalar";

            // Game Settings Window
            Strings[LocalizationID.GameSettings_InstallLocation] = "Ubicación de instalación";
            Strings[LocalizationID.GameSettings_EnableAutomaticUpdates] = "Habilitar actualizaciones automáticas";
            Strings[LocalizationID.GameSettings_UseAdditionalLaunchParameters] = "Usar parametros de ejecución adicionales";
            Strings[LocalizationID.GameSettings_Installed] = "Instalado";

            // Uninstall Window
            Strings[LocalizationID.Uninstall_UninstallText] = "¿Estás seguro de que deseas desinstalar {0}? Esto eliminará todos los archivos y componentes";
            Strings[LocalizationID.Uninstall_Yes] = "Sí, Desinstalar";
            Strings[LocalizationID.Uninstall_No] = "No";
            /// Patcher
            Strings [LocalizationID.AllFilesAreDownloadedInXSeconds] = "Todos los archivos fueron correctamente descargados en {0} segundos";
            Strings [LocalizationID.AllPatchesCreatedInXSeconds] = "Todos los parches creados en {0} segundos";
            Strings[LocalizationID.AlreadyUpToDateXthFile] = "{0}/{1} Ya actualizado: {2}";
            Strings[LocalizationID.ApplyingIncrementalPatch] = "Aplicando parche incremental";
            Strings[LocalizationID.ApplyingInstallerPatch] = "Aplicando parche de instalación";
            Strings[LocalizationID.ApplyingRepairPatch] = "Aplicando parche de reparación";
            Strings[LocalizationID.AppIsUpToDate] = "La aplicación está actualizada...";
            Strings[LocalizationID.CalculatingDiffOfX] = "Calculando la diferencia de {0}";
            Strings[LocalizationID.CalculatingFilesToDownload] = "Calculando archivos para descargar...";
            Strings[LocalizationID.CalculatingNewOrChangedFiles] = "Calculando archivos nuevos o modificados...";
            Strings[LocalizationID.CalculatingObsoleteFiles] = "Calculando archivos obsoletos...";
            Strings[LocalizationID.Cancelled] = "Operación cancelada...";
            Strings[LocalizationID.CheckingForUpdates] = "Comprobando actualizaciones...";
            Strings[LocalizationID.CheckingIfFilesAreUpToDate] = "Comprobando si los archivos están actualizados...";
            Strings[LocalizationID.CompressingFilesToDestination] = "Comprimiendo archivos de compilación a destino...";
            Strings[LocalizationID.CompressingPatchIntoOneFile] = "Comprimiendo parche incremental en un archivo...";
            Strings[LocalizationID.CompressingXToY] = "Comprimiendo {0} a {1}";
            Strings[LocalizationID.CompressionFinishedInXSeconds] = "Compresión finalizada en {0} segundos";
            Strings[LocalizationID.CompressionRatioIsX] = "La relación de compresión es {0}%";
            Strings[LocalizationID.CopyingXToPatch] = "Copiando {0} al parche";
            Strings[LocalizationID.CreatingIncrementalPatch] = "Creando parche incremental...";
            Strings[LocalizationID.CreatingIncrementalPatchX] = "Creando parche incremental: {0}";
            Strings[LocalizationID.CreatingInstallerPatch] = "Creando parche de instalación...";
            Strings[LocalizationID.CreatingRepairPatch] = "Creando parche de reparación...";
            Strings[LocalizationID.CreatingXthFile] = "{0}/{1} Creando: {2}";
            Strings[LocalizationID.DecompressingPatchX] = "Descomprimiendo parche {0}...";
            Strings[LocalizationID.DeletingX] = "Eliminando {0}";
            Strings[LocalizationID.DeletingXObsoleteFiles] = "Eliminando {0} archivos obsoletos...";
            Strings[LocalizationID.Done] = "Listo";
            Strings[LocalizationID.DownloadingPatchX] = "Descargando parche: {0}...";
            Strings[LocalizationID.DownloadingXFiles] = "Descargando {0} archivos nuevos o actualizados...";
            Strings[LocalizationID.DownloadingXProgressInfo] = "Descargando {0}: {1}/{2} MB ({3})";
            Strings[LocalizationID.DownloadingXthFile] = "{0}/{1} Descargando: {2} ({3}MB)";
            Strings[LocalizationID.E_AccessToXIsForbiddenRunInAdminMode] = "ERROR: el acceso a {0} está prohibido; ejecutar patcher en modo administrador";
            Strings[LocalizationID.E_AnotherInstanceOfXIsRunning] = "ERROR: se está ejecutando otra instancia de {0}";
            Strings[LocalizationID.E_CouldNotDownloadPatchInfoX] = "ERROR: no se pudo descargar la información del parche para {0}";
            Strings[LocalizationID.E_CouldNotReadDownloadLinksFromX] = "ERROR: no se pudieron leer los enlaces de descarga de {0}";
            Strings[LocalizationID.E_DiffOfXDoesNotExist] = "ERROR: no se pudo encontrar el archivo de parche para {0}";
            Strings[LocalizationID.E_DirectoryXIsEmpty] = "ERROR: el directorio {0} está vacío";
            Strings[LocalizationID.E_DirectoryXIsNotEmpty] = "ERROR: el directorio {0} no está vacío";
            Strings[LocalizationID.E_DirectoryXMissing] = "ERROR: falta el directorio {0}";
            Strings[LocalizationID.E_DownloadedFileXIsCorrupt] = "ERROR: el archivo descargado {0} está dañado";
            Strings[LocalizationID.E_DownloadLinkXIsNotValid] = "ERROR: el enlace de descarga {0} no tiene el formato [RelativePathToFile URL]";
            Strings[LocalizationID.E_FilesAreNotUpToDateAfterPatch] = "ERROR: los archivos no están actualizados después del parche";
            Strings[LocalizationID.E_FileXDoesNotExistOnServer] = "ERROR: el archivo {0} no existe en el servidor";
            Strings[LocalizationID.E_FileXIsNotValidOnServer] = "ERROR: el archivo {0} no es válido en el servidor";
            Strings[LocalizationID.E_FileXMissing] = "ERROR: falta el archivo {0}";
            Strings[LocalizationID.E_InsufficientSpaceXNeededInY] = "ERROR: espacio libre insuficiente en {1}, se necesita al menos {0}";
            Strings[LocalizationID.E_InvalidPatchInfoX] = "ERROR: patch info for {0} is invalid";
            Strings[LocalizationID.E_NoSuitablePatchMethodFound] = "ERROR: la información del parche para {0} no es válida";
            Strings[LocalizationID.E_PatchInfoCouldNotBeVerified] = "ERROR: no se pudo verificar la información del parche descargado";
            Strings[LocalizationID.E_PatchInfoDoesNotExistAtX] = "ERROR: la información del parche no existe en {0}";
            Strings[LocalizationID.E_PatchXCouldNotBeDownloaded] = "ERROR: no se pudo descargar el parche {0}";
            Strings[LocalizationID.E_PreviousVersionXIsNotLessThanY] = "ERROR: la versión anterior ({0}) es mayor o igual que la versión actual ({1})";
            Strings[LocalizationID.E_ProjectInfoCouldNotBeDeserializedFromX] = "ERROR: la información del proyecto no se pudo deserializar de {0}";
            Strings[LocalizationID.E_ProjectInfoOutdated] = "ERROR: la información del proyecto está desactualizada. Cree un nuevo proyecto en una ruta temporal y examine su archivo Settings.xml para ver las configuraciones nuevas o eliminadas. Agregue cualquier configuración nueva a su propio Settings.xml (también actualice el valor del atributo 'Surum')";
            Strings[LocalizationID.E_SelfPatcherDoesNotExist] = "ERROR: el selftpatcher no existe";
            Strings[LocalizationID.E_ServersUnderMaintenance] = "ERROR: los servidores están actualmente en mantenimiento";
            Strings[LocalizationID.E_VersionCodeXIsInvalid] = "ERROR: el código de versión '{0}' no es válido";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDeserializedFromX] = "ERROR: la información de la versión no se pudo deserializar de {0}";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDownloaded] = "ERROR: no se pudo descargar la información de la versión del servidor";
            Strings[LocalizationID.E_VersionInfoCouldNotBeVerified] = "ERROR: no se pudo verificar la información de la versión descargada";
            Strings[LocalizationID.E_VersionInfoInvalid] = "ERROR: la información de la versión no es válida";
            Strings[LocalizationID.E_XCanNotBeEmpty] = "ERROR: {0} can not be empty";
            Strings[LocalizationID.E_XContainsInvalidCharacters] = "ERROR: {0} contiene caracteres no válidos";
            Strings[LocalizationID.E_XCouldNotBeDownloaded] = "ERROR: {0} no se pudo descargar";
            Strings[LocalizationID.E_XDoesNotExist] = "ERROR: {0} no existe";
            Strings[LocalizationID.GeneratingListOfFilesInBuild] = "Generando lista de archivos en la compilación...";
            Strings[LocalizationID.GotVersionInfoXML] = "Se ha obtenido el XML con la información de la versión";
            Strings[LocalizationID.IncrementalPatchCreatedInXSeconds] = "Parche incremental creado en {0} segundos...";
            Strings[LocalizationID.NoObsoleteFiles] = "Sin archivos obsoletos...";
            Strings[LocalizationID.PatchAppliedInXSeconds] = "Parche aplicado en {0} segundos...";
            Strings[LocalizationID.PatchCompletedInXSeconds] = "Parche completado con éxito en {0} segundos...";
            Strings[LocalizationID.PatchCreatedInXSeconds] = "Patch created in {0} seconds...";
            Strings[LocalizationID.PatchMethodXSizeY] = "Método de parche preferido {0}: {1}";
            Strings[LocalizationID.ReadyToSelfPatch] = "Esperando a que el parche automático complete la actualización...";
            Strings[LocalizationID.RenamingXFiles] = "Cambiando el nombre de {0} archivos/carpetas...";
            Strings[LocalizationID.RetrievingVersionInfo] = "Obteniendo información de la versión...";
            Strings[LocalizationID.SomeFilesAreStillNotUpToDate] = "Algunos archivos aún no están actualizados, buscando soluciones...";
            Strings[LocalizationID.UpdateAvailable] = "Nueva versión disponible";
            Strings[LocalizationID.UpdatingX] = "Actualizando {0}";
            Strings[LocalizationID.UpdatingXFiles] = "Actualizando {0} archivo(s)...";
            Strings[LocalizationID.UpdatingXFilesAtY] = "Actualizando {0} archivo(s) en {1}...";
            Strings[LocalizationID.UpdatingXthFile] = "{0}/{1} Actualizando: {2}";
            Strings[LocalizationID.WritingIncrementalPatchInfoToXML] = "Escribiendo información de parche incremental en XML...";
            Strings[LocalizationID.WritingVersionInfoToXML] = "Escribiendo información de versión en XML...";
            Strings[LocalizationID.XDownloadedInYSeconds] = "{0} descargado en {1} segundos";
            Strings[LocalizationID.XDownloadLinksAreUpdatedSuccessfully] = "Los enlaces de descarga {0}/{1} se actualizaron correctamente";
            Strings[LocalizationID.XFilesUpdatedSuccessfully] = "{0}/{1} archivos actualizados correctamente";
        }

        private static void SetLanguageTR_TR()
        {
            Strings.Clear();

            // UI
            Strings [LocalizationID.MainUI_MainButtonState_Play] = "Play";
            Strings [LocalizationID.MainUI_MainButtonState_Retry] = "Retry";
            Strings [LocalizationID.MainUI_MainButtonState_DownloadingUpdate] = "Downloading update...";
            Strings [LocalizationID.MainUI_MainButtonState_Downloading] = "Downloading";
            Strings [LocalizationID.MainUI_MainButtonState_Patching] = "Patching...";
            Strings [LocalizationID.MainUI_MainButtonState_Checking] = "Checking...";
            Strings [LocalizationID.MainUI_NoNewsAtTheMoment] = "No news at the moment";
            Strings [LocalizationID.MainUI_RegionVersion] = "Region: {0} | Version: {1}";

            // Settings
            Strings [LocalizationID.Settings_LauncherSettings_LauncherSettingsTitle] = "Launcher Settings";

            // Patcher
            Strings [LocalizationID.AllFilesAreDownloadedInXSeconds] = "Tüm dosyalar {0} saniyede başarılı bir şekilde indirildi";
            Strings[LocalizationID.AllPatchesCreatedInXSeconds] = "...Tüm patch'ler {0} saniyede oluşturuldu...";
            Strings[LocalizationID.AlreadyUpToDateXthFile] = "{0}/{1} Zaten güncel: {2}";
            Strings[LocalizationID.ApplyingIncrementalPatch] = "Incremental patch uygulanıyor";
            Strings[LocalizationID.ApplyingInstallerPatch] = "Installer patch uygulanıyor";
            Strings[LocalizationID.ApplyingRepairPatch] = "Repair patch uygulanıyor";
            Strings[LocalizationID.AppIsUpToDate] = "...Uygulama güncel...";
            Strings[LocalizationID.CalculatingDiffOfX] = "{0} dosyasının diff'i hesaplanıyor";
            Strings[LocalizationID.CalculatingFilesToDownload] = "...İndirilmesi gereken dosyalar hesaplanıyor...";
            Strings[LocalizationID.CalculatingNewOrChangedFiles] = "...Yeni veya değişmiş dosyalar hesaplanıyor...";
            Strings[LocalizationID.CalculatingObsoleteFiles] = "...Artık kullanılmayan dosyalar hesaplanıyor...";
            Strings[LocalizationID.Cancelled] = "...İşlem iptal edildi...";
            Strings[LocalizationID.CheckingForUpdates] = "...Güncellemeler kontrol ediliyor...";
            Strings[LocalizationID.CheckingIfFilesAreUpToDate] = "...Dosyaların güncel olup olmadığı kontrol ediliyor...";
            Strings[LocalizationID.CompressingFilesToDestination] = "...Dosyalar hedef klasörde sıkıştırılıyor...";
            Strings[LocalizationID.CompressingPatchIntoOneFile] = "...Patch dosyası sıkıştırılıyor...";
            Strings[LocalizationID.CompressingXToY] = "{0} sıkıştırılıyor: {1}";
            Strings[LocalizationID.CompressionFinishedInXSeconds] = "Sıkıştırma işlemi {0} saniyede tamamlandı";
            Strings[LocalizationID.CompressionRatioIsX] = "Toplam sıkıştırma oranı: %{0}";
            Strings[LocalizationID.CopyingXToPatch] = "{0} patch'in içerisine kopyalanıyor";
            Strings[LocalizationID.CreatingIncrementalPatch] = "...Incremental patch oluşturuluyor...";
            Strings[LocalizationID.CreatingIncrementalPatchX] = "Incremental patch oluşturuluyor: {0}";
            Strings[LocalizationID.CreatingInstallerPatch] = "...Installer patch oluşturuluyor...";
            Strings[LocalizationID.CreatingRepairPatch] = "...Repair patch oluşturuluyor...";
            Strings[LocalizationID.CreatingXthFile] = "{0}/{1} Oluşturuluyor: {2}";
            Strings[LocalizationID.DecompressingPatchX] = "...Patch'in içerisindeki dosyalar çıkartılıyor: {0}...";
            Strings[LocalizationID.DeletingX] = "Siliniyor: {0}";
            Strings[LocalizationID.DeletingXObsoleteFiles] = "...{0} eski dosya siliniyor...";
            Strings[LocalizationID.Done] = "...Tamamlandı...";
            Strings[LocalizationID.DownloadingPatchX] = "...Patch indiriliyor: {0}...";
            Strings[LocalizationID.DownloadingXFiles] = "...{0} yeni veya değişmiş dosya indiriliyor...";
            Strings[LocalizationID.DownloadingXProgressInfo] = "{0} indiriliyor: {1}/{2}MB ({3})";
            Strings[LocalizationID.DownloadingXthFile] = "{0}/{1} İndiriliyor: {2} ({3}MB)";
            Strings[LocalizationID.E_AccessToXIsForbiddenRunInAdminMode] = "HATA: {0} konumuna erişim engellendi, uygulamayı yönetici olarak çalıştırın";
            Strings[LocalizationID.E_AnotherInstanceOfXIsRunning] = "HATA: birden çok {0} çalışıyor";
            Strings[LocalizationID.E_FilesAreNotUpToDateAfterPatch] = "HATA: patch sonrası dosyalar hâlâ güncel değil";
            Strings[LocalizationID.E_CouldNotDownloadPatchInfoX] = "HATA: {0} için patch bilgileri indirilemiyor";
            Strings[LocalizationID.E_CouldNotReadDownloadLinksFromX] = "HATA: indirme linkleri {0} dosyasından okunamadı";
            Strings[LocalizationID.E_DiffOfXDoesNotExist] = "HATA: {0} için patch dosyası bulunamadı";
            Strings[LocalizationID.E_DirectoryXIsEmpty] = "HATA: klasörün içi boş: {0}";
            Strings[LocalizationID.E_DirectoryXIsNotEmpty] = "HATA: klasörün içi boş değil: {0}";
            Strings[LocalizationID.E_DirectoryXMissing] = "HATA: klasör {0} mevcut değil";
            Strings[LocalizationID.E_DownloadedFileXIsCorrupt] = "HATA: indirilen dosya {0} bozuk";
            Strings[LocalizationID.E_DownloadLinkXIsNotValid] = "HATA: {0} şu formda değil: [DosyanınKonumu İndirmeLinki]";
            Strings[LocalizationID.E_FileXDoesNotExistOnServer] = "HATA: {0} dosyası sunucuda bulunamadı";
            Strings[LocalizationID.E_FileXIsNotValidOnServer] = "HATA: sunucudaki {0} dosyası bozuk";
            Strings[LocalizationID.E_FileXMissing] = "HATA: dosya {0} mevcut değil";
            Strings[LocalizationID.E_InsufficientSpaceXNeededInY] = "HATA: {1} diskinde yeterli boş yer yok, en az {0} gerekli";
            Strings[LocalizationID.E_InvalidPatchInfoX] = "HATA: {0} için patch bilgileri hatalı";
            Strings[LocalizationID.E_NoSuitablePatchMethodFound] = "HATA: uygulanacak uygun bir patch yöntemi bulunamadı";
            Strings[LocalizationID.E_PatchInfoCouldNotBeVerified] = "HATA: indirilen patch bilgileri doğrulanamıyor";
            Strings[LocalizationID.E_PatchInfoDoesNotExistAtX] = "HATA: patch bilgileri {0} konumunda bulunamadı";
            Strings[LocalizationID.E_PatchXCouldNotBeDownloaded] = "HATA: patch {0} indirilemedi";
            Strings[LocalizationID.E_PreviousVersionXIsNotLessThanY] = "HATA: önceki sürümün versiyonu ({0}) mevcut versiyona ({1}) eşit veya daha büyük";
            Strings[LocalizationID.E_ProjectInfoCouldNotBeDeserializedFromX] = "HATA: {0} konumundaki proje bilgileri bozuk";
            Strings[LocalizationID.E_ProjectInfoOutdated] = "HATA: proje dosyasının formatı eskimiş. Geçici olarak yeni bir proje oluşturun ve bu projenin Settings.xml dosyasını inceleyin. Ardından kendi Settings'inizdeki eksik/fazla ayarları tespit edip güncelleyin ('Surum' etiketinin değerini de güncelleyin)";
            Strings[LocalizationID.E_SelfPatcherDoesNotExist] = "HATA: oto-patch dosyası mevcut değil";
            Strings[LocalizationID.E_ServersUnderMaintenance] = "HATA: sunucular şu anda bakım modunda";
            Strings[LocalizationID.E_VersionCodeXIsInvalid] = "HATA: versiyon kodu '{0}' geçersiz";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDeserializedFromX] = "HATA: {0} konumundaki versiyon bilgileri bozuk";
            Strings[LocalizationID.E_VersionInfoCouldNotBeDownloaded] = "HATA: versiyon bilgileri sunucudan çekilemiyor";
            Strings[LocalizationID.E_VersionInfoCouldNotBeVerified] = "HATA: indirilen versiyon bilgileri doğrulanamıyor";
            Strings[LocalizationID.E_VersionInfoInvalid] = "HATA: versiyon bilgileri hatalı";
            Strings[LocalizationID.E_XCanNotBeEmpty] = "HATA: {0} boş bırakılamaz";
            Strings[LocalizationID.E_XContainsInvalidCharacters] = "HATA: {0} geçersiz karakter(ler) içermekte";
            Strings[LocalizationID.E_XCouldNotBeDownloaded] = "HATA: {0} indirilemedi";
            Strings[LocalizationID.E_XDoesNotExist] = "HATA: {0} bulunamadı";
            Strings[LocalizationID.GeneratingListOfFilesInBuild] = "...Versiyondaki dosyaların listesi çıkartılıyor...";
            Strings[LocalizationID.GotVersionInfoXML] = "...Got Version Info XML";
            Strings[LocalizationID.IncrementalPatchCreatedInXSeconds] = "...Incremental patch {0} saniyede oluşturuldu...";
            Strings[LocalizationID.NoObsoleteFiles] = "...Eski dosya bulunmamakta...";
            Strings[LocalizationID.PatchAppliedInXSeconds] = "...Patch {0} saniyede tamamlandı...";
            Strings[LocalizationID.PatchCompletedInXSeconds] = "...Patch {0} saniyede başarıyla tamamlandı...";
            Strings[LocalizationID.PatchCreatedInXSeconds] = "...Patch {0} saniyede oluşturuldu...";
            Strings[LocalizationID.PatchMethodXSizeY] = "Tercih edilen patch yöntemi {0}: {1}";
            Strings[LocalizationID.ReadyToSelfPatch] = "...Uygulamanın kendini güncellemesi bekleniyor...";
            Strings[LocalizationID.RenamingXFiles] = "...{0} dosya veya klasörün ismi güncelleniyor...";
            Strings[LocalizationID.RetrievingVersionInfo] = "...Versiyon bilgileri alınıyor...";
            Strings[LocalizationID.SomeFilesAreStillNotUpToDate] = "...Bazı dosyalar hâlâ güncel değil, onarma işlemi deneniyor...";
            Strings[LocalizationID.UpdateAvailable] = "...Yeni bir güncelleme var...";
            Strings[LocalizationID.UpdatingX] = "{0} güncelleniyor";
            Strings[LocalizationID.UpdatingXFiles] = "...{0} dosya güncelleniyor...";
            Strings[LocalizationID.UpdatingXFilesAtY] = "...{1} konumundaki {0} dosya güncelleniyor...";
            Strings[LocalizationID.UpdatingXthFile] = "{0}/{1} Güncelleniyor: {2}";
            Strings[LocalizationID.WritingIncrementalPatchInfoToXML] = "...Incremental patch bilgileri XML dosyasına yazılıyor...";
            Strings[LocalizationID.WritingVersionInfoToXML] = "...Versiyon bilgileri XML dosyasına yazılıyor...";
            Strings[LocalizationID.XDownloadedInYSeconds] = "{0} {1} saniyede indirildi";
            Strings[LocalizationID.XDownloadLinksAreUpdatedSuccessfully] = "{0}/{1} indirme linki başarıyla güncellendi";
            Strings[LocalizationID.XFilesUpdatedSuccessfully] = "{0}/{1} dosya başarıyla güncellendi";
        }
    }
}