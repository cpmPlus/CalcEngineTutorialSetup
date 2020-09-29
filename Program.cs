using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CalcEngineTutorialSetup
{
    class Program
    {

        private static List<IFixture> fixtures;

        static void Main(string[] args)
        {
            parseArgs(args);

            if (Context.Password == null)
            {
                getPassword();
            }

            using (DBConnection dbConnection = new DBConnection(Context.Hostname, Context.Username, Context.Password))
            {
                dbConnection.ConnectOrThrow();

                Context.Driver = dbConnection.RTDBDriver;

                Console.WriteLine("Connection successful");

                fixtures = new List<IFixture>()
                {
                    new VariablesFixture(),
                    new EquipmentModelFixture(),
                };

                if (Context.Cleanup)
                {
                    Cleanup();
                    Console.WriteLine("Cleanup successful");
                }
                else
                {
                    Setup();
                    Console.WriteLine("Setup successful");
                }
            }
        }

        static private void Setup()
        {
            foreach (var fixture in fixtures)
            {
                fixture.Setup();
            }
        }

        static private void Cleanup()
        {
            foreach (var fixture in fixtures)
            {
                fixture.Cleanup();
            }
        }

        static private void parseArgs(string[] args)
        {
            if (args[0] == "cleanup")
            {
                Context.Cleanup = true;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" && i < args.Length - 1)
                {
                    Context.Hostname = args[i + 1];
                }
                if (args[i] == "-u" && i < args.Length - 1)
                {
                    Context.Username = args[i + 1];
                }
                if (args[i] == "-p" && i < args.Length - 1)
                {
                    Context.Password = new NetworkCredential("", args[i + 1]).SecurePassword;
                }
                if (args[i] == "-s" && i < args.Length - 1)
                {
                    Context.NumberOfSites = uint.Parse(args[i + 1]);
                }
                if (args[i] == "-c" && i < args.Length - 1)
                {
                    Context.CalcUsername = args[i + 1];
                }
            }

            if (Context.Hostname == null)
            {
                throw new System.ArgumentException("Define hostname using -h <hostname>");
            }
            if (Context.Username == null)
            {
                throw new System.ArgumentException("Define username using -u <username>");
            }
        }

        static private void getPassword()
        {
            Console.Write($"Password for {Context.Username}@{Context.Hostname}: ");

            // From https://stackoverflow.com/a/3404522/2876520

            var password = new SecureString();

            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000') // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                {
                    password.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }

            Context.Password = password;
        }
    }
}
