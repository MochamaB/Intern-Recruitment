using System.ComponentModel.DataAnnotations;
using Workflows.Models;

namespace Workflows.ViewModels
{
    public class DocumentViewModel
    {
       
        [Display(Name = "File Name")]
        [Required]
        public string FileName { get; set; }
        public List<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    }
}
