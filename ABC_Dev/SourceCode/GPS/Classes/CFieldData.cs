using System;

namespace ABC
{
    public class CFieldData
    {
        private readonly FormGPS mf;

        //all the section area added up;
        public double workedAreaTotal;

        //just a cumulative tally based on distance and eq width.
        public double workedAreaTotalUser;

        //accumulated user distance
        public double distanceUser;

        //not really used - but if needed
        public double userSquareMetersAlarm;

        //constructor
        public CFieldData(FormGPS _f)
        {
            mf = _f;
            workedAreaTotal = 0;
            workedAreaTotalUser = 0;
            userSquareMetersAlarm = 0;
        }

        //USer tally string
        public string WorkedUserHectares { get { return (workedAreaTotalUser * glm.m2ha).ToString("N2") + " Ha"; } }

        public string WorkedUserHectares2 { get { return (workedAreaTotalUser * glm.m2ha).ToString("N2"); } }

        //user tally string
        public string WorkedUserAcres { get { return (workedAreaTotalUser * glm.m2ac).ToString("N2") + " Ac"; } }

        public string WorkedUserAcres2 { get { return (workedAreaTotalUser * glm.m2ac).ToString("N2"); } }

        //String of Area worked
        public string WorkedAcres
        {
            get
            {
                if (workedAreaTotal < 404048) return (workedAreaTotal * 0.000247105).ToString("N2");
                else return (workedAreaTotal * 0.000247105).ToString("N1");
            }
        }

        public string WorkedHectares
        {
            get
            {
                if (workedAreaTotal < 99000) return (workedAreaTotal * 0.0001).ToString("N2");
                else return (workedAreaTotal * 0.0001).ToString("N1");
            }
        }

        //User Distance strings
        public string DistanceUserMeters { get { return Convert.ToString((UInt16)(distanceUser)) + " m"; } }

        public string DistanceUserFeet { get { return Convert.ToString((UInt16)(distanceUser * glm.m2ft)) + " ft"; } }

        public string WorkRateHectares { get { return (mf.vehicle.toolWidth * mf.pn.speed * 0.1).ToString("N1") + "\r\nHa/hr"; } }
        public string WorkRateAcres { get { return (mf.vehicle.toolWidth * mf.pn.speed * 0.2471).ToString("N1") + "\r\nAc/hr"; } }
    }
}