using System.Xml.Serialization;

namespace Scripts.jukes
{
	public class Configuration
	{

        public bool PirateGridDeletion_Enabled = true;
        public int PirateGridDeletion_Interval = 40 * 60 * 1000;
        public double PirateGridDeletion_PlayerDistanceThreshold = 7005;
        //public int PirateGridDeletion_BlockCountThreshold = 50;
        public bool PirateGridDeletion_MessageAdminsOnly = true;
    }
}
