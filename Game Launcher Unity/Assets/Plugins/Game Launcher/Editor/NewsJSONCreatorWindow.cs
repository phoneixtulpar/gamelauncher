using GameLauncherCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using GameLauncher.Utilities;

namespace GameLauncher
{
    public class NewsJSONCreatorWindow : EditorWindow
    {
        private string projectRootPath;
        private Vector2 scrollPosition;

        private ProjectManager project;

        bool ModeLauncherWorkspace = true;

        List<bool> alertsCollapsedList = new List<bool> ();
        List<bool> newsCollapsedList = new List<bool> ();
        List<bool> subNewsCollapsedList = new List<bool> ();

        // string newVersionFromUI = "1.0.0";

        List<VersionCode> versionCodes = new List<VersionCode> ();

        #region Main Data

        List<string> languages = new List<string> {
            "en_US",
            "es_MX"
        };

        int popUpSelectedLanguage = 0;

        List<string> environments = new List<string> {
            "Release",
            "Beta"
        };

        int popUpSelectedEnvironment = 0;

        List<string> regions = new List<string> {
            "Any",
        };

        List<string> serverStatus = new List<string> {
            "Online",
            "Offline"
        };

        #endregion

        #region Alerts
        List<string> alertTypes = new List<string> {
            "Information",
            "Warning"
        };

        #endregion

        int popupSelectedHost = 0;

        NewsContent newsContent;

        [MenuItem ("Tools/Game Launcher/News JSON Creator", false, 111)]
        private static void Initialize ()
        {
            NewsJSONCreatorWindow window = GetWindow<NewsJSONCreatorWindow> ();
            window.titleContent = new GUIContent ("News JSON Creator");
            window.minSize = new Vector2 (1400f, 850f);

            window.Initiate ();
            window.Show ();
        }
        private void Initiate ()
        {
            //CheckIfWorkspaceFolderExists ();
            //GetCurrentVersion ();

            newsContent = new NewsContent ();
            newsContent.Alerts = new List<Alerts> { new Alerts () };
            newsContent.News = new List<News> { new News () };
            newsContent.SubNews = new List<SubNews> { new SubNews () };

            InitiateValues ();
        }

        private void InitiateValues ()
        {

            newsCollapsedList.Clear ();
            alertsCollapsedList.Clear ();
            subNewsCollapsedList.Clear ();

            // Add values to alerts collapsable data
            for (int i = 0; i < newsContent.Alerts.Count; i++)
            {
                alertsCollapsedList.Add (true);
            }

            // Add values to news collapsable data
            for (int i = 0; i < newsContent.News.Count; i++)
            {
                newsCollapsedList.Add (true);
            }

            // Add values to subnews collapsable data
            for (int i = 0; i < newsContent.SubNews.Count; i++)
            {
                subNewsCollapsedList.Add (true);
            }
        }

        private static Texture2D CreateColorTexture (Color color)
        {
            var size = 1;
            var texture = new Texture2D (size, size);
            Color[] pixels = Enumerable.Repeat (color, size * size).ToArray ();
            texture.SetPixels (pixels);
            texture.Apply ();
            return texture;
        }

