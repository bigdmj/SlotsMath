using System.Collections.Generic;

namespace SlotsMath.Properties.SlotsMethod
{
    public class DoSlotsBigBass:DoSlotsById
    {
        public List<int> triggerFreeNeedWild;                //free中追加free需要的wild数
        public List<int> triggerFreeCount;                    //free中追加free的次数
        
        public DoSlotsBigBass(string id) : base(id)
        {
            triggerFreeNeedWild = new List<int>(){4,8,12};
            triggerFreeCount = new List<int>(){2,3,10};
        }
        
        
    }
}