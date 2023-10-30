using GameLauncher;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GameLauncher {

    [InitializeOnLoad]
    public class Wizard : EditorWindow
    {
        static Wizard()
        {
            EditorApplication.update += CheckWindow;
        }

        public static void CheckWindow()
        {
            if (!PlayerPrefs.HasKey("GameLauncherWelcome"))
            {
                //Show existing window instance. If one doesn't exist, make one.
                ShowWindow();

                PlayerPrefs.SetString("GameLauncherWelcome", "true");
                PlayerPrefs.Save();

            }
        }

        [MenuItem("Tools/Game Launcher/Getting Started", false, -100)]
        public static void ShowWindow()
        {
            Wizard wizard = GetWindow<Wizard>();
            wizard.titleContent = new GUIContent("Getting Started");
            wizard.minSize = new Vector2(482, 676f);
        }

        void OnGUI()
        {

            GUIStyle groupStyle = new GUIStyle(EditorStyles.helpBox);
            groupStyle.normal.background = AdminWindow.CreateColorTexture(new Color32(255, 255, 255, 50));
            groupStyle.margin = new RectOffset(20, 50, 0, 0);
            // GUI.backgroundColor = new Color(1, 0.55f, 0);

            GUIStyle groupStyle2 = new GUIStyle(EditorStyles.label);
            //groupStyle.normal.background = CreateColorTexture (new Color32(0, 255, 255, 125));
            //groupStyle2.margin = new RectOffset (0, 0, 0, 0);
            //groupStyle2.stretchWidth = true;

            GUIStyle normalLabel = new GUIStyle(EditorStyles.largeLabel);
            normalLabel.richText = true;
            //normalLabel.normal.textColor = Color.white;
            normalLabel.margin = new RectOffset(25, 25, 10, 10);

            GUIStyle title1TextStyle = new GUIStyle(EditorStyles.label);
            title1TextStyle.richText = true;
            title1TextStyle.fontSize = 24;
            title1TextStyle.fontStyle = FontStyle.Bold;
            title1TextStyle.normal.textColor = Color.white;
            title1TextStyle.alignment = TextAnchor.MiddleCenter;
            title1TextStyle.margin = new RectOffset(10, 10, 0, 0);

            GUIStyle title2TextStyle = new GUIStyle(EditorStyles.label);
            title2TextStyle.richText = true;
            title2TextStyle.fontSize = 20;
            title2TextStyle.fontStyle = FontStyle.Bold;
            title2TextStyle.normal.textColor = Color.white;
            title2TextStyle.hover.textColor = Color.yellow;
            title2TextStyle.margin = new RectOffset(20, 0, 0, 0);

            GUILayout.BeginVertical();

            // Load logo
            Texture2D logo = (Texture2D)EditorGUIUtility.Load("Assets/Plugins/Game Launcher/Editor/Resources/Game Launcher - Logo with Shadow Small.png");

            //// Load background
            Texture2D background = (Texture2D)EditorGUIUtility.Load ("Assets/Plugins/Game Launcher/Editor/Resources/Background.png");
            GUI.DrawTexture (new Rect (0, 0, position.width, position.height), background);

            // Right Border
            GUI.DrawTexture (new Rect (position.width - (logo.width / 4f), 0, logo.width / 4f, logo.height / 4f), logo, ScaleMode.ScaleToFit, true);

            if (GUILayout.Button("Documentation", GUILayout.Height(25), GUILayout.MaxWidth(200)))
                AdminWindow.OpenDocumentation();

            GUILayout.Space(50);
            GUILayout.Label("Getting Started", title1TextStyle);
            GUILayout.Space(10);

            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical(groupStyle);
            {
                GUILayout.Space(10);

                GUILayout.Label("Installation", title2TextStyle);
                GUILayout.Label("Start here to prepare the Game Launcher environment", normalLabel);

                if (GUILayout.Button("Installation", GUILayout.Height(25), GUILayout.MaxWidth(200)))
                    AdminWindow.OpenInstallation();

                GUI.backgroundColor = Color.grey;
                GUILayout.Space(10);
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical(groupStyle);
            {
                GUILayout.Space(10);

                GUILayout.Label("Support", title2TextStyle);
                GUILayout.Label("Did you require special features?\r\nProblems with the Launcher?\r\nNeed help?", normalLabel);
                GUILayout.Label("carlosarturors@gmail.com", normalLabel);

                if (GUILayout.Button("Contact", GUILayout.Height(25), GUILayout.MaxWidth(200)))
                    AdminWindow.OpenDocumentation();

                GUI.backgroundColor = Color.grey;
                GUILayout.Space(10);
            }
            GUILayout.EndVertical();

            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical (groupStyle);
            {
                GUILayout.Space (10);

                GUILayout.Label ("Discord community and Github updates", title2TextStyle);
                GUILayout.Label ("Join discord and send your invoice number to get\r\nthe lastest Game Launcher Updates", normalLabel);

                if (GUILayout.Button ("Discord", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    AdminWindow.OpenDiscord ();

                GUI.backgroundColor = Color.grey;
                GUILayout.Space (10);
            }
            GUILayout.EndVertical ();

            GUI.backgroundColor = Color.black;

            GUILayout.BeginVertical (groupStyle);
            {
                GUILayout.Space (10);

                GUILayout.Label ("Other products", title2TextStyle);
                GUILayout.Label ("Do you want to manage multiple games in one launcher?\r\ntry the Enterprise Version", normalLabel);
                GUILayout.Label ("Also, you can buy new templates and integrations\r\nfor your launcher", normalLabel);

                if (GUILayout.Button ("Game Launcher Enterprise", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    AdminWindow.OpenGameLauncherEnterprise ();

                GUI.backgroundColor = Color.grey;
                GUILayout.Space (10);
            }
            GUILayout.EndVertical ();

            GUILayout.BeginVertical(groupStyle);
            {
                GUILayout.Space(10);

                GUILayout.Label("Developed by Carlos Arturo Rodriguez Silva", normalLabel);

                GUI.backgroundColor = Color.grey;
            }
            GUILayout.EndVertical();

            GUILayout.EndVertical();

        }
    }
}