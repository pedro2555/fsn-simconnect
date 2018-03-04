using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.Collections.Generic;

namespace simconnect
{
    public class FlightDataApi
    {
        public string BaseUrl
        { get; }
        public JsonSerializerSettings SerializationSettings;
        public string Transponder;

        private FlightData lastSentData;

        public FlightDataApi(string BaseUrl)
        {
            this.BaseUrl = BaseUrl;
            this.SerializationSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new IsoDateTimeConverter()
                    {
                        DateTimeFormat = "ddd, dd MMM yyyy hh:mm:ss G\\MT",
                        DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal,
                        Culture = new System.Globalization.CultureInfo("")
                    }
                }
            };
        }

        public void ExecuteAsyncRequest<K>(RestRequest request, K obj = null)
            where K : class, new()
        {
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter(
                "application/json",
                JsonConvert.SerializeObject(obj, SerializationSettings),
                ParameterType.RequestBody);
            string te = JsonConvert.SerializeObject(obj, SerializationSettings);
            new RestClient(BaseUrl).ExecuteAsync<K>(request, (response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    dynamic d = response.Data;
                    switch (request.Resource)
                    {
                        case "transponders":
                            TransponderRegisteredEvent(this, d["_id"]);
                            break;
                        case "position-reports":
                            // Console.WriteLine("{0} {1}", response.StatusCode, response.Data["_id"]);
                            break;

                    }
                }
                else
                    Console.WriteLine(response.StatusCode);
            });
        }

        public delegate void TransponderRegisteredEventHandler(object sender, string transponder);
        public event TransponderRegisteredEventHandler TransponderRegisteredEvent;
        public void RegisterTransponder(string transponder = null)
        {
            if (transponder != "")
                Transponder = transponder;
            else
            {
                // register app on the flight-data-api
                ExecuteAsyncRequest<dynamic>(
                    new RestRequest("transponders", Method.POST), new Dictionary<dynamic, dynamic>());
            }
        }

        public bool Enqueue(FlightData data)
        {
            if ((lastSentData == null) // this is the base case
                                       // turns of more than 5 degrees
                || (CompassDelta(data.Compass, lastSentData.Compass) > 7)
                // altitude changes of more than 50ft
                || (Math.Abs(data.Altitude - lastSentData.Altitude) > 150)
                // groundspeed changes of more than 10kts
                || (Math.Abs(data.GroundSpeed - lastSentData.GroundSpeed) > 27)
                // on ground we should be a bit more agreesive
                || (data.OnGround &&
                    // movements over 100 meters
                    (lastSentData.Position.GetDistanceTo(data.Position) > 15))
                || (!data.OnGround &&
                    // movements over 5 Km
                    (lastSentData.Position.GetDistanceTo(data.Position) > 5700))
                // more than 5minute without a report
                || ((data.TimeStamp - lastSentData.TimeStamp).TotalMinutes > 5))
            {
                data.Transponder = Transponder;
                ExecuteAsyncRequest(
                    new RestRequest("position-reports", Method.POST),
                    FlightData.FilterDiferences(data, lastSentData));

                lastSentData = data;

                return true;
            }

            // no need to push
            return false;
        }

        private int CompassDelta(int firstAngle, int secondAngle)
        {
            double difference = secondAngle - firstAngle;
            while (difference < -180)
                difference += 360;
            while (difference > 180)
                difference -= 360;
            return (int)difference;
        }
    }
}
