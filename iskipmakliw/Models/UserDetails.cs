using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskipmakliw.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string TypeId { get; set; }
        public byte[]? GovernmentId { get; set; }
        public byte[]? CapturedId { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public int Cvv { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int? PlansId { get; set; }
        public Plans Plans { get; set; }
        public string? Status { get; set; }
        public string? Subscription { get; set; }
        public int? SubscriptionDuration { get; set; }

        [NotMapped]
        public IFormFile? GovernmentIdFile { get; set; }

        [NotMapped]
        public IFormFile? CapturedIdFile { get; set; }
    }
}
