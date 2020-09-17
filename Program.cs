using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcEngineTutorialSetup
{
    class Program
    {
        private static string hostname;
        private static string username;
        private static string password;

        private static uint numberOfSites = 1;

        private static bool cleanup = false;

        private static List<IFixture> fixtures;

        static void Main(string[] args)
        {
            parseArgs(args);

            using (DBConnection dbConnection = new DBConnection(hostname, username, password))
            {
                dbConnection.ConnectOrThrow();

                Console.WriteLine("Connection successful");

                fixtures = new List<IFixture>()
                {
                    new VariablesFixture(dbConnection.RTDBDriver),
                    new EquipmentModelFixture(dbConnection.RTDBDriver, numberOfSites),
                };

                if (cleanup)
                {
                    Cleanup();
                }
                else
                {
                    Setup();
                }
            }
        }

        static void Setup()
        {
            foreach (var fixture in fixtures)
            {
                fixture.Setup();
            }
        }

        static void Cleanup()
        {
            foreach (var fixture in fixtures)
            {
                fixture.Cleanup();
            }
        }

        static void parseArgs(string[] args)
        {
            if (args[0] == "cleanup")
            {
                cleanup = true;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" && i < args.Length - 1)
                {
                    hostname = args[i + 1];
                }
                if (args[i] == "-u" && i < args.Length - 1)
                {
                    username = args[i + 1];
                }
                // TODO It's not a secure way to enter the password into a command
                if (args[i] == "-p" && i < args.Length - 1)
                {
                    password = args[i + 1];
                }
                if (args[i] == "-s" && i < args.Length - 1)
                {
                    numberOfSites = uint.Parse(args[i + 1]);
                }
            }

            if (hostname == null)
            {
                throw new System.ArgumentException("Define hostname using -h <hostname>");
            }
            if (username == null)
            {
                throw new System.ArgumentException("Define username using -u <username>");
            }
            if (password == null)
            {
                throw new System.ArgumentException("Define password using -p <password>");
            }
        }
    }
}
