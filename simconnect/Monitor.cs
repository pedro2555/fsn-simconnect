using FSUIPC;
using Newtonsoft.Json;
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
            api.TransponderRegisteredEvent += Api_TransponderRegisteredEvent;

            api.RegisterTransponder(Properties.Settings.Default.transponder);

            NewFlightDataEvent += Monitor_NewFlightDataEvent;

            if (Properties.Settings.Default.transponder != "")
                Console.WriteLine("Your ID: {0}", Properties.Settings.Default.transponder);

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
                                FlightData newData = new FlightData(true);
                                NewFlightDataEvent(newData);

                                //api.Enqueue(newData);
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

        private void Monitor_NewFlightDataEvent(FlightData flightData)
        {
            Console.Clear();
            //Console.WriteLine(JsonConvert.SerializeObject(flightData, Formatting.Indented));
        }

        public delegate void NewFlightDataEventHandler(FlightData flightData);

        public event NewFlightDataEventHandler NewFlightDataEvent;

        private void Api_TransponderRegisteredEvent(object sender, string transponder)
        {
            Properties.Settings.Default.transponder = transponder;
            Properties.Settings.Default.Save();
            Console.WriteLine("Your ID: {0}", Properties.Settings.Default.transponder);
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
