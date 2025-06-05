using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/attachments")]
    public class AttachmentsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public AttachmentsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetAttachments")]
        public JsonResult GetAttachments()
        {
            string query = "select * from dbo.Attachments";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPost]
        [Route("AddAttachments")]
        public async Task<IActionResult> AddAttachments([FromForm] AttachmentUploadDto dto)
        {
            // Validate if file is provided
            if (dto.File == null || dto.File.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Không có file nào được tải lên." });
            }

            try
            {
                // Use ContentRootPath instead of WebRootPath
                string uploadFolder = Path.Combine(_env.ContentRootPath, "Uploads");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                string fullPath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                string fileUrl = "/Uploads/" + fileName;

                string insertQuery = @"
INSERT INTO dbo.Attachments (TaskID, FilePath, UploadedBy, CreatedAt)
VALUES (@TaskID, @FilePath, @UploadedBy, GETDATE())";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(insertQuery, myCon))
                    {
                        cmd.Parameters.AddWithValue("@TaskID", dto.TaskID);
                        cmd.Parameters.AddWithValue("@FilePath", fileUrl);
                        cmd.Parameters.AddWithValue("@UploadedBy", dto.UploadedBy ?? (object)DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult(new { success = true, message = "Tải file thành công!", path = fileUrl });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteAttachment")]
        public async Task<IActionResult> DeleteAttachment(int fileId)
        {
            try
            {
                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");
                string filePath = null;

                // Lấy đường dẫn file từ DB trước khi xóa
                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();
                    string selectQuery = "SELECT FilePath FROM Attachments WHERE FileID = @FileID";

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, myCon))
                    {
                        selectCmd.Parameters.AddWithValue("@FileID", fileId);
                        var result = await selectCmd.ExecuteScalarAsync();

                        if (result == null)
                        {
                            return new JsonResult(new { success = false, message = "Không tìm thấy tệp để xóa." });
                        }

                        filePath = result.ToString();
                    }

                    // Xóa bản ghi khỏi DB
                    string deleteQuery = "DELETE FROM Attachments WHERE FileID = @FileID";

                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@FileID", fileId);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }
                }

                // Xóa file khỏi ổ đĩa (nếu tồn tại)
                if (!string.IsNullOrEmpty(filePath))
                {
                    // Đường dẫn tuyệt đối đến file
                    string physicalPath = Path.Combine(_env.ContentRootPath, filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                return new JsonResult(new { success = true, message = "Xóa file thành công." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateAttachment")]
        public async Task<IActionResult> UpdateAttachment([FromForm] AttachmentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Không có file mới được tải lên." });
            }

            try
            {
                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");
                string oldFilePath = null;

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Lấy đường dẫn cũ từ DB
                    string selectQuery = "SELECT FilePath FROM Attachments WHERE FileID = @FileID";
                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, myCon))
                    {
                        selectCmd.Parameters.AddWithValue("@FileID", dto.FileID);
                        var result = await selectCmd.ExecuteScalarAsync();

                        if (result == null)
                        {
                            return new JsonResult(new { success = false, message = "Không tìm thấy tệp để cập nhật." });
                        }

                        oldFilePath = result.ToString();
                    }

                    // Xóa file cũ trên ổ đĩa nếu tồn tại
                    if (!string.IsNullOrEmpty(oldFilePath))
                    {
                        string physicalOldPath = Path.Combine(_env.ContentRootPath, oldFilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(physicalOldPath))
                        {
                            System.IO.File.Delete(physicalOldPath);
                        }
                    }

                    // Tạo tên file mới và lưu
                    string uploadFolder = Path.Combine(_env.ContentRootPath, "Uploads");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string newFileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                    string newFilePath = Path.Combine(uploadFolder, newFileName);

                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await dto.File.CopyToAsync(stream);
                    }

                    string dbPath = "/Uploads/" + newFileName;

                    // Cập nhật đường dẫn mới trong DB
                    string updateQuery = @"
UPDATE Attachments 
SET FilePath = @FilePath, CreatedAt = GETDATE() 
WHERE FileID = @FileID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@FileID", dto.FileID);
                        updateCmd.Parameters.AddWithValue("@FilePath", dbPath);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult(new { success = true, message = "Cập nhật file thành công." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAttachmentByFileID")]
        public async Task<IActionResult> GetAttachmentByFileID(int fileID)
        {
            try
            {
                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    string selectQuery = @"
SELECT FileID, FilePath, TaskID, UploadedBy, CreatedAt 
FROM Attachments 
WHERE FileID = @FileID";

                    using (SqlCommand cmd = new SqlCommand(selectQuery, myCon))
                    {
                        cmd.Parameters.AddWithValue("@FileID", fileID);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var attachment = new
                                {
                                    FileID = reader["FileID"],
                                    FilePath = reader["FilePath"],
                                    TaskID = reader["TaskID"],
                                    UploadedBy = reader["UploadedBy"],
                                    CreatedAt = reader["CreatedAt"]
                                };

                                return new JsonResult(new { success = true, data = attachment });
                            }
                            else
                            {
                                return new JsonResult(new { success = false, message = "Không tìm thấy file với FileID này." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAttachmentsByTaskID")]
        public async Task<IActionResult> GetAttachmentsByTaskID(int taskID)
        {
            try
            {
                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");
                var attachments = new List<object>();

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    string query = "SELECT * FROM Attachments WHERE TaskID = @TaskID";
                    using (SqlCommand cmd = new SqlCommand(query, myCon))
                    {
                        cmd.Parameters.AddWithValue("@TaskID", taskID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                attachments.Add(new
                                {
                                    FileID = reader["FileID"],
                                    FilePath = reader["FilePath"],
                                    TaskID = reader["TaskID"],
                                    UploadedBy = reader["UploadedBy"],
                                    CreatedAt = reader["CreatedAt"]
                                });
                            }
                        }
                    }
                }

                return new JsonResult(attachments);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("DownloadAttachment")]
        public IActionResult DownloadAttachment(string fileName)
        {
            // Thư mục chứa file upload
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
            var filePath = Path.Combine(uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { success = false, message = "File không tồn tại." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream"; // Hoặc dùng MimeMapping nếu muốn chính xác theo file
            return File(fileBytes, contentType, fileName);
        }



    }
}
