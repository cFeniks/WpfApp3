using System;
using System.Collections.Generic;

namespace WpfApp3.Models;

public partial class Unit
{
    public int UnitsId { get; set; }

    public string UnitsName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
