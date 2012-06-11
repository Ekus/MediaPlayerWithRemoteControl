using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace MediaPlayerWithRemoteControl
{
    class RemoteControlHost : IDisposable
    {
        private bool isInitialized;

        private ServiceHost innerServiceHost;
        private MainWindow MainWindowReference;

        public RemoteControlHost(MainWindow window)
        {
            MainWindowReference = window;
            InitializeServiceHost();
        }

        private void InitializeServiceHost()
        {
            try
            {
                RemoteControl remoteControl = new RemoteControl(MainWindowReference);
                innerServiceHost = new ServiceHost(remoteControl);
                innerServiceHost.Opened += new EventHandler(innerServiceHost_Opened);
                innerServiceHost.Faulted += new EventHandler(innerServiceHost_Faulted);
                innerServiceHost.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to initialize RemoteControlHost", ex);
            }
        }



        void innerServiceHost_Faulted(object sender, EventArgs e)
        {
            this.innerServiceHost.Abort();

            if (isInitialized)
            {
                isInitialized = false;
                InitializeServiceHost();
            }
        }

        void innerServiceHost_Opened(object sender, EventArgs e)
        {
            isInitialized = true;
        }

        public void Dispose()
        {
            try{
                innerServiceHost.Opened -= innerServiceHost_Opened;
                innerServiceHost.Faulted -= innerServiceHost_Faulted;
                innerServiceHost.Close();
            }
            catch
            {
                try {innerServiceHost.Abort();} catch {}
            }

        }
    }
}