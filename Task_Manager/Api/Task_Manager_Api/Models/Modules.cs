using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Hosting;

namespace Task_Manager_Api.Models
{
    public class Modules
    {
        [SwaggerIgnore]
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int ParentID { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        /*-- Xác định module có yêu cầu quyền thực thi không*/
        public bool IsAction { get; set; }

        public string Link { get; set; }

        public string Icon { get; set; }
        public int OrderNumber { get; set; }

        /*-- Trạng thái kích hoạt(1: hoạt động, 0: ẩn)*/
        public bool IsActive { get; set; }

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime UpdatedAt { get; set; }

    }
}
