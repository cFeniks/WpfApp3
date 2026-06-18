using System;
using System.Collections.Generic;

namespace WpfApp3.Models
{
    // данные одной позиции заказа для карточек и окна редактирования
    public partial class OrderRowModel
    {
        public int OrderId { get; set; }

        // id позиции заказа - по нему идут правка и удаление
        public int OrdersItemId { get; set; }

        public string Article { get; set; } = "";

        public string Status { get; set; } = "";

        // готовый адрес для отображения на карточке
        public string PickupPointAddress { get; set; } = "";

        // выбранный пункт выдачи для сохранения
        public int PickupPointId { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }
    }
}