        private void OnGUI ()
        {

            if (newsContent == null)
            {
                Initiate ();
            }

            scrollPosition = GUILayout.BeginScrollView (scrollPosition);

            GUIStyle groupStyle = new GUIStyle (EditorStyles.helpBox);
            groupStyle.normal.background = CreateColorTexture (new Color32 (255, 255, 255, 50));
            groupStyle.margin = new RectOffset (50, 50, 0, 0);
            //groupStyle.border = new RectOffset(10, 10, 10, 10);
            // GUI.backgroundColor = new Color(1, 0.55f, 0);

            GUIStyle groupStyle2 = new GUIStyle (EditorStyles.helpBox);
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

            GUIStyle remarkTextStyle2 = new GUIStyle (EditorStyles.label);
            remarkTextStyle2.richText = true;
            remarkTextStyle2.fontSize = 14;
            remarkTextStyle2.fontStyle = FontStyle.Bold;
            remarkTextStyle2.normal.textColor = Color.white;
            remarkTextStyle2.margin = new RectOffset (0, 0, 0, 0);
            remarkTextStyle2.alignment = TextAnchor.MiddleLeft;

            GUIStyle textFieldStyle2 = new GUIStyle (EditorStyles.textField);
            textFieldStyle2.richText = true;
            textFieldStyle2.alignment = TextAnchor.MiddleLeft;
            textFieldStyle2.fontStyle = FontStyle.Bold;
            textFieldStyle2.fontSize = 13;

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
            normalLabel.margin = new RectOffset (25, 0, 0, 0);

            GUIStyle toggle = new GUIStyle (EditorStyles.toggle);
            toggle.margin = new RectOffset (25, 0, 0, 0);

            GUIStyle textFieldStyle = new GUIStyle (EditorStyles.textField);
            textFieldStyle.richText = true;
            textFieldStyle.alignment = TextAnchor.MiddleLeft;
            textFieldStyle.fontStyle = FontStyle.Bold;
            textFieldStyle.fontSize = 13;
            textFieldStyle.margin = new RectOffset (0, 25, 0, 0);
            textFieldStyle.fixedWidth = 500;

            GUIStyle textFieldStyleNoFixed = new GUIStyle (EditorStyles.textField);
            textFieldStyleNoFixed.richText = true;
            textFieldStyleNoFixed.alignment = TextAnchor.MiddleLeft;
            textFieldStyleNoFixed.fontStyle = FontStyle.Bold;
            textFieldStyleNoFixed.fontSize = 13;
            textFieldStyleNoFixed.margin = new RectOffset (0, 25, 0, 0);

            GUIStyle popupStyle = new GUIStyle (EditorStyles.popup);
            popupStyle.richText = true;
            popupStyle.alignment = TextAnchor.MiddleCenter;
            popupStyle.fontStyle = FontStyle.Bold;
            popupStyle.fontSize = 13;
            popupStyle.stretchWidth = false;


            //button.normal.background = CreateColorTexture (new Color32 (39, 183, 245, 165));

            GUIStyle CenterStyle = new GUIStyle (EditorStyles.textField);
            CenterStyle.alignment = TextAnchor.MiddleCenter;


            //// Load background
            Texture2D background = (Texture2D)EditorGUIUtility.Load ("Assets/Plugins/Game Launcher/Editor/Resources/Background.png");
            GUI.DrawTexture (new Rect (0, 0, position.width, position.height), background);

            // Load logo
            Texture2D logo = (Texture2D)EditorGUIUtility.Load ("Assets/Plugins/Game Launcher/Editor/Resources/Game Launcher Complete Logo No Background Shadow.png");

            // Right Border
            GUI.DrawTexture (new Rect (position.width - (logo.width / 4f), 0, logo.width / 4f, logo.height / 4f), logo, ScaleMode.ScaleToFit, true);

            // Topside
            //GUI.DrawTexture (new Rect (position.width / 3f, 0, logo.width / 2f, logo.height / 2f), logo, ScaleMode.ScaleToFit, true);

            GUI.backgroundColor = Color.black;

            GUILayout.BeginHorizontal ();
            {
                if (GUILayout.Button ("Documentation", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    AdminWindow.OpenDocumentation ();

                if (GUILayout.Button ("Discord", GUILayout.Height (25), GUILayout.MaxWidth (200)))
                    AdminWindow.OpenDiscord ();
            }
            GUILayout.EndHorizontal ();


            GUI.backgroundColor = Color.grey;


            GUILayout.Label (new GUIContent ("News JSON Creator"), title1TextStyle);

            GUILayout.Space (10);

            if (GUILayout.Button (new GUIContent ("This manager is out of support and outdated. The new manager is available in Discord"), title3TextStyle, GUILayout.ExpandWidth (true)))
            {
                Application.OpenURL ("https://discord.com/channels/1050526244556517456/1050526245118541898/1077853790440525835");
            }

            /// Load and Export JSON Layout
            //#region Load and Export JSON Layout
            //GUILayout.BeginHorizontal ();

            //GUILayout.FlexibleSpace ();

            //if (GUILayout.Button ("Load JSON", button, GUILayout.Height (35), GUILayout.MaxWidth (200))) {
            //    // Action
            //}

            //if (GUILayout.Button ("Export JSON", button, GUILayout.Height (35), GUILayout.MaxWidth (200))) {
            //    // Action
            //}

            //GUILayout.FlexibleSpace ();


            //GUILayout.EndHorizontal ();

            //// END // Load and Export JSON Layout
            //#endregion

            #region Horizontal Line
            GUI.backgroundColor = Color.white;
            DrawHorizontalLine ((int)(position.width));
            GUI.backgroundColor = Color.gray;
            #endregion Horizontal Line

            GUILayout.BeginHorizontal ();
            GUILayout.BeginVertical ();

            #region New File Name
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();

            GUILayout.Label (new GUIContent ($"<b>File Name:</b> {$"{languages[popUpSelectedLanguage]}_{environments[popUpSelectedEnvironment]}_News.txt"}"), normalLabel);

            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            #endregion

            #region Language & Environment

            GUILayout.Label (new GUIContent ($"<b>Language & Environment</b>"), normalLabel);

            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
            #endregion

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            #region Language
            int languageIndex = languages.FindIndex (s => s.Contains (newsContent.Language));

            if (languageIndex == -1)
            {
                languageIndex = 0;
            }

            languageIndex = EditorGUILayout.Popup ("", languageIndex, languages.ToArray (), popupStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (300));
            newsContent.Language = languages[languageIndex];
            popUpSelectedLanguage = languageIndex;
            #endregion

            #region Environment
            int environmentIndex = environments.FindIndex (s => s.Contains (newsContent.Environment));

            if (environmentIndex == -1)
            {
                environmentIndex = 0;
            }

            environmentIndex = EditorGUILayout.Popup ("", environmentIndex, environments.ToArray (), popupStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (300));
            newsContent.Environment = environments[environmentIndex];
            popUpSelectedEnvironment = environmentIndex;

            #endregion

            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            #region Main Data
            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label ("Main Data", title2TextStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            // Project Name
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Project Name", normalLabel, GUILayout.ExpandWidth (false));
            newsContent.ProjectName = GUILayout.TextField (newsContent.ProjectName, 50, textFieldStyle);
            GUILayout.EndHorizontal ();

            GUILayout.Space (5);

            // Region
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Region", normalLabel, GUILayout.ExpandWidth (false));

            int regionIndex = regions.FindIndex (s => s.Contains (newsContent.Region));

            if (regionIndex == -1)
            {
                regionIndex = 0;
            }

            regionIndex = EditorGUILayout.Popup ("", regionIndex, regions.ToArray (), popupStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (300));
            newsContent.Region = regions[regionIndex];
            GUILayout.EndHorizontal ();

            GUILayout.Space (5);

            // Server Status
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Server Status", normalLabel, GUILayout.ExpandWidth (false));

            int serverStatusIndex = serverStatus.FindIndex (s => s.Contains (newsContent.ServerStatus));

            if (serverStatusIndex == -1)
            {
                serverStatusIndex = 0;
            }

            serverStatusIndex = EditorGUILayout.Popup ("", serverStatusIndex, serverStatus.ToArray (), popupStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (300));
            newsContent.ServerStatus = serverStatus[serverStatusIndex];
            GUILayout.EndHorizontal ();

            GUILayout.Space (5);

            // MY ACCOUNT URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("My Account URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.MyAccountURL = GUILayout.TextField (newsContent.MyAccountURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);

            // WEBPAGE URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Webpage URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.WebpageURL = GUILayout.TextField (newsContent.WebpageURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);

            // PATCH NOTES URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Patch Notes URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.PatchNotesURL = GUILayout.TextField (newsContent.PatchNotesURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);

            // Report Bug URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Report Bug URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.ReportBugURL = GUILayout.TextField (newsContent.ReportBugURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);

            // TERMS OF SERVICE URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Terms of Service URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.TermsOfServiceURL = GUILayout.TextField (newsContent.TermsOfServiceURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);

            // PRIVACY Policy URL
            GUILayout.BeginHorizontal ();
            {
                GUILayout.Label ("Privacy Policy URL", normalLabel, GUILayout.ExpandWidth (false));
                newsContent.PrivacyPolicyURL = GUILayout.TextField (newsContent.PrivacyPolicyURL, textFieldStyle);
                GUILayout.EndHorizontal ();
            }
            GUILayout.Space (10);


            GUILayout.EndVertical ();

            #endregion Main Data

            GUILayout.Space (20);
            #region Alerts

            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label ("Alerts", title2TextStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            // Elements


            for (int i = 0; i < newsContent.Alerts.Count; i++)
            {
                GUI.backgroundColor = Color.white;

                // 1 Begin Vertical
                GUILayout.BeginVertical (groupStyle2, GUILayout.ExpandWidth (false));
                GUI.backgroundColor = Color.gray;


                GUILayout.BeginHorizontal ();
                GUILayout.Space (10);

                GUILayout.Label ($"<b>#{i}</b>", normalLabel, GUILayout.ExpandWidth (false));

                int alertTypeIndex = alertTypes.FindIndex (s => s.Contains (newsContent.Alerts[i].type));

                if (alertTypeIndex == -1)
                {
                    alertTypeIndex = 0;
                }

                var alertSubstring = newsContent.Alerts[i].message;

                if (alertSubstring.Length > 50)
                {
                    alertSubstring = newsContent.Alerts[i].message.Substring (0, 50);
                }

                alertsCollapsedList[i] = EditorGUILayout.Foldout (alertsCollapsedList[i], $"Collapse | [{alertTypes[alertTypeIndex]}] {alertSubstring}...");

                GUILayout.EndHorizontal ();

                GUILayout.Space (10);

                if (alertsCollapsedList[i] == false)
                {
                    GUILayout.EndVertical ();
                    continue;
                }

                if (alertsCollapsedList[i] == true)
                {

                    // Alert Type
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Alert Type", normalLabel, GUILayout.ExpandWidth (false));

                    if (alertTypeIndex == 0)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }
                    else if (alertTypeIndex == 1)
                    {
                        GUI.backgroundColor = Color.yellow;

                    }

                    alertTypeIndex = EditorGUILayout.Popup ("", alertTypeIndex, alertTypes.ToArray (), popupStyle, GUILayout.MaxHeight (30), GUILayout.MaxWidth (300));

                    newsContent.Alerts[i].type = alertTypes[alertTypeIndex];

                    GUI.backgroundColor = Color.gray;

                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Date
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Date", normalLabel, GUILayout.ExpandWidth (false));
                    //newsContent.Alerts [i].date = GUILayout.TextField (newsContent.Alerts [i].date, textFieldStyle);
                    DateTime dateFromISO = DateTime.UtcNow;
                    try
                    {
                        dateFromISO = DateTime.Parse (newsContent.Alerts[i].date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    catch
                    {

                    }

                    newsContent.Alerts[i].dateYear = dateFromISO.Year;
                    newsContent.Alerts[i].dateMonth = dateFromISO.Month;
                    newsContent.Alerts[i].dateDay = dateFromISO.Day;
                    newsContent.Alerts[i].dateHour = dateFromISO.Hour;
                    newsContent.Alerts[i].dateMinute = dateFromISO.Minute;


                    GUILayout.Label ("Year", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].dateYear = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].dateYear, 1000, 9999), textFieldStyleNoFixed);

                    GUILayout.Label ("Month", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].dateMonth = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].dateMonth, 1, 99), textFieldStyleNoFixed);

                    GUILayout.Label ("Day", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].dateDay = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].dateDay, 1, 99), textFieldStyleNoFixed);

