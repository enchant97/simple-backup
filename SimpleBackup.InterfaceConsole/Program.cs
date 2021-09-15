using System;
using SimpleBackup.Core;
using System.IO;

namespace SimpleBackup.InterfaceConsole
{
    class Program
    {
        private static SimpleBackup.Core.Configuration.Types.AppConfig appConfig;
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
            Console.WriteLine("MENU\n");
            Console.WriteLine("-\t(H)elp -> show help");
            Console.WriteLine("-\t(Q)uit -> exit the app");
            Console.WriteLine();
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
                Console.Clear();
                ShowHeader();
                ShowMenu();
                switch (GetInput().ToLower())
                {
                    case "h":
                        ShowHelp();
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
