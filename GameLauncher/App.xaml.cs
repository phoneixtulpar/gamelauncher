using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GameLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (!IsValidLocation())
            {
                Application.Current.Shutdown();
            }

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        // This function checks if the launcher is located in a disallowed location
        public bool IsValidLocation()
        {
            // Get the directory where the current executing assembly (your application) is located
            string applicationPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            //LogError($"Current Location: {applicationPath}");

            // Define an array of paths where the application shouldn't be allowed to run from
            List<string> disallowedLocations = Enum.GetValues(typeof(Environment.SpecialFolder))
            .Cast<Environment.SpecialFolder>()
            .Select(Environment.GetFolderPath)
            .ToList();

            // Add the Downloads folder explicitly to the disallowed locations
            disallowedLocations.Add(KnownFolders.Downloads.Path);

            // Loop through all the drives in the system
            foreach (var drive in DriveInfo.GetDrives())
            {
                // If the drive is a hard disk or a removable disk (like a USB stick)
                if (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)
                {
                    // Get the root directory of the drive
                    string rootPath = drive.RootDirectory.FullName;

                    // Add this root directory to the list of disallowed locations
                    disallowedLocations.Add(rootPath);
                }
            }

            // Loop through each disallowed location
            foreach (var disallowed in disallowedLocations)
            {
                //LogError($"Disallowed Location: {disallowed}");

                // If the application's directory starts with the disallowed directory
                if (applicationPath.StartsWith(disallowed, StringComparison.OrdinalIgnoreCase))
                {
                    // Find the part of the application's path that follows the disallowed directory
                    string relativePath = applicationPath.Substring(disallowed.Length).Trim(Path.DirectorySeparatorChar);

                    // If the relative path doesn't contain any directory separators
                    // then the application is directly inside the disallowed directory
                    if (relativePath.IndexOf(Path.DirectorySeparatorChar) == -1)
                    {
                        // Construct an error message
                        var error = $"The application is running from a disallowed location: {disallowed}. The application will now exit.";

                        // Show a MessageBox with the error
                        MessageBox.Show(error, "Application Location Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        // Log the error (you need to implement this method)
                        LogError(error);

                        // Return false indicating the location is not valid
                        return false;
                    }
                }
            }

            // If we got this far then the application isn't in any disallowed locations
            // so return true indicating the location is valid
            return true;
        }




        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("UNHANDLED EXCEPTION: {0}\n{1}", e.Exception.Message, e.Exception.StackTrace);

            e.Handled = true;

            // Log the error to a file
            LogError(errorMessage);

            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Cerrar la aplicación
            Environment.Exit(0);
        }

        private void LogError(string errorMessage)
        {
            string logPath = "gamelauncher_error_log.txt";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine(errorMessage);
                writer.WriteLine(Environment.NewLine + "-------------------------------------------------------------" + Environment.NewLine);
            }
        }
    }
}
