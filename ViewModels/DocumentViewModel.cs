using System.ComponentModel.DataAnnotations;
using Workflows.Models;

namespace Workflows.ViewModels
{
    public class DocumentViewModel
    {
        public Document Document { get; set; }
        public DocumentType DocumentType { get; set; }

    }
}
