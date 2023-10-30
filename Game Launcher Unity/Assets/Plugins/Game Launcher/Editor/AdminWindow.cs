using GameLauncherCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using GameLauncher.Utilities;

namespace GameLauncher
{
    public class AdminWindow : EditorWindow
    {
		private string projectRootPath;
		private Vector2 scrollPosition;

		private ProjectManager project;

        private ProjectInfo projectInfo;

        private const string WorkspaceLauncherFolderName = "Launcher Workspace";
        private const string WorkspaceAppFolderName = "App Workspace";

        bool ModeLauncherWorkspace = true;

        string baseDownloadURLTextField = "";

        string newVersionFromUI = "1.0.0";

        List<VersionCode> versionCodes = new List<VersionCode> ();

        VersionCode currentVersion = new VersionCode(1, 0, 0);
        VersionCode newVersion = new VersionCode(1, 0, 0);
        bool noCurrentVersion = true;
        Rect rect;

        int popUpSelectedVersion = 0;

        List<string> hosts = new List<string> { "Other host", "Google Drive (Deprecated)" };
        int popupSelectedHost = 0;


        static List<string> environments = new List<string> { "Release", "Beta" };
        static int popupSelectedEnvironment = 0;

        [MenuItem ("Tools/Game Launcher/UI Manager", false, 100)]
        private static void Initialize ()
        {
            AdminWindow window = GetWindow<AdminWindow> ();
            window.titleContent = new GUIContent ("Game Launcher UI Manager");
            window.minSize = new Vector2 (1200f, 850f);

			window.Initiate ();
            window.Show ();
        }

        [MenuItem ("Tools/Game Launcher/Open launcher workspace folder")]
        public static void OpenLauncherWorkspaceFolder ()
        {
            System.Diagnostics.Process.Start(Path.Combine(Path.GetDirectoryName(Application.dataPath), WorkspaceLauncherFolderName));
        }

        public static void OpenLauncherFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Launcher"));
        }


        [MenuItem ("Tools/Game Launcher/Open app workspace folder")]
        public static void OpenAppWorkspaceFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment]));
        }
        public static void OpenAppFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment], "App"));
        }

        public static void OpenSelfPatcherFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment], "SelfPatcher"));
        }

        public static void OpenLauncherOutputFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Output"));
        }

        public static void OpenAppOutputFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment], "Output"));
        }

        public static void OpenLauncherVersionsFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Versions"));
        }

        public static void OpenAppVersionsFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, "Versions"));
        }

        public static void OpenLauncherLinksFolder ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Links"));
        }

        public static void OpenAppLinksFolder (string environment)
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environment, "Links"));
        }

        public static void OpenLauncherLinksFile ()
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Links") + Path.DirectorySeparatorChar + "DownloadLinks.txt");
        }

        public static void OpenAppLinksFile (string environment)
        {
            System.Diagnostics.Process.Start (Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environment, "Links") + Path.DirectorySeparatorChar + "DownloadLinks.txt");
        }

        public static void OpenLauncherExplorerAndSelectVersionInfoFile ()
        {
            string filePath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName, "Output") + Path.DirectorySeparatorChar + "VersionInfo.info";

            string argument = "/select, \"" + filePath + "\"";

            System.Diagnostics.Process.Start ("explorer.exe", argument);
        }

        public static void OpenAppExplorerAndSelectVersionInfoFile (string environment)
        {
            string filePath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environment, "Output") + Path.DirectorySeparatorChar + "VersionInfo.info";

            string argument = "/select, \"" + filePath + "\"";

            System.Diagnostics.Process.Start ("explorer.exe", argument);
        }

        [MenuItem ("Tools/Game Launcher/Documentation")]
        public static void OpenDocumentation ()
        {
            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/");
        }

        public static void OpenDiscord ()
        {
            Application.OpenURL ("https://discord.com/invite/rJq6cEresy");
        }

        public static void OpenGameLauncherEnterprise ()
        {
            Application.OpenURL ("https://assetstore.unity.com/packages/tools/utilities/game-launcher-patcher-and-updater-enterprise-238739");
        }

        public static void OpenInstallation()
        {
            Application.OpenURL("https://gamelauncher.gitbook.io/documentation/installation");
        }

        private void Initiate ()
        {
            //Debug.Log ("= Initializing Game Launcher...");

            //Debug.Log ("= Checking if workspace exists...");
            CheckIfWorkspaceFolderExists ();

            //Debug.Log ("= Get Current Version...");
            GetCurrentVersion ();
        }

        void GetCurrentVersion ()
        {
            // Set current version
            currentVersion = new VersionCode (1, 0, 0);

            versionCodes.Clear ();

            noCurrentVersion = true;

            // Search for current version
            // Get directorys 
            string versionsFolder = Path.Combine (projectRootPath, "Versions");

            if (!Directory.Exists(versionsFolder)) {
                return;
            }


            string [] folders = System.IO.Directory.GetDirectories (versionsFolder, "*", System.IO.SearchOption.AllDirectories);

            // Get the version code of every folder

            bool error = false;

            if (folders.Length > 0) {

                foreach (string folder in folders) {
                    var files = Directory.GetFiles (folder, "*_version.data");
                    if (files.Length == 0) {
                        //NO matching files
                    }
                    else {
                        //has matching files
                        //versionCodes.Add (new DirectoryInfo (folder).Name);
                        string versionString = System.IO.File.ReadAllText (files[0]);


                        if (new VersionCode (versionString).IsValid)
                        { 
                            if (!newVersionFromUI.EndsWith("."))
                            {
                                versionCodes.Add (versionString);
                            }
                            else
                            {
                                Debug.LogWarning ($"Version is invalid: '{versionString}' from file: '{files[0]}'");

                                error = true;

                            }
                        } else {
                            Debug.LogWarning ($"Version is invalid: '{versionString}' from file: '{files [0]}'");
                            error = true;
                        }

                        noCurrentVersion = false;
                    }
                }
            }

            if (!noCurrentVersion && error == false) { 

                versionCodes = versionCodes.OrderByDescending (x => x).ToList ();

                currentVersion = versionCodes.First ();

                // Set new version
                // Reconstruct

                int[] parts = new int[currentVersion.Length];
                Array.Copy (currentVersion.parts, parts, currentVersion.Length);

                var version = currentVersion.parts[currentVersion.Length - 1] + 1;
                parts[currentVersion.Length - 1] = version;
                
                newVersion = new VersionCode (parts);
                newVersionFromUI = newVersion.ToString ();

                //foreach (VersionCode versionCode in versionCodes) {
                //    Debug.Log (versionCode.ToString());
                //}
            } else {
                newVersion = new VersionCode (1, 0, 0);
                newVersionFromUI = newVersion.ToString ();
            }

        }
        void CheckIfWorkspaceFolderExists ()
        {

#if UNITY_EDITOR
            var rootPath = "";


            // Create folders
            rootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName);

            if (!Directory.Exists (rootPath)) {
                Debug.Log ("Creating Launcher workspace folder...");
                Directory.CreateDirectory (rootPath);
                projectRootPath = rootPath;

                CheckProjectExists (true);
            }

            rootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[0]);

            if (!Directory.Exists (rootPath)) {
                Debug.Log ("Creating App Release workspace folder...");
                Directory.CreateDirectory (rootPath);
                projectRootPath = rootPath;

                CheckProjectExists (false);
            }

            rootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[1]);

            if (!Directory.Exists (rootPath)) {
                Debug.Log ("Creating App Beta workspace folder...");
                Directory.CreateDirectory (rootPath);
                projectRootPath = rootPath;

                CheckProjectExists (false);
            }


            if (ModeLauncherWorkspace) {
                rootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName);
            } else {
                rootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment]);
            }

            SetProjectPath (rootPath);


