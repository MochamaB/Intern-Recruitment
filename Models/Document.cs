using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflows.Models
{
    public class Document
    {
        public int Id { get; set; }

   
       
        public int Requisition_id { get; set; }

        [Display(Name = "Intern")]
        public int Intern_id {  get; set; }

        public int DocumentType { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public string? FileType { get; set; }
        public long FileSize { get; set; } = 0;
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please upload a file.")]
        [NotMapped]
        public IFormFile File { get; set; }
    }
}
