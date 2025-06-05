using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Attachments
    {
        public int FileID { get; set; }

        public string FilePath { get; set; }

        public int TaskID { get; set; }

        [ForeignKey("TaskID")]
        public Tasks Tasks { get; set; }

        public string UploadedBy { get; set; }

        [ForeignKey("UploadedBy")]
        public Users Users { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
