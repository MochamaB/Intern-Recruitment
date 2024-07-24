using System.ComponentModel.DataAnnotations;
using Workflows.Models;

namespace Workflows.ViewModels
{
    public class DocumentViewModel
    {
        public Document Document { get; set; }
        public DocumentType DocumentType { get; set; }


        public Intern? Intern { get; set; }

        // Additional property for DepartmentName
        public string? DepartmentName { get; set; }

    }

    public class DocumentEditViewModel
    {
        public int Id { get; set; }
        public int Requisition_id { get; set; }
        [Display(Name = "Intern")]
        public int Intern_id { get; set; }
        public int DocumentTypeId { get; set; }
        public int DepartmentCode { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string? MimeType { get; set; }
        public long FileSize { get; set; }
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IFormFile NewFile { get; set; }
    }
}
