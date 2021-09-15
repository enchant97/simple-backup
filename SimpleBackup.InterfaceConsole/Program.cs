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
        static void ReadConfig()
        {
            Directory.CreateDirectory(Constants.UserHomePath);
            if (!File.Exists(Constants.ConfigFullPath))
                SimpleBackup.Core.Configuration.Helpers.WriteDefaults(Constants.ConfigFullPath);
            appConfig = SimpleBackup.Core.Configuration.Helpers.Read(Constants.ConfigFullPath);
        }
        static void ShowHeader()
        {
            Console.WriteLine("Simple Backup - CLI Mode");
            Console.WriteLine(new String('-', Console.WindowWidth));
        }
        static void ShowMenu()
        {
            Console.WriteLine("MENU\n");
            Console.WriteLine("-\t(Q)uit -> exit the app");
            Console.WriteLine();
        }
        static string GetInput()
        {
            Console.Write(">> ");
            return Console.ReadLine();
        }
        static void ShowError(string msg)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("ERROR\n");
            Console.WriteLine(msg);
            Console.Write("\nPress RET To Resume");
            Console.ReadKey();
        }
        static void InteractiveMode()
        {
            Console.Title = "Simple Backup - CLI Mode";
            bool run = true;
            while (run)
            {
                Console.Clear();
                ShowHeader();
                ShowMenu();
                switch (GetInput().ToLower())
                {
                    case "q":
                        run = false;
                        break;
                    default:
                        ShowError("Unknown Input");
                        break;
                }
            }
        }
    }
}
