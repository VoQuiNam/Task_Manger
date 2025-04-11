using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Notifications
    {
        [SwaggerIgnore]
        public int NotificationID { get; set; }

        public string UserID { get; set; }  // Cho phép nhận từ JSON

        [ForeignKey("CreatedBy")]
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public Users User { get; set; }

        public string Message { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
