﻿using System;
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
        #region CLI Utils
        static void ShowHeader()
        {
            Console.WriteLine("Simple Backup - CLI Mode");
            Console.WriteLine(new String('-', Console.WindowWidth));
        }
        static string GetInput()
        {
            Console.Write(">> ");
            return Console.ReadLine();
        }
        static bool ShowYesNoInput(string msg)
        {
            while (true)
            {
                Console.Clear();
                ShowHeader();
                Console.WriteLine("CONFORMATION\n");
                Console.WriteLine(msg);
                Console.WriteLine("\t- (Y)es -> Accept");
                Console.WriteLine("\t- (N)o -> Deny");

                string choice = GetInput().ToLower();

                if (choice == "y") { return true; }
                if (choice == "n") { return false; }
                else { ShowError("Enter A Valid Option"); }
            }
        }
        static string ShowStringInput(string msg)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("ENTER STRING\n");
            Console.WriteLine(msg);
            return GetInput();
        }
        static int ShowIntInput(string msg)
        {
            while (true)
            {
                Console.Clear();
                ShowHeader();
                Console.WriteLine("ENTER INTEGER\n");
                Console.WriteLine(msg);
                bool isInt = int.TryParse(GetInput(), out int enteredInt);
                if (isInt) { return enteredInt; }
                ShowError("Not A Valid Integer");
            }
        }
        static void ShowResume()
        {
            Console.Write("\nPress RET To Resume");
            Console.ReadKey();
        }
        static void ShowError(string msg)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("ERROR\n");
            Console.WriteLine(msg);
            ShowResume();
        }
        #endregion
        static void ShowHelp()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("HELP\n");
            Console.WriteLine("Welcome to Simple Backup, here is some help to get you started:\n");
            Console.WriteLine("-\tYou can do full-backups");
            Console.WriteLine("-\tYou can include and exclude paths to backup");
            Console.WriteLine("-\tYou can set versions of backups to keep");
            Console.WriteLine("-\tYou can keep different configurations for backups");
            Console.WriteLine("-\tOnce everything is configured you can backup with one command");
            Console.WriteLine("-\tAll configs are stored in a xml file");
            ShowResume();
        }
        static void ShowMenu()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("MENU\n");
            Console.WriteLine("-\t(C)onfigure -> view and edit app configuration");
            Console.WriteLine("-\t(H)elp -> show help");
            Console.WriteLine("-\t(Q)uit -> exit the app");
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
                ShowHeader();
                Console.WriteLine(title);
                int pathsCount = currentPaths.Count;
                for (int i = 0; i < pathsCount; i++)
                {
                    Console.WriteLine("\t({0}) -> {1}", i + 1, currentPaths[i]);
                }
                Console.WriteLine("\t(A)ppend -> add a new entry");
                Console.WriteLine("\t(C)lear -> remove all entries");
                Console.WriteLine("\t(Q)uit -> go back");

                string input = GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "q")
                {
                    break;
                }
                else if (input == "a")
                {
                    // TODO Validate it is a real path
                    string newPath = ShowStringInput("Enter Path To Append");
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
                    bool okToRemove = ShowYesNoInput("Are You Sure You Want To Remove That Path?");
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
                else { ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveConfigMenu(int configIndex)
        {
            while (true)
            {
                BackupConfig selectedConfig = appConfig.BackupConfigs[configIndex];
                Console.Clear();
                ShowHeader();
                Console.WriteLine("CONFIG - {0}\n", selectedConfig.Name);
                Console.WriteLine("\t(1) -> Name = {0}", selectedConfig.Name);
                Console.WriteLine("\t(2) -> Destination Path = {0}", selectedConfig.DestinationPath);
                Console.WriteLine("\t(3) -> Included Paths = {0} Paths", selectedConfig.IncludedPaths.Length);
                Console.WriteLine("\t(4) -> Excluded Paths = {0} Paths", selectedConfig.ExcludedPaths.Length);
                Console.WriteLine("\t(5) -> Versions To Keep = {0}", selectedConfig.VersionsToKeep);
                Console.WriteLine("\t(Q)uit -> go back");

                string input = GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "q")
                {
                    break;
                }
                else if (isInt && (option > 0 && option <= 5))
                {
                    if (option == 1)
                    {
                        string newName = ShowStringInput("Enter Updated Config Name");
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
                        string newDestination = ShowStringInput("Enter Updated Destination Path");
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
                        selectedConfig.VersionsToKeep = ShowIntInput("Enter Updated Versions To Keep");
                        appConfig.BackupConfigs[configIndex] = selectedConfig;
                        WriteConfig();
                    }
                }
                else { ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveDefaultBackupConfigMenu()
        {
            int configsCount = appConfig.BackupConfigs.Length;
            while (true)
            {
                Console.Clear();
                ShowHeader();
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
                string input = GetInput().ToLower();
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
                else { ShowError("Not A Valid Option"); }
            }
        }
        static void InteractiveConfigSelectMenu()
        {
            while (true)
            {
                Console.Clear();
                ShowHeader();
                Console.WriteLine("CONFIG\n");

                int configsCount = appConfig.BackupConfigs.Length;
                for (int i = 0; i < configsCount; i++)
                {
                    var config = appConfig.BackupConfigs[i];
                    Console.WriteLine("\t({0}) -> {1}", i + 1, config.Name);
                }
                Console.WriteLine("\t(W)elcome -> show welcome");
                Console.WriteLine("\t(D)efault -> default backup config");
                Console.WriteLine("\t(R)eset -> reset to defaults");
                Console.WriteLine("\t(Q)uit -> go back");

                string input = GetInput().ToLower();
                bool isInt = int.TryParse(input, out int option);
                if (input == "w")
                {
                    appConfig.ShowHelp = ShowYesNoInput("Do You Want To Show Welcome?");
                    WriteConfig();
                }
                else if (input == "d")
                {
                    InteractiveDefaultBackupConfigMenu();
                }
                else if (input == "r")
                {
                    bool resetConfirm = ShowYesNoInput("Do You Want To Reset ALL Configs?");
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
                    ShowError("Not A Valid Option");
                }
            }

        }
        static void InteractiveMode()
        {
            Console.Title = "Simple Backup - CLI Mode";
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
                switch (GetInput().ToLower())
                {
                    case "h":
                        ShowHelp();
                        break;
                    case "c":
                        InteractiveConfigSelectMenu();
                        break;
                    case "q":
                        run = false;
                        Console.Clear();
                        break;
                    case "":
                    case " ":
                        break;
                    default:
                        ShowError("Unknown Input");
                        break;
                }
            }
        }
    }
}
