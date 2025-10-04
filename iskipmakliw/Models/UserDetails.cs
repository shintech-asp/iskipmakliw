using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskipmakliw.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string TypeId { get; set; }
        public string GovernmentIdPath { get; set; }
        public string CapturedIdPath { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public int Cvv { get; set; }
        public DateTime ExpirationDate { get; set; }

        // Foreign keys
        public int UsersId { get; set; }
        public Users Users { get; set; }

        public string? Status { get; set; }

        // Uploads (ignored in DB)
        [NotMapped]
        public IFormFile? GovernmentIdFile { get; set; }

        [NotMapped]
        public IFormFile? CapturedIdFile { get; set; }
    }

}
