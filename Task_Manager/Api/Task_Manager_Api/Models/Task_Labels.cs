using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Hosting;

namespace Task_Manager_Api.Models
{
    public class Task_Labels
    {
        public int TaskID { get; set; }

        [ForeignKey("TaskID")]
        [SwaggerIgnore]
        public Tasks Tasks { get; set; }

        public int LabelID { get; set; }

        [ForeignKey("LabelID")]
        [SwaggerIgnore]
        public Labels Labels { get; set; }
    }
}
