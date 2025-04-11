using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Task_Status
    {
        [SwaggerIgnore]
        public int StatusID { get; set; }

        public string Name { get; set; }

        public string ColorCode { get; set; }
    }
}
