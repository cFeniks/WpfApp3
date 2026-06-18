using Microsoft.EntityFrameworkCore;
using WpfApp3.Data;
using WpfApp3.Models;

namespace WpfApp3.Services
{
    // работа с заказами и пунктами выдачи
    public class OrderService
    {
        // список пунктов выдачи для выпадающего списка
        public async Task<List<PickupPointRow>> GetPickupPointsAsync()
        {
            await using var db = new AppDbContext();

            return await db.PickupPoints
                .AsNoTracking()
                .OrderBy(point => point.City)
                .Select(point => new PickupPointRow
                {
                    Id = point.PickupPointId,
                    Address =
                        point.PostalCode + ", г. " +
                        point.City + ", ул. " +
                        point.Street + ", дом " +
                        point.Building
                })
                .ToListAsync();
        }

        // все позиции заказов для списка карточек
        public async Task<List<OrderRowModel>> GetOrdersAsync()
        {
            await using var db = new AppDbContext();

            return await db.OrdersItems
                .AsNoTracking()
                .Include(item => item.Orders)
                    .ThenInclude(order => order.Status)
                .Include(item => item.Orders)
                    .ThenInclude(order => order.PickupPoint)
                .Include(item => item.Products)
                .Select(item => new OrderRowModel
                {
                    OrderId = item.OrdersId,
                    OrdersItemId = item.OrdersItemId,
                    Article = item.Products.Article,
                    Status = item.Orders.Status.StatusName,
                    PickupPointAddress =
                        item.Orders.PickupPoint.PostalCode + ", г. " +
                        item.Orders.PickupPoint.City + ", ул. " +
                        item.Orders.PickupPoint.Street + ", дом " +
                        item.Orders.PickupPoint.Building,
                    OrderDate = item.Orders.OrderDate.ToDateTime(TimeOnly.MinValue),
                    DeliveryDate = item.Orders.DeliveryDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToListAsync();
        }

        // данные одной позиции заказа для редактирования
        public async Task<OrderRowModel?> GetOrderItemByIdAsync(int ordersItemId)
        {
            await using var db = new AppDbContext();

            return await db.OrdersItems
                .AsNoTracking()
                .Include(item => item.Orders)
                    .ThenInclude(order => order.Status)
                .Include(item => item.Orders)
                    .ThenInclude(order => order.PickupPoint)
                .Include(item => item.Products)
                .Where(item => item.OrdersItemId == ordersItemId)
                .Select(item => new OrderRowModel
                {
                    OrderId = item.OrdersId,
                    OrdersItemId = item.OrdersItemId,
                    Article = item.Products.Article,
                    Status = item.Orders.Status.StatusName,
                    PickupPointId = item.Orders.PickupPointId,
                    OrderDate = item.Orders.OrderDate.ToDateTime(TimeOnly.MinValue),
                    DeliveryDate = item.Orders.DeliveryDate.ToDateTime(TimeOnly.MinValue)
                })
                .FirstOrDefaultAsync();
        }

        // создаёт новый заказ с одним товаром
        public async Task AddOrderAsync(OrderRowModel orderRow)
        {
            await using var db = new AppDbContext();

            Product? product = await db.Products
                .FirstOrDefaultAsync(product => product.Article == orderRow.Article);

            if (product == null)
            {
                throw new Exception("Товар с указанным артикулом не найден.");
            }

            Status? status = await db.Statuses
                .FirstOrDefaultAsync(status => status.StatusName == orderRow.Status);

            if (status == null)
            {
                throw new Exception("Указанный статус заказа не найден.");
            }

            PickupPoint? pickupPoint = await db.PickupPoints
                .FirstOrDefaultAsync(point => point.PickupPointId == orderRow.PickupPointId);

            if (pickupPoint == null)
            {
                throw new Exception("Указанный пункт выдачи не найден.");
            }

            int nextOrderId = await db.Orders.AnyAsync()
                ? await db.Orders.MaxAsync(order => order.OrdersId) + 1
                : 1;

            Order order = new Order
            {
                OrdersId = nextOrderId,
                Status = status,
                PickupPoint = pickupPoint,
                OrderDate = DateOnly.FromDateTime(orderRow.OrderDate),
                DeliveryDate = DateOnly.FromDateTime(orderRow.DeliveryDate)
            };

            OrdersItem orderItem = new OrdersItem
            {
                OrdersId = nextOrderId,
                ProductsId = product.ProductsId,
                Quantity = 1
            };

            db.Orders.Add(order);
            db.OrdersItems.Add(orderItem);

            await db.SaveChangesAsync();
        }

        // обновляет выбранную позицию и общие поля её заказа
        public async Task UpdateOrderAsync(OrderRowModel orderRow)
        {
            await using var db = new AppDbContext();

            OrdersItem? orderItem = await db.OrdersItems
                .Include(item => item.Orders)
                    .ThenInclude(order => order.Status)
                .Include(item => item.Orders)
                    .ThenInclude(order => order.PickupPoint)
                .FirstOrDefaultAsync(item => item.OrdersItemId == orderRow.OrdersItemId);

            if (orderItem == null)
            {
                throw new Exception("Позиция заказа не найдена.");
            }

            Order order = orderItem.Orders;

            Product? product = await db.Products
                .FirstOrDefaultAsync(product => product.Article == orderRow.Article);

            if (product == null)
            {
                throw new Exception("Товар с указанным артикулом не найден.");
            }

            Status? status = await db.Statuses
                .FirstOrDefaultAsync(status => status.StatusName == orderRow.Status);

            if (status == null)
            {
                throw new Exception("Указанный статус заказа не найден.");
            }

            PickupPoint? pickupPoint = await db.PickupPoints
                .FirstOrDefaultAsync(point => point.PickupPointId == orderRow.PickupPointId);

            if (pickupPoint == null)
            {
                throw new Exception("Указанный пункт выдачи не найден.");
            }

            order.Status = status;
            order.PickupPoint = pickupPoint;
            order.OrderDate = DateOnly.FromDateTime(orderRow.OrderDate);
            order.DeliveryDate = DateOnly.FromDateTime(orderRow.DeliveryDate);

            orderItem.ProductsId = product.ProductsId;

            await db.SaveChangesAsync();
        }

        // удаляет позицию, а если она последняя - удаляет и заказ
        public async Task DeleteOrderItemAsync(int ordersItemId)
        {
            await using var db = new AppDbContext();

            OrdersItem? orderItem = await db.OrdersItems
                .FirstOrDefaultAsync(item => item.OrdersItemId == ordersItemId);

            if (orderItem == null)
                return;

            int orderId = orderItem.OrdersId;

            db.OrdersItems.Remove(orderItem);
            await db.SaveChangesAsync();
            
            bool hasItemsLeft = await db.OrdersItems
                .AnyAsync(item => item.OrdersId == orderId);

            if (!hasItemsLeft)
            {
                Order? order = await db.Orders
                    .FirstOrDefaultAsync(order => order.OrdersId == orderId);

                if (order != null)
                {
                    db.Orders.Remove(order);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}