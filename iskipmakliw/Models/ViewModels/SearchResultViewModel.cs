namespace iskipmakliw.Models.ViewModels
{
    public class SearchResultViewModel
    {
        public List<ClientViewModel> Products { get; set; }
        public List<SellerViewModel> Sellers { get; set; }
    }

    public class SellerViewModel
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public string Role { get; set; }
        public string? ProfileImage { get; set; }
    }
}
