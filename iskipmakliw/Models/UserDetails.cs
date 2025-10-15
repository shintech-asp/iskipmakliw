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
        public string? CardName { get; set; }
        public string? CardNumber { get; set; }
        public int? Cvv { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Zip { get; set; }
        public string? LandMark { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }

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
