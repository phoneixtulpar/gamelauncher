using System;
using System.Collections.Generic;
using System.Text;

namespace GameLauncher.NewsInformation
{
    public class NewsContent
    {
        public string ProjectName { get; set; }
        public string Language { get; set; }
        public string Environment { get; set; }
        public string Region { get; set; }
        public string ServerStatus { get; set; }
        public string MyAccountURL { get; set; }
        public string WebpageURL { get; set; }
        public string PatchNotesURL { get; set; }
        public string TermsAndConditionsURL { get; set; }
        public string PrivacyPolicyURL { get; set; }
        public string ReportBugURL { get; set; }
        public List<Alerts> Alerts { get; set; }
        public List<News> News { get; set; }
        public List<SubNews> SubNews { get; set; }
    }

    public class News
    {
        public string header { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string subtitleColor { get; set; }

        public DateTime date { get; set; }
        public string[] imagesURL { get; set; }
        public string videoURL { get; set; }
        public string interactionURL { get; set; }
        public string content { get; set; }
        public string buttonContent { get; set; }

        public bool showHeader { get; set; }
        public bool showTitle { get; set; }
        public bool showSubTitle { get; set; }
        public bool subtitleCustomColor { get; set; }
        public bool showContent { get; set; }
        public bool showDate { get; set; }
        public bool showButton { get; set; }
        public bool? showVideo { get; set; }
    }

    public class Alerts
    {
        public string type { get; set; }
        public string date { get; set; }
        public DateTime showAfterDate { get; set; }
        public bool showAfterDateBool { get; set; }
        public string message { get; set; }

        public string interactionURL { get; set; }
    }

    public class SubNews
    {
        public string title { get; set; }
        public DateTime date { get; set; }
        public string imageURL { get; set; }
        public string interactionURL { get; set; }
        public string content { get; set; }
        public bool showTitle { get; set; }
        public bool showContent { get; set; }
        public bool showDate { get; set; }
    }
}
