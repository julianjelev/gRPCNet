namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class ImageInfo
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public bool IsPrimary { get; set; }
        public string ImageFileName { get; set; }
        public string ThumbnailFileName { get; set; }
        public string ForeignKeyId { get; set; }
    }
}
