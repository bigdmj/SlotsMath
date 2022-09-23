using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SlotsMath.Properties.SlotsComputer
{
    public class SlotsComputerTest:SlotsComputer
    {
        public Dictionary<int, int> BonusCountDict;  
        public SlotsComputerTest(Dictionary<string, DataTable> dataTableDictionary, string logName , int rowsCount = 3, int columnsCount = 5, bool isWithLine = true) : base(dataTableDictionary, logName, rowsCount, columnsCount, isWithLine)
        {
            BonusCountDict = new Dictionary<int, int>();
            BonusCountDict.Add(3,5);
            BonusCountDict.Add(4,8);
            BonusCountDict.Add(5,15);
        }

        /// <summary>
        /// 计算获取的free次数
        /// </summary>
        /// <param name="freeSymbolCount">free元素数量</param>
        /// <returns></returns>
        public override int GetFreeTime(List<List<int>> symbolArray)
        {
            int outInt = 0;
            List<int> bonusSymbolIdList = new List<int>();
            foreach (SlotsSymbol slotsSymbol in BonusSymbolsList)
            {
                bonusSymbolIdList.Add(slotsSymbol.SymbolId);
            }
            int freeSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, bonusSymbolIdList);
            if (FreeCountDict.Keys.Contains(freeSymbolCount))
            {
                outInt = FreeCountDict[freeSymbolCount];
            }
            else
            {
                outInt = freeSymbolCount > FreeCountDict.Keys.Max() ? FreeCountDict[FreeCountDict.Keys.Max()] : 0;
            }

            return outInt;
        }
        
    }
}