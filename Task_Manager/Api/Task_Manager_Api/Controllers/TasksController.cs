using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/tasks")]
    public class TasksController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public TasksController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetTasks")]
        public JsonResult GetTasks()
        {
            string query = "select * from dbo.Tasks";
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
        [Route("AddTasks")]
        public async Task<IActionResult> AddTasks([FromBody] Tasks obj)
        {
            try
            {
                string insertQuery = @"
        INSERT INTO dbo.Tasks 
            (Title, Description, ProjectID, AssignedTo, StatusID, ParentTaskID, DueDate, CreatedAt)
        VALUES 
            (@Title, @Description, @ProjectID, @AssignedTo, @StatusID, @ParentTaskID, @DueDate, GETDATE())";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@Title", obj.Title ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Description", obj.Description ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ProjectID", obj.ProjectID);
                        insertCmd.Parameters.AddWithValue("@AssignedTo", obj.AssignedTo ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@StatusID", obj.StatusID);
                        insertCmd.Parameters.AddWithValue("@ParentTaskID",
                        (obj.ParentTaskID == null || obj.ParentTaskID == 0)
                        ? (object)DBNull.Value
                        : obj.ParentTaskID);
                        insertCmd.Parameters.AddWithValue("@DueDate", obj.DueDate == default ? (object)DBNull.Value : obj.DueDate);
                        //default của DateTime là 01/01/0001 00:00:00 (rất hiếm khi là ngày hợp lệ).
                        //Nếu DueDate vẫn giữ giá trị mặc định(chưa set) → gán DBNull.Value.
                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm task thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm task." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteTasks")]
        public async Task<IActionResult> DeleteTasks([FromQuery] int TaskID)
        {
            try
            {
                if (TaskID <= 0)
                {
                    return new JsonResult(new { success = false, message = "TaskID không hợp lệ." });
                }

                // Kiểm tra xem Label có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Tasks WHERE TaskID = @TaskID";
                string deleteQuery = "DELETE FROM dbo.Tasks WHERE TaskID = @TaskID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của Label
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TaskID", TaskID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Task không tồn tại." });
                        }
                    }

                    // Xóa Label nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@TaskID", TaskID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa Task thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa Task." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateTasks")]
        public async Task<IActionResult> UpdateTasks([FromQuery] int TaskID, [FromBody] Tasks updateData)
        {
            try
            {
                updateData.TaskID = TaskID;
                if (updateData == null || updateData.TaskID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkTaskQuery = "SELECT COUNT(*) FROM dbo.Tasks WHERE TaskID = @TaskID";
                string updateQuery = @"
    UPDATE dbo.Tasks
    SET Title = @Title,
        Description = @Description,
        ProjectID = @ProjectID,
        AssignedTo = @AssignedTo,
        StatusID = @StatusID,
        ParentTaskID = @ParentTaskID,
        DueDate = @DueDate
    WHERE TaskID = @TaskID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Task có tồn tại không
                    using (SqlCommand checkCmd = new SqlCommand(checkTaskQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TaskID", updateData.TaskID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Task không tồn tại." });
                        }
                    }

                    // Cập nhật thông tin Task
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@TaskID", updateData.TaskID);
                        updateCmd.Parameters.AddWithValue("@Title", updateData.Title ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Description", updateData.Description ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        updateCmd.Parameters.AddWithValue("@AssignedTo", updateData.AssignedTo ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        updateCmd.Parameters.AddWithValue("@ParentTaskID", updateData.ParentTaskID == 0 ? (object)DBNull.Value : updateData.ParentTaskID);
                        updateCmd.Parameters.AddWithValue("@DueDate", updateData.DueDate == default ? (object)DBNull.Value : updateData.DueDate);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật Task thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật Task." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

    }
}