                    GUILayout.Label ("Hour", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].dateHour = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].dateHour, 0, 99), textFieldStyleNoFixed);

                    GUILayout.Label ("Minutes", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].dateMinute = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].dateMinute, 0, 99), textFieldStyleNoFixed);

                    DateTime dt = new DateTime ();
                    try
                    {
                        dt = new DateTime (
                            Mathf.Clamp (newsContent.Alerts[i].dateYear, 1000, 9999),
                            Mathf.Clamp (newsContent.Alerts[i].dateMonth, 1, 12),
                            Mathf.Clamp (newsContent.Alerts[i].dateDay, 1, 31),
                            Mathf.Clamp (newsContent.Alerts[i].dateHour, 0, 23),
                            Mathf.Clamp (newsContent.Alerts[i].dateMinute, 0, 59),
                            0);

                        newsContent.Alerts[i].validDate = true;
                    }
                    catch
                    {
                        newsContent.Alerts[i].validDate = false;
                    }

                    newsContent.Alerts[i].date = dt.ToString ("O");
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Message
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Title", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].title = GUILayout.TextField (newsContent.Alerts[i].title, textFieldStyle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Message
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Message", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].message = GUILayout.TextField (newsContent.Alerts[i].message, textFieldStyle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Show after date
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Show after date?", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].showAfterDateBool = GUILayout.Toggle (newsContent.Alerts[i].showAfterDateBool, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.Alerts[i].validShowAfterDate == false)
                    {
                        GUILayout.Label ($"<b><color=yellow>Invalid date</color></b>", normalLabel, GUILayout.ExpandWidth (false));
                    }
                    if (newsContent.Alerts[i].showAfterDateBool == true)
                    {
                        GUILayout.BeginHorizontal ();

                        DateTime dateFromISO2 = DateTime.UtcNow;
                        try
                        {
                            dateFromISO2 = DateTime.Parse (newsContent.Alerts[i].showAfterDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        }
                        catch
                        {

                        }

                        newsContent.Alerts[i].showAfterDateYear = dateFromISO2.Year;
                        newsContent.Alerts[i].showAfterDateMonth = dateFromISO2.Month;
                        newsContent.Alerts[i].showAfterDateDay = dateFromISO2.Day;
                        newsContent.Alerts[i].showAfterDateHour = dateFromISO2.Hour;
                        newsContent.Alerts[i].showAfterDateMinute = dateFromISO2.Minute;


                        GUILayout.Label ("Year", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.Alerts[i].showAfterDateYear = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].showAfterDateYear, 1000, 9999), textFieldStyleNoFixed);

                        GUILayout.Label ("Month", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.Alerts[i].showAfterDateMonth = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].showAfterDateMonth, 1, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Day", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.Alerts[i].showAfterDateDay = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].showAfterDateDay, 1, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Hour", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.Alerts[i].showAfterDateHour = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].showAfterDateHour, 0, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Minutes", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.Alerts[i].showAfterDateMinute = EditorGUILayout.IntField (Mathf.Clamp (newsContent.Alerts[i].showAfterDateMinute, 0, 99), textFieldStyleNoFixed);

                        DateTime dt2 = new DateTime ();
                        try
                        {
                            dt2 = new DateTime (
                                Mathf.Clamp (newsContent.Alerts[i].showAfterDateYear, 1000, 9999),
                                Mathf.Clamp (newsContent.Alerts[i].showAfterDateMonth, 1, 12),
                                Mathf.Clamp (newsContent.Alerts[i].showAfterDateDay, 1, 31),
                                Mathf.Clamp (newsContent.Alerts[i].showAfterDateHour, 0, 23),
                                Mathf.Clamp (newsContent.Alerts[i].showAfterDateMinute, 0, 59),
                                0);

                            newsContent.Alerts[i].validShowAfterDate = true;
                        }
                        catch
                        {
                            newsContent.Alerts[i].validShowAfterDate = false;
                        }

                        newsContent.Alerts[i].showAfterDate = dt2.ToString ("O");
                        GUILayout.EndHorizontal ();
                    }


                    //if (newsContent.Alerts [i].showAfterDateBool) {

                    //    GUILayout.BeginHorizontal ();
                    //    GUILayout.Space (30);
                    //    newsContent.Alerts [i].showAfterDate = GUILayout.TextField (newsContent.Alerts [i].showAfterDate, textFieldStyle);

                    //    GUILayout.EndHorizontal ();

                    //}
                    GUILayout.Space (5);

                    // Interaction URL

                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Interaction URL", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.Alerts[i].interactionURL = GUILayout.TextField (newsContent.Alerts[i].interactionURL, textFieldStyle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (10);

                    // Move Up / Move Down / Delete

                    GUILayout.BeginHorizontal ();

                    if (i == 0)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button ("Move Up", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        int actualIndex = i;

                        var element = (Alerts)Extensions.DeepClone (newsContent.Alerts[i]);

                        newsContent.Alerts.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsContent.Alerts.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), element);

                        var currentValueAlertsToShow = alertsCollapsedList[i];
                        alertsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        alertsCollapsedList.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), currentValueAlertsToShow);
                        break;
                    }

                    GUI.enabled = true;

                    if (i == newsContent.Alerts.Count - 1)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button ("Move Down", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        int actualIndex = i;

                        var element = (Alerts)Extensions.DeepClone (newsContent.Alerts[i]);

                        newsContent.Alerts.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsContent.Alerts.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.Alerts.Count), element);

                        var currentValueAlertsToShow = alertsCollapsedList[i];
                        alertsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        alertsCollapsedList.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.Alerts.Count), currentValueAlertsToShow);

                        break;
                    }

                    GUI.enabled = true;

                    GUI.backgroundColor = Color.red;

                    if (GUILayout.Button ("Delete", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        newsContent.Alerts.RemoveAt (i);
                        alertsCollapsedList.RemoveAt (i);
                    }

                    GUI.backgroundColor = Color.gray;

                    GUILayout.EndHorizontal ();

                    GUILayout.EndVertical ();

                    GUILayout.Space (10);
                }
            }

            GUILayout.BeginHorizontal ();

            // Add

            if (GUILayout.Button ("Add", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
            {
                // Action
                newsContent.Alerts.Add (new Alerts ());

                alertsCollapsedList.Add (true);
            }

            GUILayout.EndHorizontal ();


            GUILayout.Space (10);

            GUILayout.EndVertical ();

            GUILayout.Space (10);
            #endregion Alerts

            #region News
            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label ("News", title2TextStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            // Elements

            for (int i = 0; i < newsContent.News.Count; i++)
            {

                GUI.backgroundColor = Color.white;

                // 1 Begin Vertical
                GUILayout.BeginVertical (groupStyle2, GUILayout.ExpandWidth (false));
                GUI.backgroundColor = Color.gray;

                GUILayout.Space (10);

                GUILayout.BeginHorizontal ();

                GUILayout.Label ($"<b>#{i}</b>", normalLabel, GUILayout.ExpandWidth (false));

                GUILayout.Space (10);

                var newsSubstring = newsContent.News[i].title;

                if (newsSubstring.Length > 50)
                {
                    newsSubstring = newsContent.News[i].title.Substring (0, 50);
                }

                newsCollapsedList[i] = EditorGUILayout.Foldout (newsCollapsedList[i], $"Collapse | {newsSubstring}...");

                GUILayout.EndHorizontal ();

                GUILayout.Space (10);

                if (newsCollapsedList[i] == false)
                {

                    GUILayout.EndVertical ();

                    continue;
                }

                if (newsCollapsedList[i])
                {

                    // Header
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Header</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showHeader = GUILayout.Toggle (newsContent.News[i].showHeader, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showHeader == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Header", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].header = GUILayout.TextField (newsContent.News[i].header, textFieldStyle);
                        GUILayout.EndHorizontal ();
                    }


                    GUILayout.Space (10);

                    // Title
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Title</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showTitle = GUILayout.Toggle (newsContent.News[i].showTitle, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showTitle == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Title", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].title = GUILayout.TextField (newsContent.News[i].title, textFieldStyle);
                        GUILayout.EndHorizontal ();
                    }


                    GUILayout.Space (10);

                    // Subtitle
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show SubTitle</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showSubTitle = GUILayout.Toggle (newsContent.News[i].showSubTitle, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showSubTitle == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("SubTitle", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].subtitle = GUILayout.TextField (newsContent.News[i].subtitle, textFieldStyle);
                        GUILayout.EndHorizontal ();
                    }


                    GUILayout.Space (10);

                    // Subtitle Custom Color (HEX)
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Custom SubTitle Color</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].subtitleCustomColor = GUILayout.Toggle (newsContent.News[i].subtitleCustomColor, "", toggle);

                    GUILayout.EndHorizontal ();


                    if (newsContent.News[i].subtitleCustomColor == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Subtitle Custom Color (HEX)", normalLabel, GUILayout.ExpandWidth (false));

                        newsContent.News[i].subtitleColorColor = EditorGUILayout.ColorField ("", newsContent.News[i].subtitleColorColor, GUILayout.Width (40));
                        newsContent.News[i].subtitleColor = ColorUtility.ToHtmlStringRGB (newsContent.News[i].subtitleColorColor);


                        //newsContent.News [i].subtitleColor = ColorUtility.ToHtmlStringRGB (newsContent.News [i].subtitleColorColor);

                        GUI.enabled = false;

                        GUILayout.TextField (newsContent.News[i].subtitleColor);

                        GUI.enabled = true;

                        GUILayout.EndHorizontal ();

                    }


                    GUILayout.Space (10);

                    // Date
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Date</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showDate = GUILayout.Toggle (newsContent.News[i].showDate, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showDate == true)
                    {
                        //GUILayout.BeginHorizontal ();

                        //GUILayout.Label ("Date", normalLabel, GUILayout.ExpandWidth (false));
                        //newsContent.News [i].date = GUILayout.TextField (newsContent.News [i].date, textFieldStyle);

                        if (newsContent.News[i].validDate == false)
                        {
                            GUILayout.Label ($"<b><color=yellow>Invalid date</color></b>", normalLabel, GUILayout.ExpandWidth (false));
                        }

                        GUILayout.BeginHorizontal ();

                        DateTime dateFromISO = DateTime.UtcNow;
                        try
                        {
                            dateFromISO = DateTime.Parse (newsContent.News[i].date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        }
                        catch
                        {

                        }

                        newsContent.News[i].dateYear = dateFromISO.Year;
                        newsContent.News[i].dateMonth = dateFromISO.Month;
                        newsContent.News[i].dateDay = dateFromISO.Day;
                        newsContent.News[i].dateHour = dateFromISO.Hour;
                        newsContent.News[i].dateMinute = dateFromISO.Minute;


                        GUILayout.Label ("Year", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].dateYear = EditorGUILayout.IntField (Mathf.Clamp (newsContent.News[i].dateYear, 1000, 9999), textFieldStyleNoFixed);

                        GUILayout.Label ("Month", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].dateMonth = EditorGUILayout.IntField (Mathf.Clamp (newsContent.News[i].dateMonth, 1, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Day", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].dateDay = EditorGUILayout.IntField (Mathf.Clamp (newsContent.News[i].dateDay, 1, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Hour", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].dateHour = EditorGUILayout.IntField (Mathf.Clamp (newsContent.News[i].dateHour, 0, 99), textFieldStyleNoFixed);

                        GUILayout.Label ("Minutes", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].dateMinute = EditorGUILayout.IntField (Mathf.Clamp (newsContent.News[i].dateMinute, 0, 99), textFieldStyleNoFixed);
                        DateTime dt = new DateTime ();
                        try
                        {
                            dt = new DateTime (
                                Mathf.Clamp (newsContent.News[i].dateYear, 1000, 9999),
                                Mathf.Clamp (newsContent.News[i].dateMonth, 1, 12),
                                Mathf.Clamp (newsContent.News[i].dateDay, 1, 31),
                                Mathf.Clamp (newsContent.News[i].dateHour, 0, 23),
                                Mathf.Clamp (newsContent.News[i].dateMinute, 0, 59),
                                0);

                            newsContent.News[i].validDate = true;
                        }
                        catch
                        {
                            newsContent.News[i].validDate = false;
                        }

                        newsContent.News[i].date = dt.ToString ("O");

                        GUILayout.EndHorizontal ();
                    }

                    GUILayout.Space (10);

                    // Video URL
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Video</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showVideo = GUILayout.Toggle (newsContent.News[i].showVideo, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showVideo == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Video URL", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].videoURL = GUILayout.TextField (newsContent.News[i].videoURL, textFieldStyle);
                        GUILayout.EndHorizontal ();
                    }

                    GUILayout.Space (10);

                    // Image URL

                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Image URL", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.News[i].imagesURL[0] = GUILayout.TextField (newsContent.News[i].imagesURL[0], textFieldStyle);
                    GUILayout.EndHorizontal ();


                    GUILayout.Space (10);

                    // Interaction URL

                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Interaction URL", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.News[i].interactionURL = GUILayout.TextField (newsContent.News[i].interactionURL, textFieldStyle);
                    GUILayout.EndHorizontal ();


                    GUILayout.Space (10);

                    // Content

                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Content", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.News[i].content = GUILayout.TextArea (newsContent.News[i].content, GUILayout.Height (300));
                    GUILayout.EndHorizontal ();


                    GUILayout.Space (10);

                    // Button
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Button</b>", normalLabel, GUILayout.ExpandWidth (false));

                    newsContent.News[i].showButton = GUILayout.Toggle (newsContent.News[i].showButton, "", toggle);
                    GUILayout.EndHorizontal ();

                    if (newsContent.News[i].showButton == true)
                    {
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Button Content", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.News[i].buttonContent = GUILayout.TextField (newsContent.News[i].buttonContent, textFieldStyle);
                        GUILayout.EndHorizontal ();
                    }

                    GUILayout.Space (10);

                    // Move Up / Move Down / Delete

                    GUILayout.BeginHorizontal ();

                    if (i == 0)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button ("Move Up", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        int actualIndex = i;

                        var element = (News)Extensions.DeepClone (newsContent.News[i]);

                        newsContent.News.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsContent.News.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), element);

                        var currentValueNewsToShow = newsCollapsedList[i];
                        newsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsCollapsedList.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), currentValueNewsToShow);

                        break;
                    }

                    GUI.enabled = true;

                    if (i == newsContent.News.Count - 1)
                    {
                        GUI.enabled = false;
                    }

                    if (GUILayout.Button ("Move Down", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        int actualIndex = i;

                        var element = (News)Extensions.DeepClone (newsContent.News[i]);

                        newsContent.News.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsContent.News.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.News.Count), element);

                        var currentValueNewsToShow = newsCollapsedList[i];
                        newsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                        newsCollapsedList.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.News.Count), currentValueNewsToShow);

                        break;
                    }

                    GUI.enabled = true;

                    GUI.backgroundColor = Color.red;

                    if (GUILayout.Button ("Delete", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                    {
                        // Action
                        newsContent.News.RemoveAt (i);

                        newsCollapsedList.RemoveAt (i);
                    }

                    GUI.backgroundColor = Color.gray;

                    GUILayout.EndHorizontal ();

                    GUILayout.EndVertical ();

                    GUILayout.Space (10);

                }
            }

            GUILayout.BeginHorizontal ();

            // Add

            if (GUILayout.Button ("Add", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
            {
                // Action
                newsContent.News.Add (new News ());

                newsCollapsedList.Add (true);
            }

            GUILayout.EndHorizontal ();

            GUILayout.Space (10);

            GUILayout.EndVertical ();

            GUILayout.Space (10);
            #endregion News

            GUILayout.Space (10);

            #region SubNews

            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label ("SubNews", title2TextStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            // Elements

            for (int i = 0; i < newsContent.SubNews.Count; i++)
            {

                GUI.backgroundColor = Color.white;

                // 1 Begin Vertical
                GUILayout.BeginVertical (groupStyle2, GUILayout.ExpandWidth (false));
                GUI.backgroundColor = Color.gray;

                GUILayout.Space (10);

                GUILayout.BeginHorizontal ();

                GUILayout.Label ($"<b>#{i}</b>", normalLabel, GUILayout.ExpandWidth (false));

                GUILayout.Space (10);

                var subNewsSubstring = newsContent.SubNews[i].title;

                if (subNewsSubstring.Length > 50)
                {
                    subNewsSubstring = newsContent.SubNews[i].title.Substring (0, 50);
                }

                subNewsCollapsedList[i] = EditorGUILayout.Foldout (subNewsCollapsedList[i], $"Collapse | {subNewsSubstring}...");

                GUILayout.EndHorizontal ();

                GUILayout.Space (10);

                if (subNewsCollapsedList[i] == false)
                {

                    GUILayout.EndVertical ();

                    continue;
                }

                if (subNewsCollapsedList[i])
                {

                    // Interaction URL
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Interaction URL", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.SubNews[i].interactionURL = GUILayout.TextField (newsContent.SubNews[i].interactionURL, textFieldStyle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Image URL
                    GUILayout.BeginHorizontal ();

                    GUILayout.Label ("Image URL", normalLabel, GUILayout.ExpandWidth (false));
                    newsContent.SubNews[i].imageURL = GUILayout.TextField (newsContent.SubNews[i].imageURL, textFieldStyle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    // Show Content
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ($"<b>Show Content</b>", normalLabel, GUILayout.ExpandWidth (false));

                    GUILayout.Space (5);

                    newsContent.SubNews[i].showContent = GUILayout.Toggle (newsContent.SubNews[i].showContent, "", toggle);
                    GUILayout.EndHorizontal ();

                    GUILayout.Space (5);

                    if (newsContent.SubNews[i].showContent == true)
                    {

                        // Title
                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Title", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.SubNews[i].title = GUILayout.TextField (newsContent.SubNews[i].title, textFieldStyle);
                        GUILayout.EndHorizontal ();

                        GUILayout.Space (5);

                        // Date
                        GUILayout.BeginHorizontal ();


                        GUILayout.Label ("Show date?", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.SubNews[i].showDate = GUILayout.Toggle (newsContent.SubNews[i].showDate, "", toggle);
                        GUILayout.EndHorizontal ();

                        if (newsContent.SubNews[i].validDate == false)
                        {
                            GUILayout.Label ($"<b><color=yellow>Invalid date</color></b>", normalLabel, GUILayout.ExpandWidth (false));
                        }

                        if (newsContent.SubNews[i].showDate)
                        {
                            GUILayout.BeginHorizontal ();

                            DateTime dateFromISO = DateTime.UtcNow;
                            try
                            {
                                dateFromISO = DateTime.Parse (newsContent.SubNews[i].date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            }
                            catch
                            {

                            }

                            newsContent.SubNews[i].dateYear = dateFromISO.Year;
                            newsContent.SubNews[i].dateMonth = dateFromISO.Month;
                            newsContent.SubNews[i].dateDay = dateFromISO.Day;
                            newsContent.SubNews[i].dateHour = dateFromISO.Hour;
                            newsContent.SubNews[i].dateMinute = dateFromISO.Minute;


                            GUILayout.Label ("Year", normalLabel, GUILayout.ExpandWidth (false));
                            newsContent.SubNews[i].dateYear = EditorGUILayout.IntField (Mathf.Clamp (newsContent.SubNews[i].dateYear, 1000, 9999), textFieldStyleNoFixed);

                            GUILayout.Label ("Month", normalLabel, GUILayout.ExpandWidth (false));
                            newsContent.SubNews[i].dateMonth = EditorGUILayout.IntField (Mathf.Clamp (newsContent.SubNews[i].dateMonth, 1, 99), textFieldStyleNoFixed);

                            GUILayout.Label ("Day", normalLabel, GUILayout.ExpandWidth (false));
                            newsContent.SubNews[i].dateDay = EditorGUILayout.IntField (Mathf.Clamp (newsContent.SubNews[i].dateDay, 1, 99), textFieldStyleNoFixed);

                            GUILayout.Label ("Hour", normalLabel, GUILayout.ExpandWidth (false));
                            newsContent.SubNews[i].dateHour = EditorGUILayout.IntField (Mathf.Clamp (newsContent.SubNews[i].dateHour, 0, 99), textFieldStyleNoFixed);

                            GUILayout.Label ("Minutes", normalLabel, GUILayout.ExpandWidth (false));
                            newsContent.SubNews[i].dateMinute = EditorGUILayout.IntField (Mathf.Clamp (newsContent.SubNews[i].dateMinute, 0, 99), textFieldStyleNoFixed);
                            DateTime dt = new DateTime ();

                            newsContent.SubNews[i].validDate = true;
                            try
                            {
                                dt = new DateTime (
                                    Mathf.Clamp (newsContent.SubNews[i].dateYear, 1000, 9999),
                                    Mathf.Clamp (newsContent.SubNews[i].dateMonth, 1, 12),
                                    Mathf.Clamp (newsContent.SubNews[i].dateDay, 1, 31),
                                    Mathf.Clamp (newsContent.SubNews[i].dateHour, 0, 23),
                                    Mathf.Clamp (newsContent.SubNews[i].dateMinute, 0, 59),
                                    0);

                                newsContent.SubNews[i].validDate = true;
                            }
                            catch
                            {
                                newsContent.SubNews[i].validDate = false;
                            }

                            newsContent.SubNews[i].date = dt.ToString ("O");

                            GUILayout.EndHorizontal ();

                            GUILayout.Space (5);
                        }

                        // Date
                        //if (newsContent.SubNews [i].showDate) {

                        //    GUILayout.BeginHorizontal ();
                        //    GUILayout.Space (30);
                        //    newsContent.SubNews [i].date = GUILayout.TextField (newsContent.SubNews [i].date, textFieldStyle);

                        //    GUILayout.EndHorizontal ();

                        //}

                        GUILayout.Space (5);

                        // Content

                        GUILayout.BeginHorizontal ();

                        GUILayout.Label ("Content", normalLabel, GUILayout.ExpandWidth (false));
                        newsContent.SubNews[i].content = GUILayout.TextArea (newsContent.SubNews[i].content, GUILayout.Height (50));
                        GUILayout.EndHorizontal ();


                        GUILayout.Space (10);
                    }
                }

                // Move Up / Move Down / Delete

                GUILayout.BeginHorizontal ();

                if (i == 0)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button ("Move Up", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                {
                    // Action
                    int actualIndex = i;

                    var element = (News)Extensions.DeepClone (newsContent.News[i]);

                    newsContent.News.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                    newsContent.News.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), element);

                    var currentValueSubNewsToShow = subNewsCollapsedList[i];
                    subNewsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                    subNewsCollapsedList.Insert (Mathf.Clamp (actualIndex - 1, 0, int.MaxValue), currentValueSubNewsToShow);

                    break;
                }

                GUI.enabled = true;

                if (i == newsContent.News.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button ("Move Down", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                {
                    // Action
                    int actualIndex = i;

                    var element = (News)Extensions.DeepClone (newsContent.News[i]);

                    newsContent.News.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                    newsContent.News.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.News.Count), element);

                    var currentValueSubNewsToShow = subNewsCollapsedList[i];
                    subNewsCollapsedList.RemoveAt (Mathf.Clamp (actualIndex, 0, int.MaxValue));
                    subNewsCollapsedList.Insert (Mathf.Clamp (actualIndex + 1, 0, newsContent.SubNews.Count), currentValueSubNewsToShow);

                    break;
                }

                GUI.enabled = true;

                GUI.backgroundColor = Color.red;

                if (GUILayout.Button ("Delete", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
                {
                    // Action
                    newsContent.SubNews.RemoveAt (i);

                    subNewsCollapsedList.RemoveAt (i);
                }

                GUI.backgroundColor = Color.gray;

                GUILayout.EndHorizontal ();
                GUILayout.EndVertical ();

                GUILayout.Space (10);
            }

            GUILayout.Space (10);


            // Add
            GUILayout.BeginHorizontal ();

            if (GUILayout.Button ("Add", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
            {
                // Action
                newsContent.SubNews.Add (new SubNews ());

                subNewsCollapsedList.Add (true);
            }

            GUILayout.EndHorizontal ();

            GUILayout.Space (10);


            GUILayout.EndVertical ();

            #endregion

            GUILayout.EndVertical ();

            GUILayout.BeginVertical ();
            #region TO JSON
            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label ("Generated JSON", title2TextStyle, GUILayout.Width (500), GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            GUILayout.Label (new GUIContent ($"<b>File Name:</b> {$"{languages[popUpSelectedLanguage]}_{environments[popUpSelectedEnvironment]}_News.txt"}"), normalLabel);

            GUILayout.Space (10);

            GUILayout.BeginHorizontal ();

            // Add

            if (GUILayout.Button ("Copy", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
            {
                // Action
                GUIUtility.systemCopyBuffer = JsonUtility.ToJson (newsContent, true);
            }

            GUILayout.EndHorizontal ();

            generatedJson = GUILayout.TextArea (JsonUtility.ToJson (newsContent, true));

            GUILayout.EndVertical ();
            #endregion

            GUILayout.Space (10);
            #region Load JSON
            GUILayout.BeginVertical (groupStyle, GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true));

            GUILayout.Space (10);

            GUILayout.Label ("Load JSON", title2TextStyle, GUILayout.ExpandWidth (false));

            GUILayout.Space (10);

            //GUILayout.Label (new GUIContent ($"<b>File Name:</b> {$"{languages [popUpSelectedLanguage]}_{environments [popUpSelectedEnvironment]}_News.txt"}"), normalLabel);

            GUILayout.Space (10);

            GUILayout.BeginHorizontal ();

            // Add

            if (string.IsNullOrEmpty (jsonToLoad) == true)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button ("Load", button, GUILayout.Height (35), GUILayout.MaxWidth (200)))
            {
                // Action
                //GUIUtility.systemCopyBuffer = JsonUtility.ToJson (newsContent, true);

                invalidJson = false;
                loadedJson = false;

                try
                {
                    newsContent = JsonUtility.FromJson<NewsContent> (jsonToLoad.Trim ());

                    for (int i = 0; i < newsContent.News.Count; i++)
                    {
                        newsContent.News[i].subtitleColorColor = FromHex (newsContent.News[i].subtitleColor);
                    }

                    loadedJson = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning ($"Invalid JSON: {e.StackTrace}");
                    invalidJson = true;
                    Initiate ();
                }

                InitiateValues ();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal ();

            GUILayout.Space (10);

            GUILayout.Label (new GUIContent ($"Paste your JSON below"), normalLabel);

            GUILayout.Space (10);


            if (invalidJson == true)
            {
                GUILayout.Label (new GUIContent ($"<b><color=yellow>Invalid JSON</color></b>"), normalLabel);
                GUILayout.Space (10);
            }
            else if (loadedJson == true)
            {
                GUILayout.Label (new GUIContent ($"<b><color=green>Loaded correctly</color></b>"), normalLabel);
                GUILayout.Space (10);
            }

            jsonToLoad = GUILayout.TextArea (string.IsNullOrEmpty (jsonToLoad) ? "{\n   \"ProjectName\": \"Game Launcher\",\n   \"Language\": \"en_US\",\n   \"Environment\": \"Release\",\n    ...\n}" : jsonToLoad.Trim ());

            GUILayout.EndVertical ();
            #endregion

            GUILayout.EndVertical ();

            GUILayout.EndHorizontal ();

            GUILayout.EndScrollView ();
        }

        string generatedJson = "";
        string jsonToLoad = "";
        bool invalidJson = false;
        bool loadedJson = false;

        private void OnInspectorUpdate ()
        {

            Repaint ();


            if (EditorApplication.isCompiling)
            {
                Close ();
            }
        }

        private void OnUpdate ()
        {
            if (project == null)
            {
                EditorApplication.update -= OnUpdate;
                return;
            }

            string log = project.FetchLog ();
            while (log != null)
            {
                Debug.Log (log);
                log = project.FetchLog ();
            }

            if (!project.IsRunning)
            {
                if (project.Result == PatchResult.Failed)
                {
                    Debug.Log ("<b><color=red>Operation failed</color></b>");
                }
                else
                {
                    Debug.Log ("<b><color=green>Operation successful</color></b>");
                }

                Initiate ();
                project = null;
                EditorApplication.update -= OnUpdate;
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

        private void DrawHorizontalLine (int size = 0)
        {
            GUILayout.Space (5);

            if (size == 0)
            {
                GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height (1));
            }
            else
            {
                GUILayout.Box ("", GUILayout.Width (size), GUILayout.Height (1));
            }

            GUILayout.Space (5);
        }

        private static void DirectoryMove (string sourceDirName, string destDirName, bool preserveSubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo (sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException (
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories ();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists (destDirName))
            {
                Directory.CreateDirectory (destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles ();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine (destDirName, file.Name);
                file.MoveTo (temppath);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (preserveSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine (destDirName, subdir.Name);
                    DirectoryMove (subdir.FullName, temppath, preserveSubDirs);
                    if (Directory.GetFiles (subdir.FullName).Length == 0)
                    {
                        FileSafety.DirectoryDelete (subdir.FullName);
                    }
                }
            }
        }
        public static Color FromHex (string hex)
        {
            hex = hex.TrimStart ('#');

            Color col;

            try
            {
                if (hex.Length == 6)
                    col = new Color32 (
                                byte.Parse (hex.Substring (0, 2), NumberStyles.HexNumber),
                                byte.Parse (hex.Substring (2, 2), NumberStyles.HexNumber),
                                byte.Parse (hex.Substring (4, 2), NumberStyles.HexNumber),
                                255);
                else // assuming length of 8
                    col = new Color32 (
                                byte.Parse (hex.Substring (0, 2), NumberStyles.HexNumber),
                                byte.Parse (hex.Substring (2, 2), NumberStyles.HexNumber),
                                byte.Parse (hex.Substring (4, 2), NumberStyles.HexNumber),
                                byte.Parse (hex.Substring (6, 2), NumberStyles.HexNumber));

            }
            catch
            {
                col = Color.black;
            }
            return col;
        }
    }

}

namespace GameLauncher
{
    [Serializable]
    public class NewsContent
    {
        public string ProjectName = "Game Launcher";
        public string Language = "en_US";
        public string Environment = "Release";
        public string Region = "Any";
        public string ServerStatus = "Online";
        public string MyAccountURL = "";
        public string WebpageURL = "";
        public string PatchNotesURL = "";
        public string TermsOfServiceURL = "";
        public string PrivacyPolicyURL = "";
        public string NewsCurrentURL = "";
        public string ReportBugURL = "";

        public List<Alerts> Alerts;
        public List<News> News;
        public List<SubNews> SubNews;
    }

    [Serializable]
    public class News
    {
        public string header = "";
        public string title = "";
        public string subtitle = "";

        [NonSerialized]
        public Color subtitleColorColor = Color.white;
        public string subtitleColor = "";

        public string date = DateTime.UtcNow.ToUniversalTime ().ToString ("O");

        [NonSerialized]
        public bool validDate = true;

        [NonSerialized]
        public int dateYear = DateTime.UtcNow.Year;
        [NonSerialized]
        public int dateMonth = DateTime.UtcNow.Month;
        [NonSerialized]
        public int dateDay = DateTime.UtcNow.Day;

        [NonSerialized]
        public int dateHour = DateTime.UtcNow.Hour;

        [NonSerialized]
        public int dateMinute = DateTime.UtcNow.Minute;
        public string[] imagesURL = new string[] { "" };
        public string videoURL = "";
        public string interactionURL = "";
        public string content = "";
        public string buttonContent = "";

        public bool showHeader = true;
        public bool showTitle = true;
        public bool showSubTitle = true;
        public bool subtitleCustomColor = false;
        public bool showContent = true;
        public bool showDate = true;
        public bool showButton = true;
        public bool showVideo = false;
    }

    [Serializable]
    public class SubNews
    {
        public string title = "";
        public string date = DateTime.UtcNow.ToUniversalTime ().ToString ("O");
        public string imageURL = "";
        public string interactionURL = "";
        public string content = "";
        public bool showTitle = true;
        public bool showContent = true;
        public bool showDate = false;

        [NonSerialized]
        public int dateYear = DateTime.UtcNow.Year;
        [NonSerialized]
        public int dateMonth = DateTime.UtcNow.Month;
        [NonSerialized]
        public int dateDay = DateTime.UtcNow.Day;

        [NonSerialized]
        public int dateHour = DateTime.UtcNow.Hour;

        [NonSerialized]
        public int dateMinute = DateTime.UtcNow.Minute;

        [NonSerialized]
        public bool validDate = true;
    }

    [Serializable]
    public class Alerts
    {
        public string type = "Information";
        public string date = DateTime.UtcNow.ToUniversalTime ().ToString ("O");
        public string showAfterDate = DateTime.UtcNow.ToUniversalTime ().ToString ("O");
        public bool showAfterDateBool = false;
        public string title = "";
        public string message = "";

        public string interactionURL = "";
        [NonSerialized] public bool validDate = true;
        [NonSerialized]
        public int dateYear = DateTime.UtcNow.Year;
        [NonSerialized]
        public int dateMonth = DateTime.UtcNow.Month;
        [NonSerialized]
        public int dateDay = DateTime.UtcNow.Day;

        [NonSerialized]
        public int dateHour = DateTime.UtcNow.Hour;

        [NonSerialized]
        public int dateMinute = DateTime.UtcNow.Minute;

        [NonSerialized] public bool validShowAfterDate = true;
        [NonSerialized]
        public int showAfterDateYear = DateTime.UtcNow.Year;
        [NonSerialized]
        public int showAfterDateMonth = DateTime.UtcNow.Month;
        [NonSerialized]
        public int showAfterDateDay = DateTime.UtcNow.Day;

        [NonSerialized]
        public int showAfterDateHour = DateTime.UtcNow.Hour;

        [NonSerialized]
        public int showAfterDateMinute = DateTime.UtcNow.Minute;
    }

    static class Extensions
    {
        public static object DeepClone (object obj)
        {
            object objResult = null;

            using (var ms = new MemoryStream ())
            {
                var bf = new BinaryFormatter ();
                bf.Serialize (ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize (ms);
            }

            return objResult;
        }
    }
}