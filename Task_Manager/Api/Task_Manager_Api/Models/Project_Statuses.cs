using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Hosting;

namespace Task_Manager_Api.Models
{
    public class Project_Statuses
    {
        public int ProjectStatusID { get; set; }

        [ForeignKey("ProjectID")]
        [SwaggerIgnore]
        public Projects Projects { get; set; }

        public int ProjectID { get; set; }

        [ForeignKey("StatusID")]
        [SwaggerIgnore]
        public Task_Status Task_Status { get; set; }

        public int StatusID { get; set; }
    }
}
