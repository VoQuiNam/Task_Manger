using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager_Api.Models
{
    public class Project_Issue_Types
    {
        public int ProjectIssueTypeID { get; set; }

        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        [SwaggerIgnore]
        public Projects Projects { get; set; }

        public int TypeID { get; set; }

        [ForeignKey("TypeID")]
        [SwaggerIgnore]
        public Issue_Types Issue_Types { get; set; }
    }
}
