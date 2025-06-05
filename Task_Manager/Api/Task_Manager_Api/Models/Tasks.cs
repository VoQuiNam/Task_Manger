using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Tasks
    {
        [SwaggerIgnore]
        public int TaskID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int ProjectID { get; set; }  // Cho phép nhận từ JSON

        [ForeignKey("ProjectID")]
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public Projects Project { get; set; }

        public string AssignedTo { get; set; }  // Cho phép nhận từ JSON

        [ForeignKey("AssignedTo")]
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public Users User { get; set; }

        public int StatusID { get; set; }  // Cho phép nhận từ JSON

        [ForeignKey("StatusID")]
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public Task_Status TaskStatus { get; set; }

        public int? ParentTaskID { get; set; }

        [ForeignKey("ParentTaskID")]
        [SwaggerIgnore]
        public Tasks ParentTask { get; set; }

        public int ProjectIssueTypeID { get; set; }

        [ForeignKey("ProjectIssueTypeID")]
        [SwaggerIgnore]
        public Project_Issue_Types Project_Issue_Types { get; set; }

        public DateTime DueDate { get; set; }


        [JsonIgnore] // Không hiển thị trong JSON response
        [BindNever]  // Không nhận từ request
        [SwaggerIgnore] // Ẩn khỏi Swagger UI
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
