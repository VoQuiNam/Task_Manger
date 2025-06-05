namespace Task_Manager_Api.Models
{
    public class AttachmentUploadDto
    {
        public int? FileID { get; set; }
        public IFormFile File { get; set; }

        public int TaskID { get; set; }

        public string UploadedBy { get; set; }
    }
}
