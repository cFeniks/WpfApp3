using System;
using System.IO;
using System.Windows;

namespace WpfApp3.Models
{
    public partial class Product
    {
        public bool HasBigDiscount => Discount > 15;
        public bool IsOutOfStock => Quantity == 0;
        public decimal FinalPrice => Price - Price * Discount / 100m;
        public Visibility OldPriceVisibility => Discount > 0 ? Visibility.Collapsed : Visibility.Visible;
        public Visibility DiscountedPriceVisibility => Discount > 0 ? Visibility.Visible : Visibility.Collapsed;

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