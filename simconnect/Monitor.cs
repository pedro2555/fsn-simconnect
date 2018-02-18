using FSUIPC;
using System;
using System.Threading;

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

        MonitorState state;

        public Monitor()
        {
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            // API data
            api = new FlightDataApi(@"https://fsn-flight-data.herokuapp.com");

            // Monitoring state
            state = MonitorState.AwaitSim;

            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (true)
                    {
                        switch (state)
                        {
                            case MonitorState.AwaitSim:
                                if (ConnectToSim())
                                    state = MonitorState.Monitoring;
                                break;
                            case MonitorState.Monitoring:
                                api.Enqueue(new FlightData(true));
                                break;
                        }
                    }
                }
                catch (Exception crap)
                {
                    Console.WriteLine(crap.Message);
                }
            }));

            thread.Start();
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
                        return false;
                    case FSUIPCError.FSUIPC_ERR_OPEN:
                        return true;
                    default:
                        throw crap;
                }
            }
        }
    }
}
