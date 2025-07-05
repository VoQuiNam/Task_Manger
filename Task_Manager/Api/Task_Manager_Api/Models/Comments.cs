using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Comments
    {
        [SwaggerIgnore]
        public int CommentID { get; set; }

        [ForeignKey("TaskID")]
        [SwaggerIgnore]
        public Tasks Tasks { get; set; }

        public int TaskID { get; set; }

        [ForeignKey("UserID")]
        [SwaggerIgnore]
        public Users Users { get; set; }

        public string UserID { get; set; }

        public string Content { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ParentCommentID")]
        [SwaggerIgnore]
        public Comments Comment { get; set; }

        public int ParentCommentID { get; set; }
    }
}
