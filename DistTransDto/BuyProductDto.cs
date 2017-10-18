using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransDto
{
    /// <summary>
    /// 准备购买的商品简要信息
    /// </summary>
    public class BuyProductDto
    {
        public int ProductId { get; set; }
        public int BuyNumber { get; set; }
    }

    /// <summary>
    /// 卖出的上牌简要信息
    /// </summary>
    public class SellProductDto : BuyProductDto
    { 
        /// <summary>
        /// 发货地点，在更新库存的时候才知道
        /// </summary>
        public string StoreHouse { get; set; }
    }
}
