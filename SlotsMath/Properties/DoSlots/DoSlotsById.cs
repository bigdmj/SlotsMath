using System;
using System.Data;
using System.Collections.Generic;
using SlotsMath.Properties.SlotsComputer;


namespace SlotsMath.Properties.SlotsMethod
{
    public class DoSlotsById
    {
        public void Main()
        {
            //生成DataTable
            Dictionary<string, DataTable> dictionary =
                FileMethod.FileMethod.ExcelToDataTables(
                    Program.configPath+"10001_casino.xlsx");
            //生成SLots
            Slots slots = new Slots(dictionary,3,5,true);
            //调用Slots
            Console.WriteLine("is OK");
        }
    }
    
    
}