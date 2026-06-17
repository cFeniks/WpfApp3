using System;
using System.Collections.Generic;

namespace WpfApp3.Models;

public partial class OrdersItem
{
    public int OrdersItemId { get; set; }

    public int ProductsId { get; set; }

    public int OrdersId { get; set; }

    public int Quantity { get; set; }

    public virtual Order Orders { get; set; } = null!;

    public virtual Product Products { get; set; } = null!;
}
