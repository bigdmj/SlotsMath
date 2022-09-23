/*
 * 玩法说明
 * 一共10条中奖线
 * 每个鱼元素会有随机money，在freeGame中wild和鱼元素同时出现时可以获得鱼身上的奖金，每有一个wild可获得1倍所有鱼奖金
 * wild元素在freeGame时会出现在所有列
 * scatter元素会出现在所有列
 * 3个scatter会获得10次free，4个scatter-15次，5个scatter-20次
 * 在free中每累积出现4个wild，会再次追加free，分别是追加2/3/10次
 */
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath.Properties.SlotsComputer
{
    /// <summary>
    /// 模拟结果，添加了moneySymbolWin，鱼元素的奖金
    /// </summary>
    public class SimulateDateInfoBigBass : SimulateDataInfo
    {
        public List<double> moenySymbolWinList;            //鱼元素的奖金列表
        public int wildCount;                                //wild元素的数量
        
        
        public SimulateDateInfoBigBass()
        {
            moenySymbolWinList = new List<double>();
            wildCount = 0;
           
        }        

        public SimulateDateInfoBigBass(List<SlotsSymbol> slotsSymbols)
        {
            moenySymbolWinList = new List<double>();
            wildCount = 0;
        }

        public override void Reset()
        {
            SpinType = SpinType.BseSpin;
            ReelName = "";
            Position.Clear();
            SymbolArray.Clear();
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList.Clear();
            NormalSymbolWinInfoDict.Clear();
            moenySymbolWinList.Clear();
            wildCount = 0;
        }
    }
    
    public class SlotsComputerBigBass:SlotsComputer
    {
        private List<double> moneySymbolValueList; //鱼元素金额权重字典 key是money金额，value是权重
        private List<double> moneySymbolweightList; //鱼元素金额权重字典 key是money金额，value是权重
        
        SimulateDateInfoBigBass SimulateDataInfoObject;

        public SlotsComputerBigBass(Dictionary<string, DataTable> dataTableDictionary, string logName, int rowsCount = 3, int columnsCount = 5, bool isWithLine = true) : base(dataTableDictionary, logName, rowsCount, columnsCount, isWithLine)
        {
            SimulateDataInfoObject = new SimulateDateInfoBigBass(NormalSymbolsList);
            
            FreeCountDict[3] = 10;        //free元素和free回合数对应关系
            FreeCountDict[4] = 15;       
            FreeCountDict[5] = 20;

//            ScatterRewardDict[3] = 1;
//            ScatterRewardDict[3] = 2;
//            ScatterRewardDict[3] = 3;
            SetReelsWeight(dataTableDictionary);
           
        }
        
        /// <summary>
        /// 计算获取的free次数
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <returns></returns>
        public override int GetFreeTime(List<List<int>> symbolArray)
        {
            int outInt = 0;
            List<int> symbolIds = new List<int>();
            foreach (SlotsSymbol slotsSymbol in ScatterSymbolsList) //olympus是scatter触发free，这里改成了统计scatter元素的数量
            {
                symbolIds.Add(slotsSymbol.SymbolId);
            }

            int symbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, symbolIds); //获取free元素的总数量
            if (FreeCountDict.Keys.Contains(symbolCount))
            {
                outInt = FreeCountDict[symbolCount];
            }
            else
            {
                outInt = 0;
            }
            return outInt;
        }

        /// <summary>
        /// free中获得的money元素的奖励金额
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <returns></returns>
        protected double GetMoneySymbolWin(List<List<int>> symbolArray)
        {
            int wildCount = SlotsTools.GetSymbolCountByArray(symbolArray, 21);
            if (wildCount ==0)
            {
                return 0;
            }
            int moneySymbolCount = SlotsTools.GetSymbolCountByArray(symbolArray, 5);
            double moneyValue = 0;
            for (int i = 0; i < moneySymbolCount; i++)
            {
                moneyValue += SlotsTools.GetRandomObjByWeight(moneySymbolValueList,moneySymbolweightList); 
            }
            moneyValue *= wildCount;
            return moneyValue;
        }
        
        
        
        
        
    }
}