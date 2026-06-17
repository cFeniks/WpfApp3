using System;
using System.Collections.Generic;

namespace WpfApp3.Models;

public partial class Product
{
    public int ProductsId { get; set; }

    public string Article { get; set; } = null!;

    public int UnitsId { get; set; }

    public int Price { get; set; }

    public int SupplierId { get; set; }

    public int ManufacturerId { get; set; }

    public int CategoryId { get; set; }

    public int Discount { get; set; }

    public int Quantity { get; set; }

    public string Description { get; set; } = null!;

    public byte[]? Photo { get; set; }

    public string ProductName { get; set; } = null!;

    public string? PhotoPath { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrdersItem> OrdersItems { get; set; } = new List<OrdersItem>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Unit Units { get; set; } = null!;
}