#endif

        }

        void SetProjectPath (string rootPath)
        {
            projectRootPath = rootPath;

            // Load project info
            project = new ProjectManager (projectRootPath);
            projectInfo = project.LoadProjectInfo ();

            // Replace the Host URL folder -> MyURL/App/Release/ with MyURL/
            projectInfo.BaseDownloadURL = projectInfo.BaseDownloadURL.Replace (environments [popupSelectedEnvironment] + "/", "");

            // Assign to the text field
            baseDownloadURLTextField = projectInfo.BaseDownloadURL;

            //Debug.LogFormat ("Project path set: {0}", projectRootPath);
        }

        public static Texture2D CreateColorTexture (Color color)
        {
            var size = 1;
            var texture = new Texture2D (size, size);
            Color [] pixels = Enumerable.Repeat (color, size * size).ToArray ();
            texture.SetPixels (pixels);
            texture.Apply ();
            return texture;
        }

        private void OnGUI ()
        {
            if (project == null || projectInfo == null)
            {
                Initiate();
                return;
            }

            try
            {
                scrollPosition = GUILayout.BeginScrollView (scrollPosition);

                GUILayout.BeginVertical ();
                GUILayout.Space (5f);

                #region Styles
                GUIStyle groupStyle = new GUIStyle (EditorStyles.helpBox);
                groupStyle.normal.background = CreateColorTexture (new Color32 (255, 255, 255, 50));
                groupStyle.margin = new RectOffset (0, 50, 0, 0);
                // GUI.backgroundColor = new Color(1, 0.55f, 0);

                GUIStyle groupStyle2 = new GUIStyle (EditorStyles.label);
                //groupStyle.normal.background = CreateColorTexture (new Color32(0, 255, 255, 125));
                //groupStyle2.margin = new RectOffset (0, 0, 0, 0);
                //groupStyle2.stretchWidth = true;

                GUISkin skin = CreateInstance<GUISkin> ();
                skin = GUI.skin;
                skin.label.richText = true;
                skin.button.margin = new RectOffset (50, 50, 0, 0);
                skin.label.normal.textColor = Color.white;
                skin.toggle.margin = new RectOffset (50, 50, 0, 0);
                //skin.label.margin = new RectOffset (50, 50, 0, 0);
                //skin.window.margin = new RectOffset (50, 50, 0, 0);
                GUI.skin = skin;

                GUIStyle title1TextStyle = new GUIStyle (EditorStyles.label);
                title1TextStyle.richText = true;
                title1TextStyle.fontSize = 24;
                title1TextStyle.fontStyle = FontStyle.Bold;
                title1TextStyle.normal.textColor = Color.white;
                title1TextStyle.alignment = TextAnchor.MiddleCenter;
                title1TextStyle.margin = new RectOffset (10, 10, 0, 0);

                GUIStyle title2TextStyle = new GUIStyle (EditorStyles.label);
                title2TextStyle.richText = true;
                title2TextStyle.fontSize = 20;
                title2TextStyle.fontStyle = FontStyle.Bold;
                title2TextStyle.normal.textColor = Color.white;
                title2TextStyle.hover.textColor = Color.yellow;
                title2TextStyle.margin = new RectOffset (20, 0, 0, 0);

                GUIStyle title3TextStyle = new GUIStyle (EditorStyles.label);
                title3TextStyle.richText = true;
                title3TextStyle.fontSize = 16;
                title3TextStyle.fontStyle = FontStyle.Bold;
                title3TextStyle.normal.textColor = Color.red;
                title3TextStyle.alignment = TextAnchor.MiddleCenter;
                title3TextStyle.margin = new RectOffset (10, 10, 0, 0);

                GUIStyle remarkTextStyle = new GUIStyle (EditorStyles.label);
                remarkTextStyle.richText = true;
                remarkTextStyle.fontSize = 15;
                remarkTextStyle.fontStyle = FontStyle.Bold;
                remarkTextStyle.normal.textColor = Color.white;
                remarkTextStyle.margin = new RectOffset (50, 0, 0, 0);
                remarkTextStyle.alignment = TextAnchor.MiddleRight;

                GUIStyle remarkTextStyleHighlight = new GUIStyle (EditorStyles.label);
                remarkTextStyleHighlight.richText = true;
                remarkTextStyleHighlight.fontSize = 15;
                remarkTextStyleHighlight.fontStyle = FontStyle.Bold;
                remarkTextStyleHighlight.normal.textColor = Color.yellow;
                remarkTextStyleHighlight.margin = new RectOffset (50, 0, 0, 0);
                remarkTextStyleHighlight.alignment = TextAnchor.MiddleRight;

                GUIStyle remarkTextStyleHighlight2 = new GUIStyle (EditorStyles.label);
                remarkTextStyleHighlight2.richText = true;
                remarkTextStyleHighlight2.fontSize = 15;
                remarkTextStyleHighlight2.fontStyle = FontStyle.Bold;
                remarkTextStyleHighlight2.normal.textColor = Color.yellow;
                remarkTextStyleHighlight2.margin = new RectOffset (50, 0, 0, 0);
                remarkTextStyleHighlight2.alignment = TextAnchor.MiddleLeft;

                GUIStyle remarkTextStyleHighlightGreen = new GUIStyle (EditorStyles.label);
                remarkTextStyleHighlightGreen.richText = true;
                remarkTextStyleHighlightGreen.fontSize = 15;
                remarkTextStyleHighlightGreen.fontStyle = FontStyle.Bold;
                remarkTextStyleHighlightGreen.normal.textColor = Color.green;
                remarkTextStyleHighlightGreen.margin = new RectOffset (50, 0, 0, 0);
                remarkTextStyleHighlightGreen.alignment = TextAnchor.MiddleRight;

                GUIStyle remarkTextStyle2 = new GUIStyle (EditorStyles.label);
                remarkTextStyle2.richText = true;
                remarkTextStyle2.fontSize = 14;
                remarkTextStyle2.fontStyle = FontStyle.Bold;
                remarkTextStyle2.normal.textColor = Color.white;
                remarkTextStyle2.margin = new RectOffset (0, 0, 0, 0);
                remarkTextStyle2.alignment = TextAnchor.MiddleLeft;

                GUIStyle remarkTextStyle3 = new GUIStyle (EditorStyles.label);
                remarkTextStyle3.richText = true;
                remarkTextStyle3.fontSize = 12;
                remarkTextStyle3.fontStyle = FontStyle.Bold;
                remarkTextStyle3.normal.textColor = Color.white;
                remarkTextStyle3.margin = new RectOffset (50, 0, 0, 0);
                remarkTextStyle3.alignment = TextAnchor.MiddleRight;

                GUIStyle toolbar = new GUIStyle (EditorStyles.toolbar);
                toolbar.richText = true;
                toolbar.fontSize = 16;
                toolbar.fontStyle = FontStyle.Bold;
                toolbar.normal.textColor = Color.white;

                toolbar.normal.background = CreateColorTexture (new Color32 (144, 64, 64, 165));
                toolbar.hover.background = CreateColorTexture (new Color32 (245, 221, 39, 165));
                toolbar.hover.textColor = Color.yellow;
                toolbar.margin = new RectOffset (50, 50, 0, 0);
                toolbar.alignment = TextAnchor.MiddleCenter;

                GUIStyle toolbar2 = new GUIStyle (EditorStyles.toolbar);
                toolbar2.richText = true;
                toolbar2.fontSize = 16;
                toolbar2.fontStyle = FontStyle.Bold;
                toolbar2.normal.textColor = Color.white;

                toolbar2.normal.background = CreateColorTexture (new Color32 (39, 183, 245, 165));
                toolbar2.hover.background = CreateColorTexture (new Color32 (245, 221, 39, 165));
                toolbar2.hover.textColor = Color.yellow;
                toolbar2.margin = new RectOffset (50, 50, 0, 0);
                toolbar2.alignment = TextAnchor.MiddleCenter;

                GUIStyle button = new GUIStyle (EditorStyles.miniButton);
                button.richText = true;
                button.fontSize = 13;
                button.fontStyle = FontStyle.Bold;
                button.normal.textColor = Color.white;
                button.fixedHeight = 25;
                button.margin = new RectOffset (50, 50, 0, 0);

                GUIStyle normalLabel = new GUIStyle (EditorStyles.largeLabel);
                normalLabel.richText = true;
                normalLabel.normal.textColor = Color.white;
                normalLabel.margin = new RectOffset (25, 25, 10, 10);

                GUIStyle textFieldStyle = new GUIStyle (EditorStyles.textField);
                textFieldStyle.richText = true;
                textFieldStyle.alignment = TextAnchor.MiddleCenter;
                textFieldStyle.fontStyle = FontStyle.Bold;
                textFieldStyle.fontSize = 13;

                GUIStyle textFieldStyle2 = new GUIStyle (EditorStyles.textField);
                textFieldStyle2.richText = true;
                textFieldStyle2.alignment = TextAnchor.MiddleLeft;
                textFieldStyle2.fontStyle = FontStyle.Bold;
                textFieldStyle2.fontSize = 13;

                GUIStyle popupStyle = new GUIStyle (EditorStyles.popup);
                popupStyle.richText = true;
                popupStyle.alignment = TextAnchor.MiddleCenter;
                popupStyle.fontStyle = FontStyle.Bold;
                popupStyle.fontSize = 13;
                popupStyle.stretchWidth = false;

                //button.normal.background = CreateColorTexture (new Color32 (39, 183, 245, 165));

                GUIStyle CenterStyle = new GUIStyle (EditorStyles.textField);
                CenterStyle.alignment = TextAnchor.MiddleCenter;
                #endregion Styles

                bool folderSelfPatcherFilesIsEmpty = false;

                string selfPatcherFolder = Path.Combine (projectRootPath, "SelfPatcher");
                if (Directory.GetFiles (selfPatcherFolder, "*").Length == 0)
                {
                    folderSelfPatcherFilesIsEmpty = true;
                }

                // Check if folders are empty 
                bool folderFilesIsEmpty = false;

                if (ModeLauncherWorkspace)
                {
                    string folder = Path.Combine (projectRootPath, "Launcher");
                    if (Directory.GetFiles (folder, "*").Length == 0)
                    {
                        folderFilesIsEmpty = true;
                    }

                }
                else
                {
                    string folder = Path.Combine (projectRootPath, "App");
                    if (Directory.GetFiles (folder, "*").Length == 0)
                    {
                        folderFilesIsEmpty = true;
                    }
                }


                bool newVersionIsValid = false;

                if (new VersionCode (newVersionFromUI).IsValid && !newVersionFromUI.EndsWith ("."))
                {
                    newVersionIsValid = true;
                }

                //// Load background
                Texture2D background = (Texture2D)EditorGUIUtility.Load ("Assets/Plugins/Game Launcher/Editor/Resources/Background.png");
                GUI.DrawTexture (new Rect (0, 0, position.width, position.height), background);

                // Load logo
                Texture2D logo = (Texture2D)EditorGUIUtility.Load ("Assets/Plugins/Game Launcher/Editor/Resources/Game Launcher - Logo with Shadow Small.png");

                // Right Border
                GUI.DrawTexture (new Rect (position.width - (logo.width / 4f), 0, logo.width / 4f, logo.height / 4f), logo, ScaleMode.ScaleToFit, true);

                // Topside
                //GUI.DrawTexture (new Rect (position.width / 3f, 0, logo.width / 2f, logo.height / 2f), logo, ScaleMode.ScaleToFit, true);

                GUI.backgroundColor = Color.black;

                GUILayout.BeginHorizontal ();
                {
                    if (GUILayout.Button ("Documentation", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                        OpenDocumentation ();

                    if (GUILayout.Button ("Discord", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                        OpenDiscord ();
                }
                GUILayout.EndHorizontal ();

                GUI.backgroundColor = Color.grey;

                GUILayout.Space (10);

                // Space

                //GUILayout.Space (logo.height / 2f);

                //GUI.BeginGroup (new Rect(0, 0, position.width, position.height - 20), groupStyle);
                GUILayout.BeginVertical ();


                if (ModeLauncherWorkspace)
                {

                    // Switch to App workspace
                    if (GUILayout.Button ("Switch to App Workspace", toolbar, GUILayout.MaxHeight (40), GUILayout.MaxWidth (500)))
                    {
                        ModeLauncherWorkspace = false;
                        Initialize ();
                    }

                    GUILayout.Space (10);

                    // Workspace folder
                    if (GUILayout.Button ("Workspace Folder", button, GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    {
                        if (ModeLauncherWorkspace)
                        {
                            OpenLauncherWorkspaceFolder ();
                        }
                        else
                        {
                            OpenAppWorkspaceFolder ();
                        }
                    }

                    // Launcher Workspace Title
                    GUILayout.Label (new GUIContent ("Launcher Workspace"), title1TextStyle);
                    DrawHorizontalLine ((int)(position.width * 0.75f));

                }
                else
                {

                    // Switch to Launcher workspace
                    if (ModeLauncherWorkspace = GUILayout.Button ("Switch to Launcher Workspace", toolbar2, GUILayout.MaxHeight (40), GUILayout.MaxWidth (500)))
                    {
                        ModeLauncherWorkspace = true;
                        Initialize ();
                    }

                    GUILayout.Space (10);

                    // Workspace folder
                    if (GUILayout.Button ("Workspace Folder", button, GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    {
                        if (ModeLauncherWorkspace)
                        {
                            OpenLauncherWorkspaceFolder ();
                        }
                        else
                        {
                            OpenAppWorkspaceFolder ();
                        }
                    }

                    // App Workspace Title
                    GUILayout.Label (new GUIContent ("App Workspace"), title1TextStyle);
                    DrawHorizontalLine ((int)(position.width * 0.75f));


                }

                if (GUILayout.Button (new GUIContent ("This manager is out of support and outdated. The new manager is available in Discord"), title3TextStyle, GUILayout.ExpandWidth (true)))
                {
                    Application.OpenURL ("https://discord.com/channels/1050526244556517456/1050526245118541898/1077853790440525835");
                }

                // Host selection
                GUILayout.BeginVertical ();
                {
                    GUILayout.Label (new GUIContent ("Host: "), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                    GUILayout.BeginHorizontal ();
                    {
                        GUILayout.Space (30);

                        popupSelectedHost = EditorGUILayout.Popup ("", popupSelectedHost, hosts.ToArray (), popupStyle, GUILayout.MaxHeight (30));

                        GUILayout.Space (30);
                    }
                    GUILayout.EndHorizontal ();

                }

                if (popupSelectedHost == 1)
                {
                    GUILayout.Label (new GUIContent ("Since Google Drive applied some restrictions, Game Launcher could not download the files correctly, we recommend you to use another Host"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                }

                GUILayout.EndVertical ();

                var newProjectRootPath = "";

                if (ModeLauncherWorkspace == false)
                {

                    GUILayout.Label (new GUIContent ("Environment: "), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                    GUILayout.BeginHorizontal ();
                    {
                        GUILayout.Space (30);

                        popupSelectedEnvironment = EditorGUILayout.Popup ("", popupSelectedEnvironment, environments.ToArray (), popupStyle, GUILayout.MaxHeight (30));

                        newProjectRootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceAppFolderName, environments[popupSelectedEnvironment]);

                        GUILayout.Space (30);
                    }
                    GUILayout.EndHorizontal ();
                }
                else
                {

                    popupSelectedEnvironment = 0;

                    newProjectRootPath = Path.Combine (Path.GetDirectoryName (Application.dataPath), WorkspaceLauncherFolderName);
                }

                if (project != null)
                {
                    if (projectRootPath != newProjectRootPath)
                    {
                        SetProjectPath (newProjectRootPath);
                        GUILayout.EndVertical ();
                        GUILayout.EndVertical ();
                        GUILayout.EndScrollView ();
                        return;
                    }
                }

                GUILayout.BeginHorizontal ();

                GUILayout.Space (30);

                if (popupSelectedHost == 0)
                {
                    GUILayout.Label (new GUIContent ("Host URL"), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));

                    baseDownloadURLTextField = GUILayout.TextField (baseDownloadURLTextField, textFieldStyle2, GUILayout.MaxHeight (30));

                    if (baseDownloadURLTextField.EndsWith ("/") == false && !string.IsNullOrEmpty (baseDownloadURLTextField))
                    {
                        baseDownloadURLTextField += "/";
                    }

                    if (projectInfo != null)
                    {
                        projectInfo.BaseDownloadURL = baseDownloadURLTextField.Replace (environments[popupSelectedEnvironment] + "/", "");
                    }

                    if (project != null && projectInfo != null)
                    {
                        project.SaveProjectInfo (projectInfo);
                    }
                }
                GUILayout.Space (30);

                GUILayout.EndHorizontal ();

                // Comprobations of HOST URL
                // Check start of host_url
                if (!projectInfo.BaseDownloadURL.StartsWith ("http://")
                    && !projectInfo.BaseDownloadURL.StartsWith ("https://")
                    && !projectInfo.BaseDownloadURL.StartsWith ("file:///")
                    && !string.IsNullOrEmpty (projectInfo.BaseDownloadURL))
                {
                    GUILayout.Label (new GUIContent ("Host URL must start with 'http://' or 'https://' or 'file:///'"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                }

                if (!ModeLauncherWorkspace)
                {

                    // Check end of host_url
                    if (!baseDownloadURLTextField.EndsWith ("/App/") && !string.IsNullOrEmpty (projectInfo.BaseDownloadURL))
                    {
                        GUILayout.Label (new GUIContent ("Host URL must end with '/App/'"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                    }

                    // Print the Host URL
                    //GUILayout.Label (new GUIContent (projectInfo.BaseDownloadURL), remarkTextStyle3, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                }
                else
                {

                    // Check end of host_url
                    if (!projectInfo.BaseDownloadURL.EndsWith ("/Launcher/") && !string.IsNullOrEmpty (projectInfo.BaseDownloadURL))
                    {
                        GUILayout.Label (new GUIContent ("Host URL must end with '/Launcher/'"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                    }

                    //GUILayout.Label (new GUIContent (projectInfo.BaseDownloadURL), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                }



                GUILayout.Space (10);

                GUILayout.EndVertical ();

                //GUI.BeginGroup (new Rect(0, 0, position.width, position.height - 20), groupStyle);

                GUILayout.BeginVertical (groupStyle);

                if (GUILayout.Button (new GUIContent ("Self Patcher"), title2TextStyle, GUILayout.ExpandWidth (false)))
                {
                    Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/installation#building-the-self-patcher");
                }


                GUILayout.Label ("Self Patcher provides the self-patcher feature to the launcher", normalLabel);


                GUILayout.BeginHorizontal ();

                if (GUILayout.Button (new GUIContent ("Self Patcher folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                {
                    OpenSelfPatcherFolder ();
                }

                if (folderSelfPatcherFilesIsEmpty)
                {
                    GUILayout.Label ("No SelfPatcher files detected", remarkTextStyleHighlight2);
                }

                GUILayout.EndHorizontal ();

                GUILayout.EndVertical ();

                GUILayout.Space (10);

                GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));

                if (ModeLauncherWorkspace)
                {
                    GUILayout.Label ("1.", title1TextStyle, GUILayout.ExpandWidth (false));
                }
                else
                {
                    GUILayout.Label ("1 & 2.", title1TextStyle, GUILayout.ExpandWidth (false));

                }

                // Start group
                {
                    GUILayout.BeginVertical (groupStyle);

                    if (ModeLauncherWorkspace)
                    {
                        if (GUILayout.Button (new GUIContent ("Deploy your Launcher"), title2TextStyle, GUILayout.ExpandWidth (false)))
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/1.-deploy-your-launcher");
                        }

                        rect = GUILayoutUtility.GetLastRect ();
                        rect.width = title2TextStyle.CalcSize (new GUIContent ("Deploy your Launcher")).x;
                        EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                    }
                    else
                    {
                        if (GUILayout.Button (new GUIContent ("Build your App"), title2TextStyle, GUILayout.ExpandWidth (false)))
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-app-version/1-and-2.-build-your-app");
                        }

                        rect = GUILayoutUtility.GetLastRect ();
                        rect.width = title2TextStyle.CalcSize (new GUIContent ("Build your App")).x;
                        EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);
                    }


                    if (ModeLauncherWorkspace)
                    {
                        GUILayout.BeginHorizontal ();
                        GUILayout.Label ("Compile your Launcher in Visual Studio \n <b>Build -> Publish</b>", normalLabel);
                        GUILayout.EndHorizontal ();
                    }
                    else
                    {
                        GUILayout.Label ("Build your App in the App folder", normalLabel);

                        if (GUILayout.Button (new GUIContent ("Open App folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                        {
                            OpenAppFolder ();
                        }


                    }
                    GUILayout.EndVertical ();
                }
                // End group
                GUILayout.EndHorizontal ();

                GUILayout.Space (10);
                //DrawHorizontalLine ();

                if (ModeLauncherWorkspace)
                {
                    GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));
                    GUILayout.Label ("2.", title1TextStyle, GUILayout.ExpandWidth (false));
                    // Start Group
                    {
                        GUILayout.BeginVertical (groupStyle);

                        if (ModeLauncherWorkspace)
                        {
                            if (GUILayout.Button (new GUIContent ("Move the files"), title2TextStyle, GUILayout.ExpandWidth (false)))
                            {
                                Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/2.-move-your-launcher");
                            }

                            rect = GUILayoutUtility.GetLastRect ();
                            rect.width = title2TextStyle.CalcSize (new GUIContent ("Move the files")).x;
                            EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                            GUILayout.Label ("Move your builded files to the <b><i>Launcher folder</i></b>", normalLabel);
                        }
                        //else {
                        //if (GUILayout.Button (new GUIContent ("Move the files"), title2TextStyle, GUILayout.ExpandWidth (false))) {
                        //    Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/2.-move-your-launcher");
                        //}

                        //rect = GUILayoutUtility.GetLastRect ();
                        //rect.width = title2TextStyle.CalcSize (new GUIContent ("Move the files")).x;
                        //EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                        //GUILayout.Label ("Move your builded files to the App folder", normalLabel);

                        //}

                        //GUILayout.Label (new GUIContent ("Create a new Version Folder, then move the files to the new created folder"), normalLabel);

                        //if (GUILayout.Button (new GUIContent ("Create new version folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200))) {

                        //    if (!newVersionIsValid) {
                        //        Debug.LogWarning ($"'{newVersionFromUI}' is not a valid version");
                        //        return;
                        //    }

                        //    string newVersionFolder = Path.GetFullPath (Path.Combine (projectRootPath, "Versions", newVersion.ToString ()));
                        //    Directory.CreateDirectory (newVersionFolder);

                        //    System.Diagnostics.Process.Start (newVersionFolder);

                        //    Debug.Log ($"New version folder created: {newVersion.ToString ()}");

                        //}

                        GUILayout.BeginHorizontal ();

                        if (ModeLauncherWorkspace)
                        {
                            if (GUILayout.Button (new GUIContent ("Open Launcher folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                            {
                                OpenLauncherFolder ();
                            }
                        }
                        //else {
                        //    if (GUILayout.Button (new GUIContent ("Open App folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200))) {
                        //        OpenAppFolder ();
                        //    }
                        //}

                        GUILayout.EndHorizontal ();

                        GUILayout.EndVertical ();

                    }
                    // End Group
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (10);
                    //DrawHorizontalLine ();
                }

                GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));
                GUILayout.Label ("3.", title1TextStyle, GUILayout.ExpandWidth (false));

                {
                    // Start group
                    GUILayout.BeginVertical (groupStyle);

                    if (GUILayout.Button (new GUIContent ("Generate your patch"), title2TextStyle, GUILayout.ExpandWidth (false)))
                    {
                        if (ModeLauncherWorkspace)
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/3.-create-your-patch");
                        }
                        else
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-app-version/3.-create-your-patch");
                        }
                    }

                    rect = GUILayoutUtility.GetLastRect ();
                    rect.width = title2TextStyle.CalcSize (new GUIContent ("Generate your patch")).x;
                    EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                    GUILayout.Label (new GUIContent ($"Create your new version patch"), normalLabel);

                    GUILayout.BeginHorizontal ();


                    GUILayout.Label (new GUIContent ("From Version: "), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));

                    // Get version
                    //project.versionsPath

                    //bool gotVersion = true;

                    //while (gotVersion) {
                    //    if (Directory.Exists (Path.Combine (projectRootPath, "Versions", currentVersion))) {
                    //        gotVersion = true;
                    //        noCurrentVersion = false;
                    //        //currentVersion = currentVersion[]
                    //    }
                    //    else {
                    //        gotVersion = false;
                    //    }
                    //}

                    //version = new VersionCode ("1.0.0");
                    GetCurrentVersion ();

                    // Show current version
                    if (noCurrentVersion)
                    {
                        GUILayout.Label ("No current version", remarkTextStyle2, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200));
                    }
                    else
                    {
                        //GUILayout.Label (string.Format("'{0}'", currentVersion.ToString ()), remarkTextStyle2, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200));

                        List<string> options = new List<string> ();

                        foreach (VersionCode versionCode in versionCodes)
                        {
                            options.Add (versionCode.ToString ());
                        }

                        var element = options.First ();
                        element += " (Current)";
                        options[0] = element;

                        popUpSelectedVersion = EditorGUILayout.Popup ("", popUpSelectedVersion, options.ToArray (), popupStyle, GUILayout.MaxHeight (30));
                        currentVersion = versionCodes[popUpSelectedVersion];
                    }


                    if (project != null)
                    {

                        if (project.IsRunning == true)
                        {
                            GUILayout.Label (new GUIContent ("Creating patch..."), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                        }
                        else
                        {

                            if (folderFilesIsEmpty)
                            {
                                if (ModeLauncherWorkspace)
                                {
                                    GUILayout.Label (new GUIContent ("No files detected in Launcher folder"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                                }
                                else
                                {
                                    GUILayout.Label (new GUIContent ("No files detected in App folder"), remarkTextStyleHighlight, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                                }
                            }
                            else
                            {
                                GUILayout.Label (new GUIContent ("Ready"), remarkTextStyleHighlightGreen, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));
                            }
                        }
                    }

                    GUILayout.EndHorizontal ();

                    GUILayout.BeginHorizontal ();

                    GUILayout.Label (new GUIContent ("New Version: "), remarkTextStyle, GUILayout.MaxHeight (30), GUILayout.ExpandWidth (false));

                    // Show new version
                    if (newVersionIsValid)
                    {
                        newVersionFromUI = GUILayout.TextField (newVersionFromUI, textFieldStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200));

                        newVersion = new VersionCode (newVersionFromUI);

                    }
                    else
                    {
                        var defaultColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;

                        newVersionFromUI = GUILayout.TextField (newVersionFromUI, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200));
                        GUI.backgroundColor = defaultColor;
                    }

                    if (folderFilesIsEmpty)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button (new GUIContent ("Create Patch"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                    {
                        //string newVersionFolder = Path.GetFullPath(Path.Combine (projectRootPath, "Versions", newVersion.ToString()));

                        //            string rootDirectory = Path.GetFullPath(Path.Combine (projectRootPath, "Versions"));

                        //            DirectoryMove (rootDirectory, newVersionFolder, true);

                        // Check if version is valid
                        if (!newVersionIsValid)
                        {
                            Debug.LogWarning ($"'{newVersionFromUI}' is not a valid version");
                            return;
                        }

                        // Change final folder only for app workspace on custom host
                        if (popupSelectedHost == 0)
                        {
                            if (!ModeLauncherWorkspace)
                            {
                                projectInfo.BaseDownloadURL = baseDownloadURLTextField + environments[popupSelectedEnvironment] + "/";
                            }
                        }
                        else // If it's another host (like Google Drive), then delete the main Host URL
                        {
                            projectInfo.BaseDownloadURL = "";

                        }

                        // Save the new host
                        if (project != null && projectInfo != null)
                        {
                            project.SaveProjectInfo (projectInfo);
                        }

                        string newVersionFolder = Path.GetFullPath (Path.Combine (projectRootPath, "Versions", newVersion.ToString ()));
                        Directory.CreateDirectory (newVersionFolder);
                        Debug.Log ($"New version folder created: {newVersionFolder}");

                        if (ModeLauncherWorkspace)
                        {
                            Debug.LogFormat ("Moving directory: {0} to {1}", Path.GetFullPath (Path.Combine (projectRootPath, "Launcher")), newVersionFolder);
                            DirectoryMove (Path.GetFullPath (Path.Combine (projectRootPath, "Launcher")), newVersionFolder, true);
                        }
                        else
                        {
                            Debug.LogFormat ("Moving directory: {0} to {1}", Path.GetFullPath (Path.Combine (projectRootPath, "App")), newVersionFolder);
                            DirectoryMove (Path.GetFullPath (Path.Combine (projectRootPath, "App")), newVersionFolder, true);
                        }

                        if (project.GeneratePatch ())
                        {
                            Debug.Log ("<b>Game Launcher: OPERATION STARTED</b>");

                            EditorApplication.update -= OnUpdate;
                            EditorApplication.update += OnUpdate;
                        }
                        else
                            Debug.LogWarning ("<b>Couldn't start the operation. Maybe it is already running?</b>");
                    }

                    if (noCurrentVersion)
                    {
                        GUILayout.Label (new GUIContent ($"<b>{newVersion} (First version)</b>"), remarkTextStyle2);
                    }
                    else
                    {
                        GUILayout.Label (new GUIContent ($"'<b>{currentVersion}</b>' to '<b>{newVersion}</b>'"), remarkTextStyle2);
                    }

                    GUI.enabled = true;

                    GUILayout.FlexibleSpace ();
                    GUILayout.EndHorizontal ();
                    GUILayout.EndVertical ();

                }
                // End Group
                GUILayout.EndHorizontal ();

                GUILayout.Space (10);
                //DrawHorizontalLine ();

                GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));
                GUILayout.Label ("4.", title1TextStyle, GUILayout.ExpandWidth (false));

                // Start group

                {
                    GUILayout.BeginVertical (groupStyle);

                    if (GUILayout.Button (new GUIContent ("Upload your files to your server"), title2TextStyle, GUILayout.ExpandWidth (false)))
                    {
                        if (ModeLauncherWorkspace)
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/4.-upload-your-files-to-your-server");
                        }
                        else
                        {
                            Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-app-version/4.-upload-your-files-to-your-server");
                        }
                    }

                    rect = GUILayoutUtility.GetLastRect ();
                    rect.width = title2TextStyle.CalcSize (new GUIContent ("Upload your files to your server")).x;
                    EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                    GUILayout.Label (new GUIContent ("Upload the created files to your server"), normalLabel);
                    GUILayout.BeginHorizontal ();

                    if (ModeLauncherWorkspace)
                    {
                        if (GUILayout.Button (new GUIContent ("Launcher output folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                        {
                            OpenLauncherOutputFolder ();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button (new GUIContent ("App output folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                        {
                            OpenAppOutputFolder ();
                        }
                    }


                    GUILayout.EndVertical ();
                    GUILayout.EndHorizontal ();

                }
                // End Group

                GUILayout.EndHorizontal ();

                if (popupSelectedHost == 1)
                {
                    GUILayout.Space (10);

                    //DrawHorizontalLine ();

                    GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));
                    GUILayout.Label ("5.", title1TextStyle, GUILayout.ExpandWidth (false));

                    // Start group

                    {
                        GUILayout.BeginVertical (groupStyle);

                        if (GUILayout.Button (new GUIContent ("Paste your links in the file"), title2TextStyle, GUILayout.ExpandWidth (false)))
                        {
                            if (ModeLauncherWorkspace)
                            {
                                Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/4.-upload-your-files-to-your-server/google-drive-5.-paste-your-links-in-the-file");
                            }
                            else
                            {
                                Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-app-version/4.-upload-your-files-to-your-server/google-drive-5.-paste-your-links-in-the-file");
                            }
                        }

                        rect = GUILayoutUtility.GetLastRect ();
                        rect.width = title2TextStyle.CalcSize (new GUIContent ("Paste your links in the file")).x;
                        EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                        GUILayout.Label (new GUIContent ("Get the download links and paste it in the file"), normalLabel);
                        GUILayout.BeginHorizontal ();

                        if (GUILayout.Button (new GUIContent ("Open links file"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                        {
                            if (ModeLauncherWorkspace)
                            {
                                OpenLauncherLinksFile ();
                            }
                            else
                            {
                                OpenAppLinksFile (environments[popupSelectedEnvironment]);
                            }
                        }

                        if (GUILayout.Button ("Update Download Links", button, GUILayout.Height (30), GUILayout.MaxWidth (200)))
                        {
                            project.UpdateDownloadLinks ();

                            EditorApplication.update -= OnUpdate;
                            EditorApplication.update += OnUpdate;
                        }

                        GUILayout.EndVertical ();
                        GUILayout.EndHorizontal ();

                    }
                    // End Group

                    GUILayout.EndHorizontal ();

                    GUILayout.Space (10);
                    //DrawHorizontalLine ();

                    GUILayout.BeginHorizontal (groupStyle2, GUILayout.ExpandWidth (false));
                    GUILayout.Label ("6.", title1TextStyle, GUILayout.ExpandWidth (false));

                    // Start group
                    {
                        GUILayout.BeginVertical (groupStyle);


                        if (GUILayout.Button (new GUIContent ("Upload the updated file 'VersionInfo.info' to server"), title2TextStyle, GUILayout.ExpandWidth (false)))
                        {
                            if (ModeLauncherWorkspace)
                            {
                                Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-launcher-version/4.-upload-your-files-to-your-server/google-drive-6.-upload-the-versioninfo.info-file");
                            }
                            else
                            {
                                Application.OpenURL ("https://gamelauncher.gitbook.io/documentation/creating-app-version/4.-upload-your-files-to-your-server/google-drive-6.-upload-the-versioninfo.info-file");
                            }
                        }

                        rect = GUILayoutUtility.GetLastRect ();
                        rect.width = title2TextStyle.CalcSize (new GUIContent ("Upload the updated file 'VersionInfo.info' to server")).x;
                        EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link);

                        GUILayout.Label ("Upload the updated file to server", normalLabel);

                        // GUILayout.Label (new GUIContent (""));
                        GUILayout.BeginHorizontal ();

                        if (GUILayout.Button (new GUIContent ("Open Output folder"), button, GUILayout.MaxHeight (30), GUILayout.MaxWidth (200)))
                        {

                            if (ModeLauncherWorkspace)
                            {
                                OpenLauncherExplorerAndSelectVersionInfoFile ();
                            }
                            else
                            {
                                OpenAppExplorerAndSelectVersionInfoFile (environments[popupSelectedEnvironment]);
                            }
                        }

                        GUILayout.EndVertical ();
                        GUILayout.EndHorizontal ();
                    }
                    // End Group

                    GUILayout.EndHorizontal ();

                    GUILayout.Space (10);
                    //DrawHorizontalLine ();

                    //if( GUILayout.Button( "Sign XMLs", GUILayout.Height( 30 ) ) )
                    //{
                    //	ProjectManager project = new ProjectManager( projectRootPath );
                    //	SecurityUtils.SignXMLsWithKeysInDirectory( project.GetXMLFiles( true, true ), project.utilitiesPath );

                    //	EditorUtility.DisplayDialog( "Security", "Don't share your private key with unknown parties!", "Got it!" );
                    //	Debug.Log( "<b>Operation successful...</b>" );
                    //}

                    //if( GUILayout.Button( "Verify Signed XMLs", GUILayout.Height( 30 ) ) )
                    //{
                    //	string[] invalidXmls;

                    //	ProjectManager project = new ProjectManager( projectRootPath );
                    //	if( !SecurityUtils.VerifyXMLsWithKeysInDirectory( project.GetXMLFiles( true, true ), project.utilitiesPath, out invalidXmls ) )
                    //	{
                    //		Debug.Log( "<b>The following XMLs could not be verified:</b>" );
                    //		for( int i = 0; i < invalidXmls.Length; i++ )
                    //			Debug.Log( invalidXmls[i] );
                    //	}
                    //	else
                    //		Debug.Log( "<b>All XMLs are verified...</b>" );
                    //}

                }
                GUI.enabled = true;

                GUI.backgroundColor = default;
                //GUI.skin = default;

                GUILayout.EndVertical ();
                //GUI.EndGroup ();

                GUILayout.EndScrollView ();
            } catch { }
        }

        private void OnInspectorUpdate ()
        {

            Repaint ();
            

            if (EditorApplication.isCompiling) {
                Close ();
            }
        }

        private void OnUpdate()
		{
			if( project == null )
			{
				EditorApplication.update -= OnUpdate;
				return;
			}

			string log = project.FetchLog();
			while( log != null )
			{
				Debug.Log( log );
				log = project.FetchLog();
			}

			if( !project.IsRunning )
			{
                if (project.Result == PatchResult.Failed) {
                    Debug.Log ("<b><color=red>GAME LAUNCHER: OPERATION FAILED</color></b>");
                }
                else {
                    Debug.Log ("<b><color=green>GAME LAUNCHER: OPERATION SUCESS</color></b>");
                }

                Initiate ();
				project = null;
				EditorApplication.update -= OnUpdate;
			}
		}

		private string PathField( string label, string path, bool isDirectory )
		{
			GUILayout.BeginHorizontal();
			path = EditorGUILayout.TextField( label, path );
			if( GUILayout.Button( "o", GUILayout.Width( 25f ) ) )
			{
				string selectedPath = isDirectory ? EditorUtility.OpenFolderPanel( "Choose a directory", "", "" ) : EditorUtility.OpenFilePanel( "Choose a file", "", "" );
				if( !string.IsNullOrEmpty( selectedPath ) )
					path = selectedPath;

				GUIUtility.keyboardControl = 0; // Remove focus from active text field
			}
			GUILayout.EndHorizontal();

			return path;
		}

		private void CheckProjectExists(bool launcher)
		{

            projectRootPath = projectRootPath == null ? "" : projectRootPath.Trim();

			if (!Directory.EnumerateFileSystemEntries(projectRootPath).Any()) {

                project = new ProjectManager (projectRootPath);
                project.CreateProject ();

                ProjectInfo projectInfo = project.LoadProjectInfo ();
                if (launcher) {
                    Debug.Log ("Creating launcher project...");

                    projectInfo.Name = "Launcher";
                } else {

                    Debug.Log ("Creating app project...");

                    projectInfo.Name = "App";
                    projectInfo.IgnoredPaths.Add("SelfUpdater\\");
                }

                projectInfo.IgnoredPaths.Add ("*output_log.txt");
                project.SaveProjectInfo (projectInfo);

                //EditorApplication.update -= OnUpdate;
                //EditorApplication.update += OnUpdate;

                DirectoryInfo projectDir = new DirectoryInfo (projectRootPath);

                if (launcher) {
                    // Create Launcher path
                    string launcherPath = Path.Combine (projectRootPath, "Launcher");
                    if (!Directory.Exists (launcherPath)) {
                        Directory.CreateDirectory (launcherPath);
                    }
                } else {
                    // Create App path
                    string appPath = Path.Combine (projectRootPath, "App");
                    if (!Directory.Exists (appPath)) {
                        Directory.CreateDirectory (appPath);
                    }
                }

                Debug.LogWarning ($"<color=yellow>Game Launcher Project Folder {(launcher ? "'Launcher'" : "'App'")} created</color>");
            } else {
                Debug.Log ("Project exists");
            }

        }


        void OnFocus ()
        {
            if (EditorPrefs.HasKey ("ModeLauncherWorkspace"))
                ModeLauncherWorkspace = EditorPrefs.GetBool ("ModeLauncherWorkspace");

            if (EditorPrefs.HasKey ("PopupSelectedHost"))
                popupSelectedHost = EditorPrefs.GetInt ("PopupSelectedHost");
        }

        void OnLostFocus ()
        {
            EditorPrefs.SetBool ("ModeLauncherWorkspace", ModeLauncherWorkspace);

            EditorPrefs.SetInt ("PopupSelectedHost", popupSelectedHost);
        }
        void OnDestroy ()
        {
            EditorPrefs.SetBool ("ModeLauncherWorkspace", ModeLauncherWorkspace);

            EditorPrefs.SetInt ("PopupSelectedHost", popupSelectedHost);

        }

        private void DrawHorizontalLine(int size = 0)
		{
			GUILayout.Space( 5 );

            if (size == 0) {
                GUILayout.Box ("", GUILayout.ExpandWidth(true), GUILayout.Height (1));
            } else {
                GUILayout.Box ("", GUILayout.Width(size), GUILayout.Height (1));
            }

            GUILayout.Space( 5 );
		}

        private static void DirectoryMove (string sourceDirName, string destDirName, bool preserveSubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo (sourceDirName);

            if (!dir.Exists) {
                throw new DirectoryNotFoundException (
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo [] dirs = dir.GetDirectories ();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists (destDirName)) {
                Directory.CreateDirectory (destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo [] files = dir.GetFiles ();
            foreach (FileInfo file in files) {
                string temppath = Path.Combine (destDirName, file.Name);
                file.MoveTo (temppath);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (preserveSubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine (destDirName, subdir.Name);
                    DirectoryMove (subdir.FullName, temppath, preserveSubDirs);
                    if (Directory.GetFiles(subdir.FullName).Length == 0) {
                        FileSafety.DirectoryDelete (subdir.FullName);
                    }
                }
            }
        }
    }

}