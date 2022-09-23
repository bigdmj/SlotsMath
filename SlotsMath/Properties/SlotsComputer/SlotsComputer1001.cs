using System.Collections.Generic;
using System.Data;

namespace SlotsMath.Properties.SlotsComputer
{
    public class SlotsComputer1001:SlotsComputer
    {
        public SlotsComputer1001(Dictionary<string, DataTable> dataTableDictionary, string logName, int rowsCount = 3, int columnsCount = 5, bool isWithLine = false) : base(dataTableDictionary, logName, rowsCount, columnsCount, isWithLine)
        {
        }

        /// <summary>
        /// 根据位置号获取元素矩阵，如果出现了触发了相同列也会处理
        /// </summary>
        /// <param name="position">位置号列表</param>
        /// <param name="type"></param>
        /// <param name="sameCount"></param>
        /// <returns></returns>
        public List<List<int>> GetSameReeList(List<int> position,string typeKey,int sameCount)
        {
            List<List<int>> outList = new List<List<int>>();
            List<List<int>> oriArray = BaseSlotsReels.SlotsReelsDictionary[typeKey].GetArray(position);
            switch (sameCount)
            {
                case 2:
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[2]);
                    outList.Add(oriArray[3]);
                    outList.Add(oriArray[4]);
                    return outList;
                case 3:
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[3]);
                    outList.Add(oriArray[4]);
                    return outList;
                case 4:
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[0]);
                    outList.Add(oriArray[4]);
                    return outList;
                default: return oriArray;
            }
            
        }
    }
}