namespace Task_Manager_Api.Models
{
    public class UpdateTaskLabelDto
    {
        public int TaskID { get; set; }
        public int OldLabelID { get; set; }
        public int NewLabelID { get; set; }
    }
}
