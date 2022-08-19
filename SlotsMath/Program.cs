using System.Security.Policy;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath
{
    
    static class Program
    {
        public static string configPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/config/";
        public static string logPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/";
        
        private static int doSlotsId;
        public static void Main(string[] args)
        {
            doSlotsId = 1001;
            DoSwitchId(doSlotsId);
        }

        private static void DoSwitchId(int slotsId)
        {
            switch (slotsId)
            {
                case 1001:
                    DoSlotsById doSlotsById = new DoSlotsById();
                    doSlotsById.Main();
                    return;
                case 1002:
                    return;
            }
        }
    }
}