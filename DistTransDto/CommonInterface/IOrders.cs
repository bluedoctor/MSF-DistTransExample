using System;
namespace DistTransCommon.Interface
{
    public interface IOrders
    {
        float AmountPrice { get; set; }
        int ID { get; set; }
        string OrderName { get; set; }
        DateTime OrderTime { get; set; }
        int OwnerID { get; set; }
    }
}
