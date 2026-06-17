using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3.Controls
{
    public partial class ProductListControl : UserControl
    {
        private readonly ProductService _productService = new();
        private Border? _selectedBorder;
        private List<Product> _products = new();
        public event Action<Product>? ProductSelected;

        public Product? SelectedProduct { get; private set; }

        // Разрешает выбор и редактирование карточек.
        public bool CanSelect { get; set; } = false;

        public ProductListControl()
        {
            InitializeComponent();
             }

        private async void ProductListControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadProductsAsync();
        }

        public async Task ReloadProductsAsync()
        {
            try
            {
                _products = await _productService.GetProductsAsync();

                ProductsItemsControl.ItemsSource = _products;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить товары.\n\nПричина: {ex.Message}",
                    "Ошибка загрузки товаров",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public void ApplyFilters(string searchText, string supplierName, int sortIndex)
        {
            IEnumerable<Product> result = _products;

            searchText = searchText.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result = result.Where(product =>
                    ContainsText(product.ProductName, searchText) ||
                    ContainsText(product.Description, searchText) ||
                    ContainsText(product.Article, searchText) ||
                    ContainsText(product.Manufacturer?.ManufacturerName, searchText) ||
                    ContainsText(product.Supplier?.SupplierName, searchText) ||
                    ContainsText(product.Units?.UnitsName, searchText));
            }

            if (!string.IsNullOrWhiteSpace(supplierName) &&
                supplierName != "Все поставщики")
            {
                result = result.Where(product =>
                    product.Supplier != null &&
                    product.Supplier.SupplierName == supplierName);
            }

            if (sortIndex == 1)
            {
                result = result.OrderBy(product => product.Quantity);
            }
            else if (sortIndex == 2)
            {
                result = result.OrderByDescending(product => product.Quantity);
            }

            ProductsItemsControl.ItemsSource = result.ToList();
        }

        private static bool ContainsText(string? value, string searchText)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.ToLower().Contains(searchText);
        }

        private void ProductCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Выбор карточки доступен только администратору
            if (!CanSelect)
                return;

            if (sender is not Border border)
                return;

            if (border.DataContext is not Product product)
                return;

            SelectedProduct = product;

            ProductSelected?.Invoke(product);

            if (_selectedBorder != null)
                _selectedBorder.Background = Brushes.Transparent;

            border.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            _selectedBorder = border;
        }

        private void ProductCard_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Редактирование карточки доступно только администратору
            if (!CanSelect)
                return;

            if (sender is not Border border)
                return;

            if (border.DataContext is not Product product)
                return;

            var window = new ProductEditWindow(product.ProductsId);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            _ = ReloadProductsAsync();

            e.Handled = true;
        }
    }
}