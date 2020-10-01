using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ABB.Vtrin.Drivers;

namespace CalcEngineTutorialSetup
{
    class Context
    {
        public static string Hostname { get; set; }
        public static string Username { get; set; }
        public static SecureString Password { get; set; }

        public static uint NumberOfSites { get; set; } = 1;

        public static bool Cleanup { get; set; } = false;

        public static string Group { get; set; } = "\\RTDB-CalcUser";

        public static cDriverSkeleton Driver;
    }
}
