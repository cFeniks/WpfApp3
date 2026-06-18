using System;
using System.IO;
using System.Windows;

namespace WpfApp3.Models
{
    // вычисляемые свойства товара для отображения на карточке
    public partial class Product
    {
        // скидка больше 15 процентов - карточка подсвечивается
        public bool HasBigDiscount => Discount > 15;

        // товара нет в наличии
        public bool IsOutOfStock => Quantity == 0;

        // цена с учётом скидки
        public decimal FinalPrice => Price - Price * Discount / 100m;

        // показываем обычную цену, когда скидки нет
        public Visibility OldPriceVisibility => Discount > 0 ? Visibility.Collapsed : Visibility.Visible;

        // показываем перечёркнутую цену и новую, когда есть скидка
        public Visibility DiscountedPriceVisibility => Discount > 0 ? Visibility.Visible : Visibility.Collapsed;

        // путь к фото товара или к заглушке, если фото нет
        public string ProductImagePath
        {
            get
            {
                string placeholderPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "picture.png"
                );

                if (string.IsNullOrWhiteSpace(PhotoPath))
                {
                    return new Uri(placeholderPath).AbsoluteUri;
                }

                string cleanPhotoPath = PhotoPath.Trim();

                string imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    cleanPhotoPath
                );

                return new Uri(imagePath).AbsoluteUri;
            }
        }
    }
}