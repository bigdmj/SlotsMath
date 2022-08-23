using System.Security.Policy;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath
{
    
    static class Program
    {
        public static string configPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/config/"; //配置文件路径
        public static string logPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/";  //日志文件路径(txt)
        public static string ExcelSavePath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/";    //输出的excel路径
        public static bool EnableSaveLog = true; //是否将日志输出
        
        
        private static int doSlotsId;
        public static void Main()
        {
            doSlotsId = 10001;
            DoSwitchId(doSlotsId);
        }

        private static void DoSwitchId(int slotsId)
        {
            switch (slotsId)
            {
                case 10001:
                    DoSlotsById doSlotsById = new DoSlotsById(10001);
                    doSlotsById.Main();
                    return;
                case 1002:
                    return;
            }
        }
    }
}