using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp3.Services;

namespace WpfApp3.Pages
{
    // страница менеджера - просмотр каталога и заказов без управления товарами
    public partial class ManagerPage : Page
    {
        private readonly ProductService _productService = new();

        public ManagerPage()
        {
            InitializeComponent();

            Loaded += ManagerPage_Loaded;
        }

        private async void ManagerPage_Loaded(object sender, RoutedEventArgs e)
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

        // переход к заказам только для просмотра - без кнопок добавления и удаления
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdersPage(false));
        }
    }
}