using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Hosting;

namespace Task_Manager_Api.Models
{
    public class RoleModules
    {
        [SwaggerIgnore]
        public int RoleModuleID { get; set; }



        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        [SwaggerIgnore]
        public Roles Role { get; set; }


        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public int ModuleID { get; set; }

        [ForeignKey("ModuleID")]
        [SwaggerIgnore]
        public Modules Modules { get; set; }

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
    }
}
