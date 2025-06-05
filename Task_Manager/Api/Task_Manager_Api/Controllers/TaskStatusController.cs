using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/taskstatus")]
    public class TaskStatusController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public TaskStatusController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetTaskStatus")]
        public JsonResult GetTaskStatus()
        {
            string query = "select * from dbo.Task_Status";
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

        [HttpGet]
        [Route("CheckTaskStatusExists")]
        public JsonResult CheckTaskStatusExists(string name, int? excludeId = null)
        {
            string query = "SELECT COUNT(1) FROM dbo.Task_Status WHERE Name = @Name";

            if (excludeId.HasValue)
            {
                query += " AND StatusID <> @ExcludeId"; // Giả sử bảng có cột ID (khóa chính)
            }

            bool exists = false;
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Name", name);

                    if (excludeId.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                    }

                    int count = (int)myCommand.ExecuteScalar();
                    exists = count > 0;
                }
                myCon.Close();
            }

            return new JsonResult(new { exists });
        }

        [HttpPost]
        [Route("AddTaskStatus")]
        public async Task<IActionResult> AddTaskStatus([FromBody] Task_Status obj)
        {
            try
            {
                // Kiểm tra CreatedBy có hợp lệ không


                // Kiểm tra người tạo có tồn tại trong bảng Users
                string CheckTaskStatusExists = "SELECT COUNT(*) FROM dbo.Task_Status WHERE StatusID = @StatusID";
                string insertQuery = @"
            INSERT INTO dbo.Task_Status 
                (Name, ColorCode) 
            VALUES 
                (@Name, @ColorCode)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra CreatedBy tồn tại

                    // Insert Label mới
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@Name", obj.Name ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ColorCode", obj.ColorCode ?? (object)DBNull.Value);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm Task Status thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm Task Status." });
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
        [Route("DeleteTaskStatus")]
        public async Task<IActionResult> DeleteTaskStatus([FromQuery] int TaskStatusID)
        {
            try
            {
                if (TaskStatusID <= 0)
                {
                    return new JsonResult(new { success = false, message = "TaskStatusID không hợp lệ." });
                }

                // Kiểm tra xem Label có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Task_Status WHERE StatusID = @StatusID";
                string deleteQuery = "DELETE FROM dbo.Task_Status WHERE StatusID = @StatusID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của Label
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@StatusID", TaskStatusID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Task Status không tồn tại." });
                        }
                    }

                    // Xóa Label nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@StatusID", TaskStatusID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa Status thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa Status." });
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
        [Route("UpdateTaskStatus")]
        public async Task<IActionResult> UpdateTaskStatus([FromQuery] int TaskStatusID, [FromBody] Task_Status updateData)
        {
            try
            {
                updateData.StatusID = TaskStatusID;
                if (updateData == null || updateData.StatusID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkLabelQuery = "SELECT COUNT(*) FROM dbo.Task_Status WHERE StatusID = @StatusID";
                string updateQuery = @"
            UPDATE dbo.Task_Status
            SET Name = @Name,
                ColorCode = @ColorCode
            WHERE StatusID = @StatusID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Label có tồn tại không
                    using (SqlCommand checkCmd = new SqlCommand(checkLabelQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Status không tồn tại." });
                        }
                    }

                    // Cập nhật thông tin Label
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        updateCmd.Parameters.AddWithValue("@Name", updateData.Name ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ColorCode", updateData.ColorCode ?? (object)DBNull.Value);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật Status thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật Status." });
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
        [Route("GetTaskStatusById")]
        public JsonResult GetTaskStatusById(string StatusID)
        {
            string query = "SELECT * FROM dbo.Task_Status WHERE StatusID = @StatusID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@StatusID", StatusID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, status = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "StatusID không tồn tại!" });
            }
        }
    }
}
