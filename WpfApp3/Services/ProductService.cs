using System;
using Microsoft.EntityFrameworkCore;
using WpfApp3.Data;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    public class ProductService
    {
        public async Task<List<Product>> GetProductsAsync()
        {
            await using var db = new AppDbContext();

            return await db.Products
                .AsNoTracking()
                .Include(product => product.Manufacturer)
                .Include(product => product.Supplier)
                .Include(product => product.Units)
                .Include(product => product.OrdersItems)
                .ToListAsync();
        }

        public async Task<List<string>> GetArticlesAsync()
        {
            await using var db = new AppDbContext();

            return await db.Products
                .AsNoTracking()
                .OrderBy(product => product.Article)
                .Select(product => product.Article)
                .ToListAsync();
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            await using var db = new AppDbContext();

            Product? product = await db.Products
                .Include(product => product.OrdersItems)
                .FirstOrDefaultAsync(product => product.ProductsId == productId);

            if (product == null)
                return false;

            if (product.OrdersItems.Any())
                return false;

            db.Products.Remove(product);

            await db.SaveChangesAsync();

            return true;
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            await using var db = new AppDbContext();

            return await db.Categories
                .AsNoTracking()
                .OrderBy(category => category.CategoryName)
                .ToListAsync();
        }

        public async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            await using var db = new AppDbContext();

            return await db.Manufacturers
                .AsNoTracking()
                .OrderBy(manufacturer => manufacturer.ManufacturerName)
                .ToListAsync();
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            await using var db = new AppDbContext();

            return await db.Suppliers
                .AsNoTracking()
                .OrderBy(supplier => supplier.SupplierName)
                .ToListAsync();
        }

        public async Task<List<Unit>> GetUnitsAsync()
        {
            await using var db = new AppDbContext();

            return await db.Units
                .AsNoTracking()
                .OrderBy(unit => unit.UnitsName)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            await using var db = new AppDbContext();

            return await db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.ProductsId == productId);
        }

        public async Task AddProductAsync(Product product)
        {
            await using var db = new AppDbContext();

            int nextId = await db.Products.AnyAsync()
                ? await db.Products.MaxAsync(product => product.ProductsId) + 1
                : 1;

            product.ProductsId = nextId;

            if (string.IsNullOrWhiteSpace(product.Article))
            {
                product.Article = await GenerateArticleAsync(db);
            }

            db.Products.Add(product);

            await db.SaveChangesAsync();
        }

        private static async Task<string> GenerateArticleAsync(AppDbContext db)
        {
            const string letters = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЭЮЯ";

            Random random = new Random();

            List<string> existing = await db.Products
                .Select(product => product.Article)
                .ToListAsync();

            string article;

            do
            {
                article =
                    letters[random.Next(letters.Length)].ToString() +
                    random.Next(100, 1000) +
                    letters[random.Next(letters.Length)] +
                    random.Next(0, 10);
            }
            while (existing.Contains(article));

            return article;
        }

        public async Task UpdateProductAsync(Product updatedProduct)
        {
            await using var db = new AppDbContext();

            Product? product = await db.Products
                .FirstOrDefaultAsync(product => product.ProductsId == updatedProduct.ProductsId);

            if (product == null)
                return;

            if (!string.IsNullOrWhiteSpace(updatedProduct.Article))
            {
                product.Article = updatedProduct.Article;
            }

            product.ProductName = updatedProduct.ProductName;
            product.CategoryId = updatedProduct.CategoryId;
            product.Description = updatedProduct.Description;
            product.ManufacturerId = updatedProduct.ManufacturerId;
            product.SupplierId = updatedProduct.SupplierId;
            product.Price = updatedProduct.Price;
            product.UnitsId = updatedProduct.UnitsId;
            product.Quantity = updatedProduct.Quantity;
            product.Discount = updatedProduct.Discount;
            product.PhotoPath = updatedProduct.PhotoPath;

            await db.SaveChangesAsync();
        }
    }
}