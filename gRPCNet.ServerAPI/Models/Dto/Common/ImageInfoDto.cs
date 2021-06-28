namespace gRPCNet.ServerAPI.Models.Dto.Common
{
    public class ImageInfoDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public bool IsPrimary { get; set; }
        public string ImageFileName { get; set; }
        public string ThumbnailFileName { get; set; }
        public string ForeignKeyId { get; set; }
    }
}
