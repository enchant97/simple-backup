using System;
using SimpleBackup.Core;

namespace SimpleBackup.InterfaceConsole
{
    public static class Utils
    {
        public static string GetTitle()
        {
            return Constants.AppName + " - CLI Mode";
        }
        public static void ShowHeader()
        {
            Console.WriteLine(GetTitle());
            Console.WriteLine(new String('-', Console.WindowWidth));
        }
        public static string GetInput()
        {
            Console.Write(">> ");
            return Console.ReadLine();
        }
        public static bool ShowYesNoInput(string msg)
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
        public static string ShowStringInput(string msg)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("ENTER STRING\n");
            Console.WriteLine(msg);
            return GetInput();
        }
        public static int ShowIntInput(string msg)
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
        public static void ShowResume()
        {
            Console.Write("\nPress RET To Resume");
            Console.ReadKey();
        }
        public static void ShowError(string msg)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine("ERROR\n");
            Console.WriteLine(msg);
            ShowResume();
        }
    }
}
