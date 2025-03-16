using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Users
    {
        [SwaggerIgnore]
        public int id { get; set; }
        [SwaggerIgnore]
        public string UserID { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public int RoleID { get; set; }

        //giúp truy xuất thông tin Role của User dễ dàng.
        [ForeignKey("RoleID")]
        [SwaggerIgnore]
        public Roles Role { get; set; }

        [NotMapped] // Thuộc tính không lưu vào database
        public string RoleName => Role?.RoleName; // Trả về RoleName thay vì RoleID

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
