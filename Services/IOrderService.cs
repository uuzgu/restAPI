using RestaurantApi.Models;

namespace RestaurantApi.Services
{
    public interface IOrderService
    {
        Task<Order?> GetOrderById(int id);
        Task<Order?> GetOrderByNumber(string orderNumber);
        Task<Order> CreateOrder(Order order);
        Task<Order> UpdateOrder(Order order);
        Task DeleteOrder(int id);
        Task<Order> CreateOrderAsync(
            List<OrderItemRequest> items,
            CustomerOrderInfo? customerInfo,
            string status,
            string paymentMethod,
            decimal totalAmount);
        Task<Order?> UpdateOrderStatusAsync(int orderId, string status);
    }
} 