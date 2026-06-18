using System;
using System.Collections.Generic;

namespace WpfApp3.Models;

// заказ - может содержать несколько позиций (OrdersItem)
public partial class Order
{
    public int OrdersId { get; set; }

    public DateOnly OrderDate { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int PickupPointId { get; set; }

    public int UsersId { get; set; }

    public string Code { get; set; } = null!;

    public int StatusId { get; set; }

    public virtual ICollection<OrdersItem> OrdersItems { get; set; } = new List<OrdersItem>();

    public virtual PickupPoint PickupPoint { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual User Users { get; set; } = null!;
}
