
using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class Intern
    {
        public int Id { get; set; }
        public string? Department_id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Othernames { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set;}
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
