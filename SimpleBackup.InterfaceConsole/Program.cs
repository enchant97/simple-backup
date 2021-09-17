using System;
using SimpleBackup.Core;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using SimpleBackup.Core.Configuration.Types;

namespace SimpleBackup.InterfaceConsole
{
    enum IncludedOrExcluded { INCLUDED, EXCLUDED }
    class Program
    {
        private static AppConfig appConfig;
        static void Main(string[] args)
        {
            ReadConfig();
            InteractiveMode();
        }
        #region Config Helpers
        static void ReadConfig()
        {
            Directory.CreateDirectory(Constants.UserHomePath);
            if (!File.Exists(Constants.ConfigFullPath))
                SimpleBackup.Core.Configuration.Helpers.WriteDefaults(Constants.ConfigFullPath);
            appConfig = SimpleBackup.Core.Configuration.Helpers.Read(Constants.ConfigFullPath);
        }
        static void WriteConfig()
        {
            SimpleBackup.Core.Configuration.Helpers.Write(Constants.ConfigFullPath, appConfig);
        }
        static void WritePathIncludedOrExcluded(string[] newPaths, int currentIndex, IncludedOrExcluded includedOrExcluded)
        {
            switch (includedOrExcluded)
            {
                case IncludedOrExcluded.INCLUDED:
                    appConfig.BackupConfigs[currentIndex].IncludedPaths = newPaths;
                    break;
                case IncludedOrExcluded.EXCLUDED:
                    appConfig.BackupConfigs[currentIndex].ExcludedPaths = newPaths;
                    break;
            }
            WriteConfig();
        }
        static void ResetConfig()
        {
            SimpleBackup.Core.Configuration.Helpers.WriteDefaults(Constants.ConfigFullPath);
            ReadConfig();
        }
        #endregion
        static void ShowHelp()
        {
            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("HELP\n");
            Console.WriteLine("Welcome to Simple Backup, here is some help to get you started:\n");
            Console.WriteLine("-\tYou can do full-backups");
            Console.WriteLine("-\tYou can include and exclude paths to backup");
            Console.WriteLine("-\tYou can set versions of backups to keep");
            Console.WriteLine("-\tYou can keep different configurations for backups");
            Console.WriteLine("-\tOnce everything is configured you can backup with one command");
            Console.WriteLine("-\tAll configs are stored in a xml file");
            Utils.ShowResume();
        }
        static void ShowCredits()
        {
            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("License GPL-3.0\n");
            Console.WriteLine(Constants.CopyrightText);
            Utils.ShowResume();
        }
        static void ShowMenu()
        {
            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("MENU\n");
            Console.WriteLine("\t(B)ackup -> Start a backup");
            Console.WriteLine("\t(C)onfigure -> View and edit app configuration");
            Console.WriteLine("\t(Cr)edits -> Show the credits");
            Console.WriteLine("\t(H)elp -> Show help");
            Console.WriteLine("\t(Q)uit -> Exit the app");
            Console.WriteLine();
        }
        static void InteractiveConfigPathsMenu(int configIndex, IncludedOrExcluded includedOrExcluded)
        {
            string headerMsg = String.Empty;
            List<string> currentPaths = new();
            switch (includedOrExcluded)
            {
                case IncludedOrExcluded.INCLUDED:
                    headerMsg = "INCLUDED";
                    currentPaths = appConfig.BackupConfigs[configIndex].IncludedPaths.ToList();
                    break;
                case IncludedOrExcluded.EXCLUDED:
                    headerMsg = "EXCLUDED";
                    currentPaths = appConfig.BackupConfigs[configIndex].ExcludedPaths.ToList();
                    break;
            }
            string title = String.Format(
                "CONFIG - {0} - {1}\n",
                appConfig.BackupConfigs[configIndex].Name,
                headerMsg
            );

            while (true)
            {
                Console.Clear();
                Utils.ShowHeader();
                Console.WriteLine(title);
                int pathsCount = currentPaths.Count;
                for (int i = 0; i < pathsCount; i++)
                {
                    Console.WriteLine("\t({0}) -> {1}", i + 1, currentPaths[i]);
                }
                Console.WriteLine("\t(A)ppend -> Add a new entry");
                Console.WriteLine("\t(C)lear -> Remove all entries");
                Console.WriteLine("\t(Q)uit -> Go back");

                string input = Utils.GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "q")
                {
                    break;
                }
                else if (input == "a")
                {
                    // TODO Validate it is a real path
                    string newPath = Utils.ShowStringInput("Enter Path To Append");
                    if (!String.IsNullOrWhiteSpace(newPath))
                    {
                        currentPaths.Add(newPath);
                        WritePathIncludedOrExcluded(
                            currentPaths.ToArray(),
                            configIndex,
                            includedOrExcluded
                        );
                    }
                }
                else if (input == "c")
                {
                    currentPaths.Clear();
                    WritePathIncludedOrExcluded(
                        currentPaths.ToArray(),
                        configIndex,
                        includedOrExcluded
                    );
                }
                else if (isInt && (option > 0 && option <= pathsCount))
                {
                    bool okToRemove = Utils.ShowYesNoInput("Are You Sure You Want To Remove That Path?");
                    option--;
                    if (okToRemove)
                    {
                        currentPaths.RemoveAt(option);
                        WritePathIncludedOrExcluded(
                            currentPaths.ToArray(),
                            configIndex,
                            includedOrExcluded
                        );
                    }
                }
                else { Utils.ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveConfigMenu(int configIndex)
        {
            while (true)
            {
                BackupConfig selectedConfig = appConfig.BackupConfigs[configIndex];
                Console.Clear();
                Utils.ShowHeader();
                Console.WriteLine("CONFIG - {0}\n", selectedConfig.Name);
                Console.WriteLine("\t(1) -> Name = {0}", selectedConfig.Name);
                Console.WriteLine("\t(2) -> Destination Path = {0}", selectedConfig.DestinationPath);
                Console.WriteLine("\t(3) -> Included Paths = {0} Paths", selectedConfig.IncludedPaths.Length);
                Console.WriteLine("\t(4) -> Excluded Paths = {0} Paths", selectedConfig.ExcludedPaths.Length);
                Console.WriteLine("\t(5) -> Versions To Keep = {0}", selectedConfig.VersionsToKeep);
                Console.WriteLine("\t(R)ename -> Rename the config name");
                Console.WriteLine("\t(D)elete -> Delete the config");
                Console.WriteLine("\t(Q)uit -> Go back");

                string input = Utils.GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "q")
                {
                    break;
                }
                else if (input == "r")
                {
                    string newConfigName = Utils.ShowStringInput("Enter Updated Config Name");
                    if (!String.IsNullOrWhiteSpace(newConfigName))
                    {
                        selectedConfig.Name = newConfigName;
                        appConfig.BackupConfigs[configIndex] = selectedConfig;
                        WriteConfig();
                    }
                }
                else if (input == "d")
                {
                    bool deleteConfirm = Utils.ShowYesNoInput("Are You Sure You Want To Delete This Config?");
                    if (deleteConfirm)
                    {
                        if (appConfig.BackupConfigs.Length <= 1)
                        {
                            appConfig.BackupConfigs = new[] { new BackupConfig() };
                        }
                        else
                        {
                            List<BackupConfig> configs = appConfig.BackupConfigs.ToList();
                            configs.RemoveAt(configIndex);
                            appConfig.BackupConfigs = configs.ToArray();
                        }
                        WriteConfig();
                        return;
                    }
                }
                else if (isInt && (option > 0 && option <= 5))
                {
                    if (option == 1)
                    {
                        string newName = Utils.ShowStringInput("Enter Updated Config Name");
                        if (!String.IsNullOrWhiteSpace(newName))
                        {
                            selectedConfig.Name = newName;
                            appConfig.BackupConfigs[configIndex] = selectedConfig;
                            WriteConfig();
                        }
                    }
                    else if (option == 2)
                    {
                        // TODO Validate it is a real path
                        string newDestination = Utils.ShowStringInput("Enter Updated Destination Path");
                        if (!String.IsNullOrWhiteSpace(newDestination))
                        {
                            selectedConfig.DestinationPath = newDestination;
                            appConfig.BackupConfigs[configIndex] = selectedConfig;
                            WriteConfig();
                        }
                    }
                    else if (option == 3)
                    {
                        InteractiveConfigPathsMenu(configIndex, IncludedOrExcluded.INCLUDED);
                        selectedConfig = appConfig.BackupConfigs[configIndex];
                    }
                    else if (option == 4)
                    {
                        InteractiveConfigPathsMenu(configIndex, IncludedOrExcluded.EXCLUDED);
                        selectedConfig = appConfig.BackupConfigs[configIndex];
                    }
                    else if (option == 5)
                    {
                        // TODO more validation needed (what happens if user enters -2?)
                        selectedConfig.VersionsToKeep = Utils.ShowIntInput("Enter Updated Versions To Keep");
                        appConfig.BackupConfigs[configIndex] = selectedConfig;
                        WriteConfig();
                    }
                }
                else { Utils.ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveDefaultBackupConfigMenu()
        {
            int configsCount = appConfig.BackupConfigs.Length;
            while (true)
            {
                Console.Clear();
                Utils.ShowHeader();
                Console.WriteLine("CONFIG CHANGE DEFAULT\n");
                Console.WriteLine(
                    "Currently: ({0}), {1}",
                    appConfig.DefaultConfigI + 1,
                    appConfig.BackupConfigs[appConfig.DefaultConfigI].Name
                );
                for (int i = 0; i < configsCount; i++)
                {
                    var config = appConfig.BackupConfigs[i];
                    Console.WriteLine("\t({0}) -> {1}", i + 1, config.Name);
                }
                Console.WriteLine("\t(Q)uit -> go back");
                string input = Utils.GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "q")
                {
                    break;
                }
                else if (isInt && (option > 0 && option <= configsCount))
                {
                    option--;
                    appConfig.DefaultConfigI = option;
                    WriteConfig();
                }
                else { Utils.ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveConfigSelectMenu()
        {
            while (true)
            {
                Console.Clear();
                Utils.ShowHeader();
                Console.WriteLine("CONFIG\n");

                int configsCount = appConfig.BackupConfigs.Length;
                for (int i = 0; i < configsCount; i++)
                {
                    var config = appConfig.BackupConfigs[i];
                    Console.WriteLine("\t({0}) -> {1}", i + 1, config.Name);
                }
                Console.WriteLine("\t(A)dd -> Add Config");
                Console.WriteLine("\t(W)elcome -> Show welcome");
                Console.WriteLine("\t(D)efault -> Default backup config");
                Console.WriteLine("\t(R)eset -> Reset to defaults");
                Console.WriteLine("\t(Q)uit -> Go back");

                string input = Utils.GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "a")
                {
                    string newConfigName = Utils.ShowStringInput("Enter New Config Name");
                    if (!String.IsNullOrWhiteSpace(newConfigName))
                    {
                        Array.Resize(ref appConfig.BackupConfigs, appConfig.BackupConfigs.Length + 1);
                        appConfig.BackupConfigs[appConfig.BackupConfigs.Length - 1] = new BackupConfig() { Name = newConfigName };
                        WriteConfig();
                    }
                }
                else if (input == "w")
                {
                    appConfig.ShowHelp = Utils.ShowYesNoInput("Do You Want To Show Welcome?");
                    WriteConfig();
                }
                else if (input == "d")
                {
                    InteractiveDefaultBackupConfigMenu();
                }
                else if (input == "r")
                {
                    bool resetConfirm = Utils.ShowYesNoInput("Do You Want To Reset ALL Configs?");
                    if (resetConfirm) { ResetConfig(); }
                }
                else if (input == "q")
                {
                    break;
                }
                else if (isInt && (option > 0 && option <= configsCount))
                {

                    option--;
                    InteractiveConfigMenu(option);

                }
                else
                {
                    Utils.ShowError("Not A Valid Option");
                }
            }

        }
        static int ShowGetBackupConfigSelect()
        {
            while (true)
            {
                Console.Clear();
                Utils.ShowHeader();
                Console.WriteLine("CONFIG SELECT\n");

                int configsCount = appConfig.BackupConfigs.Length;
                for (int i = 0; i < configsCount; i++)
                {
                    Console.WriteLine("\t({0}) -> {1}", i + 1, appConfig.BackupConfigs[i].Name);
                }
                Console.WriteLine("\t(D)efault -> Use Default, {0}", appConfig.DefaultConfigI + 1);
                Console.WriteLine("\t(Q)uit -> Go back");

                string input = Utils.GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);

                if (input == "q")
                {
                    return -1;
                }
                else if (input == "d")
                {
                    return appConfig.DefaultConfigI;
                }
                else if (isInt && (option > 0 && option <= configsCount))
                {
                    option--;
                    return option;
                }
                else
                {
                    Utils.ShowError("Not A Valid Option");
                }

            }
        }
        static void ShowDiscoveringFiles(int currCount)
        {
            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("DISCOVERING FILES\n");
            Console.WriteLine("Found: {0}", currCount);
        }
        static void ShowCopyingFiles(int currCount, int maxCount)
        {
            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("COPYING FILES\n");
            Console.WriteLine("Copied: {0}/{1}", currCount, maxCount);
        }
        static void InteractiveBackup()
        {
            int selectedBackupConfig = ShowGetBackupConfigSelect();
            if (selectedBackupConfig == -1)
            {
                // user wanted to go back
                return;
            }

            Queue<string> pathsToBackup = new();
            string[] includedPaths = appConfig.BackupConfigs[selectedBackupConfig].IncludedPaths;
            string[] excludedPaths = appConfig.BackupConfigs[selectedBackupConfig].ExcludedPaths;
            string backupDstPath = Path.Join(
                appConfig.BackupConfigs[selectedBackupConfig].DestinationPath,
                Paths.GenerateBackupName()
            );
            int foundCount = 0;
            int copiedCount = 0;

            foreach (var searchPath in includedPaths)
            {
                foreach (var foundFilePath in Discovery.SearchFilesEnumerated(searchPath, excludedPaths))
                {
                    pathsToBackup.Enqueue(foundFilePath);
                    foundCount++;
                    ShowDiscoveringFiles(foundCount);
                }
            }

            foreach (var foundFilePath in pathsToBackup)
            {
                string currDstPath = Paths.CombineFullPath(foundFilePath, backupDstPath);
                Directory.CreateDirectory(Path.GetDirectoryName(currDstPath));
                try
                {
                    File.Copy(foundFilePath, currDstPath);
                    copiedCount++;
                    ShowCopyingFiles(copiedCount, foundCount);
                }
                catch (Exception ex)
                {
                    if (ex is System.IO.FileNotFoundException ||
                        ex is System.IO.IOException) { }
                    else { throw; }
                    // TODO show a log of all files missing or is a file type that can't be copied
                }
            }

            Console.Clear();
            Utils.ShowHeader();
            Console.WriteLine("FINISHED\n");
            Console.WriteLine("All Found Files Were Copied");
            Utils.ShowResume();
        }
        static void InteractiveMode()
        {
            Console.Title = Utils.GetTitle();
            if (appConfig.ShowHelp)
            {
                ShowHelp();
                appConfig.ShowHelp = false;
                WriteConfig();
            }

            bool run = true;
            while (run)
            {
                ShowMenu();
                switch (Utils.GetInput().ToLower())
                {
                    case "h":
                        ShowHelp();
                        break;
                    case "b":
                        InteractiveBackup();
                        break;
                    case "c":
                        InteractiveConfigSelectMenu();
                        break;
                    case "cr":
                        ShowCredits();
                        break;
                    case "q":
                        run = false;
                        Console.Clear();
                        break;
                    case "":
                    case " ":
                        break;
                    default:
                        Utils.ShowError("Unknown Input");
                        break;
                }
            }
        }
    }
}
