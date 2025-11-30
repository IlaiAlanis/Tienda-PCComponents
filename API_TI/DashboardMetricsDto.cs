using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class DashboardMetricsDto
    {
        [JsonPropertyName("totalSales")]
        public decimal TotalSales { get; set; }
        [JsonPropertyName("salesToday")]
        public decimal SalesToday { get; set; }
        [JsonPropertyName("salesLastWeek")]
        public decimal SalesLast7Days { get; set; }
        [JsonPropertyName("salesLastMonth")]
        public decimal SalesLast30Days { get; set; }
        [JsonPropertyName("totalOrders")]
        public int TotalOrders { get; set; }
        [JsonPropertyName("ordersToday")]
        public int OrdersToday { get; set; }
        [JsonPropertyName("pendingOrders")]
        public int PendingOrders { get; set; }
        [JsonPropertyName("completeOrders")]
        public int CompletedOrders { get; set; }
        [JsonPropertyName("totalUsers")]
        public int TotalUsers { get; set; }
        [JsonPropertyName("activeUsers")]
        public int ActiveUsers { get; set; }
        [JsonPropertyName("newUsersToday")]
        public int NewUsersToday { get; set; }
        [JsonPropertyName("newUsersLastWeek")]
        public int NewUsersLast7Days { get; set; }
        [JsonPropertyName("newUsersCount")]
        public int NewUsersLast30Days { get; set; }
        [JsonPropertyName("totalProducts")]
        public int TotalProducts { get; set; }
        [JsonPropertyName("lowStockProducts")]
        public int LowStockProducts { get; set; }
        [JsonPropertyName("outOfStockProducts")]
        public int OutOfStockProducts { get; set; }
        [JsonPropertyName("topProducts")]
        public List<TopProductDto> TopSellingProducts { get; set; }
        [JsonPropertyName("dailyRevenue")]
        public List<DailyRevenueDto> RevenueTrend { get; set; }

        [JsonPropertyName("salesData")]
        public List<WeeklySalesDto> SalesData { get; set; }
        [JsonPropertyName("lowStockProductsList")]
        public List<LowStockProductDto> LowStockProductsList { get; set; }
        [JsonPropertyName("recentOrders")]
        public List<RecentOrderDto> RecentOrders { get; set; }
    }
}
