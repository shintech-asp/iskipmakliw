namespace iskipmakliw.Models.DTO
{
    public class MeshyApiResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ModelUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
