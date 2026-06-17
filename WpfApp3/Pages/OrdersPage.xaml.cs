using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp3.Services;
using WpfApp3.Models;

namespace WpfApp3.Pages
{
    public partial class OrdersPage : Page
    {
        private readonly OrderService _orderService = new();

        private readonly bool _isAdmin;

        public OrdersPage(bool isAdmin)
        {
            InitializeComponent();

            _isAdmin = isAdmin;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            AdminButtonsPanel.Visibility = _isAdmin
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                OrdersList.ItemsSource = await _orderService.GetOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список заказов.\n\nПричина: {ex.Message}",
                    "Ошибка загрузки заказов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OrderEditWindow window = new OrderEditWindow();

            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is not OrderRowModel selectedOrder)
            {
                MessageBox.Show(
                    "Сначала выберите позицию для редактирования.",
                    "Позиция не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            OrderEditWindow window = new OrderEditWindow(selectedOrder.OrdersItemId);

            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is not OrderRowModel selectedOrder)
            {
                MessageBox.Show(
                    "Сначала выберите позицию для удаления.",
                    "Позиция не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить позицию «{selectedOrder.Article}» из заказа №{selectedOrder.OrderId}?\n\nЕсли это последняя позиция, заказ будет удалён целиком.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _orderService.DeleteOrderItemAsync(selectedOrder.OrdersItemId);

                MessageBox.Show(
                    "Позиция успешно удалена.",
                    "Удаление позиции",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить позицию.\n\nПричина: {ex.Message}",
                    "Ошибка удаления позиции",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}