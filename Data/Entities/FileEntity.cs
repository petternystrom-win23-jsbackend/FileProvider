using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class FileEntity
    {
        [Key]
        [Required]
        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public string UploaderName { get; set; } = null!;

        public DateTime UploadDate { get; set; }
        public string FilePath { get; set; } = null!;

        public string ContainerName { get; set; } = null!;
    }
}
