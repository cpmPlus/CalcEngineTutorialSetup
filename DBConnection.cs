using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.Vtrin;
using ABB.Vtrin.Drivers;

namespace CalcEngineTutorialSetup
{
    class DBConnection: IDisposable
    {
        private readonly cDataLoader dataloader = new cDataLoader();
        public cDriverSkeleton RTDBDriver;

        private readonly string RTDBHost;
        private readonly string RTDBUsername;
        private readonly string RTDBPassword;

        public DBConnection(string _RTDBHost, string _RTDBUsername, string _RTDBPassword)
        {
            RTDBHost = _RTDBHost;
            RTDBUsername = _RTDBUsername;
            RTDBPassword = _RTDBPassword;
        }

        public void ConnectOrThrow()
        {
            // Set up a memory stream to catch exceptions
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var listener = new System.Diagnostics.TextWriterTraceListener(memoryStream, "connectlistener");
                System.Diagnostics.Trace.Listeners.Add(listener);

                // Set connection options
                dataloader.ConnectOptions =
                    ABB.Vtrin.cDataLoader.cConnectOptions.AcceptNewServerKeys
                    | ABB.Vtrin.cDataLoader.cConnectOptions.AcceptServerKeyChanges;

                // Initialize the database driver
#pragma warning disable CS0618 // Type or member is obsolete
                RTDBDriver = dataloader.Connect(
                    RTDBHost,
                    RTDBUsername,
                    RTDBPassword,
                    false);
#pragma warning restore CS0618 // Type or member is obsolete

                // Unbind the connect listener
                System.Diagnostics.Trace.Listeners.Remove("connectlistener");

                // Case: driver is null, something went wrong
                // > throw an error
                if (RTDBDriver == null)
                {
                    // Read stack trace from the memorystream buffer
                    string msg = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer());
                    throw new System.ApplicationException(msg);
                }
            }
        }

        public void Dispose()
        {
            if (dataloader != null)
                dataloader.Dispose();
        }
    }
}
