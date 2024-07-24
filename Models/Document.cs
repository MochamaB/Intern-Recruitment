using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workflows.Attributes;

namespace Workflows.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Display(Name = "ReqId")]
        public int Requisition_id { get; set; }

        [Display(Name = "Intern")]
        public int Intern_id {  get; set; }

        public int DocumentTypeId { get; set; }

        [Column("departmentCode")]
        [Display(Name = "Department")]
        public int DepartmentCode { get; set; }


        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "File Type")]
        public string? FileType { get; set; }

        [Display(Name = "MIME Type")]
        public string? MimeType { get; set; }

        [Display(Name = "Size")]
        public long FileSize { get; set; } = 0;


        [Display(Name = "File Path")]
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please upload a file.")]
        [ValidFile(ErrorMessage = "Please select a valid file.")]
        [NotMapped]
        public IFormFile File { get; set; }

        // Navigation property
        [NotMapped]
        public Intern? Intern { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}
