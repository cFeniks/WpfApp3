using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3.Pages
{
    // страница администратора - каталог товаров с управлением и переходом к заказам
    public partial class AdminPage : Page
    {
        private readonly ProductService _productService = new();

        public AdminPage()
        {
            InitializeComponent();

            // только администратор может выбирать и редактировать карточки
            ProductList.CanSelect = true;

            Loaded += AdminPage_Loaded;
        }

        private async void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSuppliersAsync();

            // установка сортировки запускает применение фильтров
            SortComboBox.SelectedIndex = 0;
        }

        // заполняет список поставщиков для фильтра
        private async Task LoadSuppliersAsync()
        {
            try
            {
                SupplierComboBox.Items.Clear();

                SupplierComboBox.Items.Add("Все поставщики");

                var suppliers = await _productService.GetSuppliersAsync();

                foreach (var supplier in suppliers)
                {
                    SupplierComboBox.Items.Add(supplier.SupplierName);
                }

                SupplierComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список поставщиков.\n\nПричина: {ex.Message}",
                    "Ошибка загрузки поставщиков",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // любой из фильтров изменился - применяем заново
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        // применяет к каталогу поиск, фильтр по поставщику и сортировку
        private void ApplyFilters()
        {
            string supplierName = SupplierComboBox.SelectedItem?.ToString() ?? "Все поставщики";

            ProductList.ApplyFilters(
                SearchBar.Text,
                supplierName,
                SortComboBox.SelectedIndex
            );
        }

        // добавление нового товара
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ProductEditWindow window = new ProductEditWindow();
            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true)
            {
                await ProductList.ReloadProductsAsync();
                ApplyFilters();
            }
        }

        // редактирование выбранного товара
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Product? product = ProductList.SelectedProduct;

            if (product == null)
            {
                MessageBox.Show(
                    "Сначала выберите товар для редактирования.",
                    "Товар не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            ProductEditWindow window = new ProductEditWindow(product.ProductsId);
            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true)
            {
                await ProductList.ReloadProductsAsync();
                ApplyFilters();
            }
        }

        // удаление выбранного товара
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Product? product = ProductList.SelectedProduct;

            if (product == null)
            {
                MessageBox.Show(
                    "Сначала выберите товар, который нужно удалить.",
                    "Товар не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить товар?\n\n{product.ProductName}\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                bool isDeleted = await _productService.DeleteProductAsync(product.ProductsId);

                if (!isDeleted)
                {
                    MessageBox.Show(
                        "Товар нельзя удалить, потому что он присутствует в заказе.",
                        "Удаление запрещено",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }

                MessageBox.Show(
                    "Товар успешно удалён.",
                    "Удаление товара",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                await ProductList.ReloadProductsAsync();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить товар.\n\nПричина: {ex.Message}",
                    "Ошибка удаления товара",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // переход к заказам с правами администратора
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdersPage(true));
        }
    }
}