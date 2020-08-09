using System.Security.Policy;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath
{
    internal class Program
    {
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