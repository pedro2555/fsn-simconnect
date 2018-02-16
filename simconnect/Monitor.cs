using FSUIPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace simconnect
{
    public enum MonitorState
    {
        AwaitSim,
        AwaitFlightPlan,
        Monitoring
    }

    class Monitor : System.Windows.Forms.ApplicationContext
    {
        FlightDataApi api;

        Timer t;

        MonitorState state;

        public Monitor()
        {
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            // API data
            api = new FlightDataApi();

            // Main timer
            t = new Timer(1000);
            t.Elapsed += T_Elapsed;

            // Monitoring state
            state = MonitorState.AwaitSim;

            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (state)
            {
                case MonitorState.AwaitSim:
                    if (ConnectToSim())
                    {
                        state = MonitorState.Monitoring;
                        t.Interval = 1000;
                    }
                    break;
                case MonitorState.Monitoring:
                    var data = new FlightData();
                    Console.WriteLine("QNH {0}", data.QNH);

                    Console.WriteLine(api.needsPush(new FlightData()));
                    break;
            }
        }

        private bool ConnectToSim()
        {
            try
            {
                FSUIPCConnection.Open();

                return true;
            }
            catch (FSUIPCException crap)
            {
                switch (crap.FSUIPCErrorCode)
                {
                    case FSUIPCError.FSUIPC_ERR_NOFS:
                        break;
                    case FSUIPCError.FSUIPC_ERR_OPEN:
                        return true;
                }
            }
            return false;
        }
    }
}
