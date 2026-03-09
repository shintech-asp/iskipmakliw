namespace iskipmakliw.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Collections
        public List<Users> RecentSubscribers { get; set; } = new();
        public List<Users> PurchasedPlans { get; set; } = new();
        public List<Users> RiderApplications { get; set; } = new();
        public List<Users> RecentCustomers { get; set; } = new();

        // Statistics
        public int TotalSubscribers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int PendingRiders { get; set; }
        public int TotalCustomers { get; set; }

        // Chart Data
        public List<MonthlySubscriberData> MonthlySubscriberData { get; set; } = new();
    }

    public class MonthlySubscriberData
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }
}
