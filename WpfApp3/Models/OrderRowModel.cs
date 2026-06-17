using System;
using System.Collections.Generic;

namespace WpfApp3.Models
{
    public partial class OrderRowModel
    {
        public int OrderId { get; set; }

        public int OrdersItemId { get; set; }

        public string Article { get; set; } = "";

        public string Status { get; set; } = "";

        public string PickupPointAddress { get; set; } = "";

        public int PickupPointId { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }
    }
}