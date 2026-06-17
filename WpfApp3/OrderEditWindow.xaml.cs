using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp3.Services;
using WpfApp3.Models;

namespace WpfApp3
{
    public partial class OrderEditWindow : Window
    {
        private readonly OrderService _orderService = new();

        private readonly ProductService _productService = new();

        private readonly int? _ordersItemId;

        public OrderEditWindow()
        {
            InitializeComponent();

            _ordersItemId = null;

            Title = "Добавление заказа";

            Loaded += OrderEditWindow_Loaded;
        }

        public OrderEditWindow(int ordersItemId)
        {
            InitializeComponent();

            _ordersItemId = ordersItemId;

            Title = "Редактирование позиции заказа";

            Loaded += OrderEditWindow_Loaded;
        }

        private async void OrderEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ArticleBox.ItemsSource = await _productService.GetArticlesAsync();

            PickupPointBox.ItemsSource = await _orderService.GetPickupPointsAsync();
            PickupPointBox.DisplayMemberPath = "Address";
            PickupPointBox.SelectedValuePath = "Id";

            if (!_ordersItemId.HasValue)
            {
                OrderDatePicker.SelectedDate = DateTime.Today;
                DeliveryDatePicker.SelectedDate = DateTime.Today;
                StatusBox.SelectedIndex = 0;
                return;
            }

            OrderRowModel? order = await _orderService.GetOrderItemByIdAsync(_ordersItemId.Value);

            if (order == null)
            {
                MessageBox.Show(
                    "Позиция заказа не найдена.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Close();
                return;
            }

            ArticleBox.SelectedItem = order.Article;
            PickupPointBox.SelectedValue = order.PickupPointId;
            OrderDatePicker.SelectedDate = order.OrderDate;
            DeliveryDatePicker.SelectedDate = order.DeliveryDate;

            SelectStatus(order.Status);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            string status = ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString()!;

            OrderRowModel order = new OrderRowModel
            {
                OrdersItemId = _ordersItemId ?? 0,
                Article = ArticleBox.SelectedItem!.ToString()!,
                Status = status,
                PickupPointId = (int)PickupPointBox.SelectedValue,
                OrderDate = OrderDatePicker.SelectedDate!.Value,
                DeliveryDate = DeliveryDatePicker.SelectedDate!.Value
            };

            try
            {
                if (_ordersItemId.HasValue)
                {
                    await _orderService.UpdateOrderAsync(order);

                    MessageBox.Show(
                        "Позиция заказа успешно изменена.",
                        "Сохранение заказа",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    await _orderService.AddOrderAsync(order);

                    MessageBox.Show(
                        "Заказ успешно добавлен.",
                        "Сохранение заказа",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }

                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить заказ.\n\nПричина: {ex.Message}",
                    "Ошибка сохранения заказа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private bool ValidateFields()
        {
            if (ArticleBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите артикул товара.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (StatusBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите статус заказа.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (PickupPointBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите пункт выдачи из списка.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (OrderDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату заказа.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (DeliveryDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите дату выдачи.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (DeliveryDatePicker.SelectedDate < OrderDatePicker.SelectedDate)
            {
                MessageBox.Show(
                    "Дата выдачи не может быть раньше даты заказа.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }

        private void SelectStatus(string status)
        {
            foreach (ComboBoxItem item in StatusBox.Items)
            {
                if (item.Content.ToString() == status)
                {
                    StatusBox.SelectedItem = item;
                    return;
                }
            }

            StatusBox.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}