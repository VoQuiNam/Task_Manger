using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Hosting;

namespace Task_Manager_Api.Models
{
    public class Project_Users
    {
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        [SwaggerIgnore]
        public Projects Projects { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")]
        [SwaggerIgnore]
        public Users Users { get; set; }

        public string RoleInProject { get; set; }
    }
}
