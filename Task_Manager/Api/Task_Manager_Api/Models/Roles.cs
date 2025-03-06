using Swashbuckle.AspNetCore.Annotations;

namespace Task_Manager_Api.Models
{
    public class Roles
    {
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public int RoleID { get; set; } // Định nghĩa khóa chính
        public string RoleName { get; set; }

        // Danh sách Users thuộc Role này
        public ICollection<Users> Users { get; set; }
    }
}
