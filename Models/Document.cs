using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class Document
    {
        public int Id { get; set; }

   
        [Required]
        public int Requisition_id { get; set; }

        [Display(Name = "Intern")]
        [Required]
        public int Intern_id {  get; set; }

        [Required]
        public int DocumentType { get; set; }

        [Display(Name = "File Name")]
        [Required]
        public string? FileName { get; set; }

        public string? FileType { get; set; }
        public long FileSize { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
