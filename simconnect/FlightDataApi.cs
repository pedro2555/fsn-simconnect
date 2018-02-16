using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simconnect
{
    public class FlightDataApi
    {
        private FlightData lastSentData;
        public bool needsPush(FlightData data)
        {
            if (   (lastSentData == null) // this is the base case
                // turns of more than 5 degrees
                || (CompassDelta(data.Compass, lastSentData.Compass) > 5)
                // altitude changes of more than 50ft
                || (Math.Abs(data.Altitude - lastSentData.Altitude) > 50)
                // groundspeed changes of more than 10kts
                || (Math.Abs(data.GroundSpeed - lastSentData.GroundSpeed) > 10)
                // on ground we should be a bit more agreesive
                || (data.OnGround && 
                    // movements over 2.5 meter
                    (lastSentData.Position.GetDistanceTo(data.Position) > 2.5))
                || (!data.OnGround && 
                    // movements over 500 meters
                    (lastSentData.Position.GetDistanceTo(data.Position) > 500))
                // more than 5minute without a report
                || ((data.TimeStamp - lastSentData.TimeStamp).TotalMinutes > 5))
            {
                lastSentData = data;

                return true;
            }

            // no need to push
            return false;
        }

        private int CompassDelta(int firstAngle, int secondAngle)
        {
            double difference = secondAngle - firstAngle;
            while (difference < -180) difference += 360;
            while (difference > 180) difference -= 360;
            return (int)difference;
        }
    }
}
