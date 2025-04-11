using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Projects
    {
        [SwaggerIgnore]
        public int ProjectID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }  // Cho phép nhận từ JSON

        [ForeignKey("CreatedBy")]
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public Users User { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
