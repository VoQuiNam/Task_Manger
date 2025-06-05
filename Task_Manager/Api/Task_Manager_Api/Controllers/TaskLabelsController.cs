using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/TaskLabels")]
    public class TaskLabelsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public TaskLabelsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetTaskLabels")]
        public JsonResult GetTaskLabels()
        {
            string query = "select * from dbo.Task_Labels";
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
        [Route("AddTaskLabel")]
        public async Task<IActionResult> AddTaskLabel([FromBody] Task_Labels obj)
        {
            try
            {
                int taskId = obj.TaskID;
                int labelId = obj.LabelID;

                if (taskId <= 0 || labelId <= 0)
                {
                    return new JsonResult(new { success = false, message = "TaskID hoặc LabelID không hợp lệ." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Task có tồn tại
                    string checkTaskQuery = "SELECT COUNT(*) FROM dbo.Tasks WHERE TaskID = @TaskID";
                    using (SqlCommand checkTaskCmd = new SqlCommand(checkTaskQuery, myCon))
                    {
                        checkTaskCmd.Parameters.AddWithValue("@TaskID", taskId);
                        int taskExists = (int)await checkTaskCmd.ExecuteScalarAsync();
                        if (taskExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "TaskID không tồn tại." });
                        }
                    }

                    // Kiểm tra Label có tồn tại
                    string checkLabelQuery = "SELECT COUNT(*) FROM dbo.Labels WHERE LabelID = @LabelID";
                    using (SqlCommand checkLabelCmd = new SqlCommand(checkLabelQuery, myCon))
                    {
                        checkLabelCmd.Parameters.AddWithValue("@LabelID", labelId);
                        int labelExists = (int)await checkLabelCmd.ExecuteScalarAsync();
                        if (labelExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "LabelID không tồn tại." });
                        }
                    }

                    // Thêm vào bảng Task_Labels
                    string insertQuery = "INSERT INTO Task_Labels (TaskID, LabelID) VALUES (@TaskID, @LabelID)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@TaskID", taskId);
                        insertCmd.Parameters.AddWithValue("@LabelID", labelId);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm label cho task thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm label cho task." });
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
        [Route("DeleteTaskLabel")]
        public async Task<IActionResult> DeleteTaskLabel([FromQuery] int TaskID, [FromQuery] int LabelID)
        {
            try
            {
                // Kiểm tra TaskID và LabelID hợp lệ
                if (TaskID <= 0 || LabelID <= 0)
                {
                    return new JsonResult(new { success = false, message = "TaskID hoặc LabelID không hợp lệ." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của Task_Labels
                    string checkQuery = "SELECT COUNT(*) FROM dbo.Task_Labels WHERE TaskID = @TaskID AND LabelID = @LabelID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TaskID", TaskID);
                        checkCmd.Parameters.AddWithValue("@LabelID", LabelID);

                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Label này không tồn tại trong task." });
                        }
                    }

                    // Xóa nếu tồn tại
                    string deleteQuery = "DELETE FROM dbo.Task_Labels WHERE TaskID = @TaskID AND LabelID = @LabelID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@TaskID", TaskID);
                        deleteCmd.Parameters.AddWithValue("@LabelID", LabelID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa label khỏi task thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa label khỏi task." });
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
        [Route("UpdateTaskLabel")]
        public async Task<IActionResult> UpdateTaskLabel([FromBody] UpdateTaskLabelDto updateData)
        {
            try
            {
                if (updateData == null || updateData.TaskID <= 0 || updateData.OldLabelID <= 0 || updateData.NewLabelID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra task-label hiện tại có tồn tại không
                    string checkQuery = @"SELECT COUNT(*) FROM dbo.Task_Labels 
                                  WHERE TaskID = @TaskID AND LabelID = @OldLabelID";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TaskID", updateData.TaskID);
                        checkCmd.Parameters.AddWithValue("@OldLabelID", updateData.OldLabelID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Label không tồn tại trong task." });
                        }
                    }

                    // Cập nhật label
                    string updateQuery = @"UPDATE dbo.Task_Labels 
                                   SET LabelID = @NewLabelID 
                                   WHERE TaskID = @TaskID AND LabelID = @OldLabelID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@TaskID", updateData.TaskID);
                        updateCmd.Parameters.AddWithValue("@OldLabelID", updateData.OldLabelID);
                        updateCmd.Parameters.AddWithValue("@NewLabelID", updateData.NewLabelID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật label thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật label." });
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
