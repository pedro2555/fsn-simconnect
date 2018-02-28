using FSUIPC;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using System;
using System.Device.Location;
using System.Linq;

namespace simconnect
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FlightData
    {
        public struct GeoJsonPoint
        {
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator !=(GeoJsonPoint obj1, GeoJsonPoint obj2)
            {
                return (obj1.Coordinates != obj2.Coordinates);
            }
            public static bool operator ==(GeoJsonPoint obj1, GeoJsonPoint obj2)
            {
                return (obj1.Coordinates == obj2.Coordinates);
            }

            [JsonProperty(PropertyName = "type")]
            public string Type
            {
                get { return "Point"; }
                set { }
            }

            [JsonProperty(PropertyName = "coordinates")]
            [DeserializeAs(Name = "coordinates")]
            public double[] Coordinates
            { get; set; }
        }

        [JsonProperty(PropertyName = "timestamp")]
        [DeserializeAs(Name = "timestamp")]
        public DateTime TimeStamp
        { get; set; }

        [JsonProperty(PropertyName = "transponder")]
        [DeserializeAs(Name = "transponder")]
        public string Transponder
        { get; set; }

        /// <summary>
        /// Position
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// json output for API
        /// </summary>
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
        [JsonProperty(PropertyName = "position")]
        [DeserializeAs(Name = "position")]
        public GeoJsonPoint JsonPosition
        {
            get
            {
                return new GeoJsonPoint()
                {
                    Coordinates = new double[]
                    {
                        position.Longitude,
                        position.Latitude
                    }
                };
            }
            set { }
        }

        /// <summary>
        /// Compass heading
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<double> _compass = new Offset<double>(0x02CC);
        private static int compass
        {
            get
            {
                return (int)Math.Round(_compass.Value);
            }
        }
        [JsonProperty(PropertyName = "compass")]
        [DeserializeAs(Name = "compass")]
        public int Compass
        { get; set; }

        /// <summary>
        /// Altitude
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<int> _altitude = new Offset<int>(0x3324);
        private static int altitude
        {
            get
            {
                return _altitude.Value;
            }
        }
        [JsonProperty(PropertyName = "altitude")]
        [DeserializeAs(Name = "altitude")]
        public int Altitude
        { get; set; }

        /// <summary>
        /// Groundspeed
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<int> _groundspeed = new Offset<int>(0x02B4);
        private static int groundspeed
        {
            get
            {
                return Convert.ToInt32((_groundspeed.Value / 65536) * 1.94384449);
            }
        }
        [JsonProperty(PropertyName = "groundspeed")]
        [DeserializeAs(Name = "groundspeed")]
        public int GroundSpeed
        { get; set; }

        /// <summary>
        /// Indicated Airspeed
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<int> _indicatedairspeed = new Offset<int>(0x02BC);
        private static int indicatedairspeed
        {
            get
            {
                return _indicatedairspeed.Value / 128;
            }
        }
        [JsonProperty(PropertyName = "indicatedairspeed")]
        [DeserializeAs(Name = "indicatedairspeed")]
        public int IndicatedAirspeed
        { get; set; }

        /// <summary>
        /// True Airspeed
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<int> _trueairspeed = new Offset<int>(0x02B8);
        private static int trueairspeed
        {
            get
            {
                return _trueairspeed.Value / 128;
            }
        }
        [JsonProperty(PropertyName = "trueairspeed")]
        [DeserializeAs(Name = "trueairspeed")]
        public int TrueAirpeed
        { get; set; }

        /// <summary>
        /// On ground value
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
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
        
        /// <summary>
        /// Pressure setting in millibars
        /// 
        /// Offsets
        /// static proterty for correctly processing Offset data
        /// instance property
        /// </summary>
        private static Offset<short> _qnh = new Offset<short>(0x0330);
        private static short qnh
        {
            get
            {
                return Convert.ToInt16(_qnh.Value / 16);
            }
        }
        [JsonProperty(PropertyName = "qnh")]
        [DeserializeAs(Name = "qnh")]
        public short QNH
        { get; set; }

        public FlightData()
            : this(false) { }

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
            this.IndicatedAirspeed = indicatedairspeed;
            this.TrueAirpeed = trueairspeed;
            this.OnGround = onground;
            this.QNH = qnh;
        }

        /// <summary>
        /// Returns a new FlightData object with values that difer betwen obj1 and obj2
        /// 
        /// Does not change objt1 or obj2
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static FlightData FilterDiferences(FlightData obj1, FlightData obj2)
        {
            if (obj2 == null)
                return obj1;

            FlightData res = new FlightData(false);

            foreach (
                var property in
                obj1.GetType().GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(DeserializeAsAttribute))))
            {
                if ((dynamic)property.GetValue(obj1) != (dynamic)property.GetValue(obj2))
                    property.SetValue(res, property.GetValue(obj1));
            }
            res.Transponder = obj2.Transponder;
            return res;
        }
    }
}
