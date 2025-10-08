namespace iskipmakliw.Services
{
    public interface IPaymongo
    {
        public Task<string> CreateCheckoutSession(
        decimal amount, // Can be removed or kept for validation
        string currency,
        string name,
        string email,
        string contact,
        List<(string name, double price, int quantity)> productDetails,
        string paymentMethod);
        public Task<string> CreateCheckoutSessionService(
                              decimal amount,
                              string currency,
                              string name,
                              string email,
                              string contact,
                              string productNames,
                              string paymentMethod);
        public Task<string> GetCheckoutSession(string sessionId);
        public void Dispose();
    }
}