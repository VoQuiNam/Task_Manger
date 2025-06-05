using Swashbuckle.AspNetCore.Annotations;

namespace Task_Manager_Api.Models
{
    public class Issue_Types
    {
        public int TypeID { get; set; } // Định nghĩa khóa chính

        public string Name { get; set; }

    }
}
