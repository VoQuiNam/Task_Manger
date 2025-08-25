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
    (Title, Description, ProjectID, AssignedTo, StatusID, ParentTaskID, ProjectIssueTypeID, DueDate, CreatedAt)
OUTPUT INSERTED.TaskID
VALUES 
    (@Title, @Description, @ProjectID, @AssignedTo, @StatusID, @ParentTaskID, @ProjectIssueTypeID, @DueDate, GETDATE())";

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
                        insertCmd.Parameters.AddWithValue("@ProjectIssueTypeID", obj.ProjectIssueTypeID);
                        insertCmd.Parameters.AddWithValue("@DueDate",
                            obj.DueDate == default ? (object)DBNull.Value : obj.DueDate);

                        // Lấy TaskID vừa tạo ra
                        object result = await insertCmd.ExecuteScalarAsync();

                        if (result != null)
                        {
                            int createdTaskId = Convert.ToInt32(result);
                            return new JsonResult(new
                            {
                                success = true,
                                message = "Thêm task thành công!",
                                task = new { TaskID = createdTaskId } // ✅ Trả TaskID về client
                            });
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
                    return new JsonResult(new { success = false, message = "Invalid TaskID." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlTransaction transaction = myCon.BeginTransaction())
                    {
                        try
                        {
                            // Check if task exists
                            string checkQuery = "SELECT COUNT(*) FROM dbo.Tasks WHERE TaskID = @TaskID";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@TaskID", TaskID);
                                int exists = (int)await checkCmd.ExecuteScalarAsync();
                                if (exists == 0)
                                {
                                    return new JsonResult(new { success = false, message = "Task does not exist." });
                                }
                            }

                            // Delete related labels
                            string deleteLabelsQuery = "DELETE FROM dbo.Task_Labels WHERE TaskID = @TaskID";
                            using (SqlCommand cmd = new SqlCommand(deleteLabelsQuery, myCon, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TaskID", TaskID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Delete related attachments
                            string deleteAttachmentsQuery = "DELETE FROM dbo.Attachments WHERE TaskID = @TaskID";
                            using (SqlCommand cmd = new SqlCommand(deleteAttachmentsQuery, myCon, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TaskID", TaskID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Remove parent references from sub-tasks
                            string updateChildTasksQuery = "UPDATE dbo.Tasks SET ParentTaskID = NULL WHERE ParentTaskID = @TaskID";
                            using (SqlCommand cmd = new SqlCommand(updateChildTasksQuery, myCon, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TaskID", TaskID);
                                await cmd.ExecuteNonQueryAsync();
                            }


                            // Delete the task
                            string deleteTaskQuery = "DELETE FROM dbo.Tasks WHERE TaskID = @TaskID";
                            using (SqlCommand cmd = new SqlCommand(deleteTaskQuery, myCon, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TaskID", TaskID);
                                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                                if (rowsAffected > 0)
                                {
                                    transaction.Commit();
                                    return new JsonResult(new { success = true, message = "Task deleted successfully!" });
                                }
                                else
                                {
                                    transaction.Rollback();
                                    return new JsonResult(new { success = false, message = "Task could not be deleted." });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return new JsonResult(new { success = false, message = $"Error deleting task: {ex.Message}" });
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
                if (updateData == null || TaskID <= 0)
                    return BadRequest("Dữ liệu không hợp lệ");

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Lấy dữ liệu gốc từ DB
                    string selectQuery = "SELECT * FROM dbo.Tasks WHERE TaskID = @TaskID";
                    Tasks existingTask = null;

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, myCon))
                    {
                        selectCmd.Parameters.AddWithValue("@TaskID", TaskID);
                        using (SqlDataReader reader = await selectCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                existingTask = new Tasks
                                {
                                    TaskID = (int)reader["TaskID"],
                                    Title = reader["Title"]?.ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    ProjectID = (int)reader["ProjectID"],
                                    AssignedTo = reader["AssignedTo"]?.ToString(),
                                    StatusID = (int)reader["StatusID"],
                                    ParentTaskID = reader["ParentTaskID"] == DBNull.Value ? null : (int?)reader["ParentTaskID"],
                                    ProjectIssueTypeID = (int)reader["ProjectIssueTypeID"],
                                    DueDate = (DateTime)reader["DueDate"]
                                };
                            }
                            else
                            {
                                return new JsonResult(new { success = false, message = "Task không tồn tại." });
                            }
                        }
                    }

                    // Gộp dữ liệu cập nhật
                    existingTask.Title = updateData.Title ?? existingTask.Title;
                    existingTask.Description = updateData.Description ?? existingTask.Description;

                    if (updateData.ProjectID != 0)
                        existingTask.ProjectID = updateData.ProjectID;

                    existingTask.AssignedTo = updateData.AssignedTo ?? existingTask.AssignedTo;

                    // ✅ LUÔN cập nhật StatusID nếu khác null
                    if (updateData.StatusID != 0 || updateData.StatusID == 0)
                        existingTask.StatusID = updateData.StatusID;

                    if (updateData.ParentTaskID.HasValue)
                        existingTask.ParentTaskID = updateData.ParentTaskID;

                    if (updateData.ProjectIssueTypeID != 0)
                        existingTask.ProjectIssueTypeID = updateData.ProjectIssueTypeID;

                    if (updateData.DueDate != default(DateTime))
                        existingTask.DueDate = updateData.DueDate;

                    // Cập nhật vào DB
                    string updateQuery = @"
         UPDATE dbo.Tasks SET 
             Title = @Title,
             Description = @Description,
             ProjectID = @ProjectID,
             AssignedTo = @AssignedTo,
             StatusID = @StatusID,
             ParentTaskID = @ParentTaskID,
             ProjectIssueTypeID = @ProjectIssueTypeID,
             DueDate = @DueDate
         WHERE TaskID = @TaskID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@TaskID", existingTask.TaskID);
                        updateCmd.Parameters.AddWithValue("@Title", (object)existingTask.Title ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Description", (object)existingTask.Description ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ProjectID", existingTask.ProjectID);
                        updateCmd.Parameters.AddWithValue("@AssignedTo", (object)existingTask.AssignedTo ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@StatusID", existingTask.StatusID);
                        updateCmd.Parameters.AddWithValue("@ParentTaskID", (object)existingTask.ParentTaskID ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ProjectIssueTypeID", existingTask.ProjectIssueTypeID);
                        updateCmd.Parameters.AddWithValue("@DueDate", existingTask.DueDate);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new
                            {
                                success = true,
                                message = "Cập nhật thành công",
                                updatedTask = existingTask
                            });
                        }
                        else
                        {
                            return new JsonResult(new
                            {
                                success = false,
                                message = "Không có gì thay đổi"
                            });
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
        [Route("GetTaskById")]
        public JsonResult GetTaskById(string TaskID)
        {
            string query = "SELECT * FROM dbo.Tasks WHERE TaskID = @TaskID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@TaskID", TaskID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, task = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "StatusID không tồn tại!" });
            }
        }

        [HttpGet]
        [Route("GetTasksByProjectId")]
        public JsonResult GetTasksByProjectId(int projectId)
        {
            string query = @"
        SELECT * FROM dbo.Tasks
        WHERE ProjectID = @ProjectID
    ";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", projectId);

                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
                myCon.Close();
            }

            return new JsonResult(table);
        }

        [HttpGet]
        [Route("GetTasksByProjectAndUser")]
        public JsonResult GetTasksByProjectAndUser(int projectId, string userId)
        {
            string query = @"
        SELECT * FROM dbo.Tasks
        WHERE ProjectID = @ProjectID AND AssignedTo = @UserID
    ";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", projectId);
                    myCommand.Parameters.AddWithValue("@UserID", userId);
                    //ExecuteReader() dùng khi câu truy vấn là SELECT và trả về nhiều dòng dữ liệu.
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
                myCon.Close();
            }

            return new JsonResult(table);
        }


    }
}
