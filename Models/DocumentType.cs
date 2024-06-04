using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class DocumentType
    {
        public int Id { get; set; }
        [Display(Name = "Document Name")]
        [Required]
        public string DocumentName { get; set; }

        public string MimeType { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Document> Document { get; set; }
    }
}
