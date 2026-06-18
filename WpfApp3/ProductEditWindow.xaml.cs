using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp3.Models;
using WpfApp3.Services;

namespace WpfApp3
{
    // окно добавления и редактирования товара
    public partial class ProductEditWindow : Window
    {
        private readonly ProductService _productService = new();

        // id товара - null означает режим добавления
        private readonly int? _productId;

        // текущий путь к фото и путь к только что выбранному файлу
        private string? _photoPath;

        private string? _newImageSourcePath;

        // режим добавления товара
        public ProductEditWindow()
        {
            InitializeComponent();

            _productId = null;

            Title = "Добавление товара";

            IdPanel.Visibility = Visibility.Collapsed;

            Loaded += ProductEditWindow_Loaded;
        }

        // режим редактирования товара
        public ProductEditWindow(int productId)
        {
            InitializeComponent();

            _productId = productId;

            Title = "Редактирование товара";

            Loaded += ProductEditWindow_Loaded;
        }

        // загрузка справочников и данных товара
        private async void ProductEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComboBoxesAsync();

            if (_productId.HasValue)
            {
                await LoadProductAsync(_productId.Value);
            }
            else
            {
                SetPreviewImage(null);
            }
        }

        // заполняет выпадающие списки справочников
        private async Task LoadComboBoxesAsync()
        {
            CategoryBox.ItemsSource = await _productService.GetCategoriesAsync();
            CategoryBox.DisplayMemberPath = "CategoryName";
            CategoryBox.SelectedValuePath = "CategoryId";

            ManufacturerBox.ItemsSource = await _productService.GetManufacturersAsync();
            ManufacturerBox.DisplayMemberPath = "ManufacturerName";
            ManufacturerBox.SelectedValuePath = "ManufacturerId";

            SupplierBox.ItemsSource = await _productService.GetSuppliersAsync();
            SupplierBox.DisplayMemberPath = "SupplierName";
            SupplierBox.SelectedValuePath = "SupplierId";

            UnitBox.ItemsSource = await _productService.GetUnitsAsync();
            UnitBox.DisplayMemberPath = "UnitsName";
            UnitBox.SelectedValuePath = "UnitsId";
        }

        // подставляет данные товара в поля
        private async Task LoadProductAsync(int productId)
        {
            Product? product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                MessageBox.Show(
                    "Товар не найден.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Close();
                return;
            }

            IdBox.Text = product.ProductsId.ToString();
            ArticleBox.Text = product.Article;
            ProductNameBox.Text = product.ProductName;
            DescriptionBox.Text = product.Description;
            PriceBox.Text = product.Price.ToString();
            QuantityBox.Text = product.Quantity.ToString();
            DiscountBox.Text = product.Discount.ToString();

            CategoryBox.SelectedValue = product.CategoryId;
            ManufacturerBox.SelectedValue = product.ManufacturerId;
            SupplierBox.SelectedValue = product.SupplierId;
            UnitBox.SelectedValue = product.UnitsId;

            _photoPath = product.PhotoPath;

            SetPreviewImage(_photoPath);
        }

        // выбор файла изображения
        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png"
            };

            if (dialog.ShowDialog() != true)
                return;

            _newImageSourcePath = dialog.FileName;

            PreviewImage.Source = new BitmapImage(new Uri(_newImageSourcePath));
        }

        // сохранение - добавление или обновление товара
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields(out int price, out int quantity, out int discount))
                return;

            try
            {
                string? finalPhotoPath = _photoPath;

                if (!string.IsNullOrWhiteSpace(_newImageSourcePath))
                {
                    finalPhotoPath = SaveImage(_newImageSourcePath, _photoPath);
                }

                Product product = new Product
                {
                    ProductName = ProductNameBox.Text.Trim(),
                    CategoryId = (int)CategoryBox.SelectedValue,
                    Description = DescriptionBox.Text.Trim(),
                    ManufacturerId = (int)ManufacturerBox.SelectedValue,
                    SupplierId = (int)SupplierBox.SelectedValue,
                    Price = price,
                    UnitsId = (int)UnitBox.SelectedValue,
                    Quantity = quantity,
                    Discount = discount,
                    PhotoPath = finalPhotoPath,
                    Article = ArticleBox.Text.Trim(),
                    Photo = null
                };

                if (_productId.HasValue)
                {
                    product.ProductsId = _productId.Value;

                    await _productService.UpdateProductAsync(product);

                    MessageBox.Show(
                        "Товар успешно изменён.",
                        "Сохранение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    await _productService.AddProductAsync(product);

                    MessageBox.Show(
                        "Товар успешно добавлен.",
                        "Сохранение",
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
                    $"Не удалось сохранить товар.\n\nПричина: {ex.Message}",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // проверка полей и разбор чисел
        private bool ValidateFields(out int price, out int quantity, out int discount)
        {
            price = 0;
            quantity = 0;
            discount = 0;

            if (string.IsNullOrWhiteSpace(ProductNameBox.Text))
            {
                MessageBox.Show(
                    "Введите наименование товара.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (CategoryBox.SelectedValue == null ||
                ManufacturerBox.SelectedValue == null ||
                SupplierBox.SelectedValue == null ||
                UnitBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите категорию, производителя, поставщика и единицу измерения.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (!int.TryParse(PriceBox.Text, out price) || price < 0)
            {
                MessageBox.Show(
                    "Цена должна быть целым неотрицательным числом.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (!int.TryParse(QuantityBox.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show(
                    "Количество на складе должно быть целым неотрицательным числом.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (!int.TryParse(DiscountBox.Text, out discount) || discount < 0)
            {
                MessageBox.Show(
                    "Скидка должна быть целым неотрицательным числом.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }

        // копирует выбранное фото в папку Resources проекта и возвращает путь к нему
        private string SaveImage(string sourcePath, string? oldRelativePath)
        {
            // папка Resources в корне проекта - на 3 уровня выше папки сборки
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..", "Resources")
            );

            string fileName = $"product_{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(sourcePath)}";

            string targetPath = Path.Combine(projectRoot, fileName);

            // просто копируем выбранный файл
            File.Copy(sourcePath, targetPath, overwrite: true);

            return targetPath;
        }

        // показывает фото товара или заглушку, если его нет
        private void SetPreviewImage(string? relativePath)
        {
            string imagePath;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "picture.png"
                );
            }
            else
            {
                imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    relativePath
                );
            }

            if (!File.Exists(imagePath))
            {
                imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "picture.png"
                );
            }

            PreviewImage.Source = new BitmapImage(new Uri(imagePath));
        }

        // закрытие без сохранения
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}