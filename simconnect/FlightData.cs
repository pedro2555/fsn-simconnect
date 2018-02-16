using FSUIPC;
using System;
using System.Device.Location;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simconnect
{
    public class FlightData
    {
        public DateTime TimeStamp
        { get; }


        static public Offset<long> _longitude = new Offset<long>(0x0568);
        static public Offset<long> _latitude = new Offset<long>(0x0560);
        private static GeoCoordinate position
        {
            get
            {
                return new GeoCoordinate(
                    _latitude.Value * (90.0 / (10001750.0 * 65536.0 * 65536.0)),
                    _longitude.Value * (360.0 / (65536.0 * 65536.0 * 65536.0 * 65536.0)));
            }
        }
        public GeoCoordinate Position
        { get; set; }

        private static Offset<double> _compass = new Offset<double>(0x02CC);
        private static int compass
        {
            get
            {
                return (int)Math.Round(_compass.Value);
            }
        }
        public int Compass
        { get; set; }

        private static Offset<short> _altitude = new Offset<short>(0x3324);
        private static short altitude
        {
            get
            {
                return _altitude.Value;
            }
        }
        public short Altitude
        { get; set; }

        private static Offset<short> _groundspeed = new Offset<short>(0x02B4);
        private static short groundspeed
        {
            get
            {
                return Convert.ToInt16((_groundspeed.Value / 65536) * 1.94384449);
            }
        }
        public short GroundSpeed
        { get; set; }

        private static Offset<short> _onground = new Offset<short>(0x0366, false);
        private static bool onground
        {
            get
            {
                return (_onground.Value == 1);
            }
        }
        public bool OnGround
        { get; set; }


        public FlightData(bool InitializeComponent = true)
        {
            this.TimeStamp = DateTime.UtcNow;

            if (InitializeComponent)
                this.InitializeComponent();
        }

        public void InitializeComponent()
        {
            try
            {
                FSUIPCConnection.Process();
            }
            catch (Exception crap)
            {
                Console.WriteLine(crap.Message);
            }

            this.Position = position;
            this.Compass = compass;
            this.Altitude = altitude;
            this.GroundSpeed = groundspeed;
            this.OnGround = onground;
        }
    }
}
