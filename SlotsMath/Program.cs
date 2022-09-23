using System;
using System.Collections.Generic;
using System.Security.Policy;
using ICSharpCode.SharpZipLib.Zip;
using SlotsMath.Properties;
using SlotsMath.Properties.CheckSlots;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath
{
    
    static class Program
    {
        public static string configPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/config/"; //配置文件路径
        public static string logPath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/";  //日志文件路径(txt)
        public static string ExcelSavePath = "/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/";    //输出的excel路径
        public static bool EnableSaveLog = true; //是否将日志输出
        
        
        private static string SlotsId;
        private static string mode;
        public static void Main()
        {
            SlotsId = "test";
            mode = "simulate";
//            mode = "simulate";
            if (mode == "check")
            {
                CheckSwitchId(SlotsId);
            }
            if (mode == "simulate")
            {
                DoSwitchId(SlotsId);
            }
            
            
        }

        private static void DoSwitchId(string slotsId)
        {
            switch (slotsId)
            {
                case "test":
                    DoSlotsById doSlotsById = new DoSlotsById("test");
                    doSlotsById.Main(100,100);
                    return;
                case "OlympusXUP":
                    DoSlotsOlympusXUP doSlotsOlympusXup = new DoSlotsOlympusXUP("OlympusXUP");
                    doSlotsOlympusXup.Main(100,100);
                    return;
                case "999":
                    foreach (int po in GetPositionListFromLog("[1,2,3,4,5]"))
                    {
                        Console.WriteLine(po);
                    }
                    return;
            }
        }
        
        private static void CheckSwitchId(string slotsId)
        {
            switch (slotsId)
            {
                case "1001":
                    CheckSlots checkSlots = new CheckSlots();
                    checkSlots.Main();
                    return;
                case "999":
                    foreach (int po in GetPositionListFromLog("[1,2,3,4,5]"))
                    {
                        Console.WriteLine(po);
                    }
                    return;
            }
        }
        
        static List<int> GetPositionListFromLog(string positionString)
        {
            
            string newList =  positionString.Substring(1,positionString.Length-2);
            System.String[] lista = newList.Split(new[] {','});
            List<int> outList = new List<int>();
            foreach (string VARIABLE in lista)
            {
                outList.Add(Convert.ToInt32(VARIABLE));
            }
            return outList;
        }
    }
}