using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Users
    {
        public int UserID { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int RoleID { get; set; }

        //giúp truy xuất thông tin Role của User dễ dàng.
        [ForeignKey("RoleID")]
        public Roles Role { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
