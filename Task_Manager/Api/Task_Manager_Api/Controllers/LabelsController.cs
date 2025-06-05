using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/labels")]
    public class LabelsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public LabelsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetLabels")]
        public JsonResult GetLabels()
        {
            string query = "select * from dbo.Labels";
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
        [Route("AddLabels")]
        public async Task<IActionResult> AddLabels([FromBody] Labels obj)
        {
            try
            {
                string insertQuery = @"
        INSERT INTO dbo.Labels 
            (Name, IsActive, CreatedBy, CreatedAt, UpdatedAt) 
        VALUES 
            (@Name, @IsActive, @CreatedBy, GETDATE(), NULL);
        SELECT SCOPE_IDENTITY();";  // ✅ Trả về ID vừa tạo

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@Name", obj.Name ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@IsActive", obj.IsActive);
                        insertCmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);

                        object result = await insertCmd.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int newLabelId))
                        {
                            var createdLabel = new
                            {
                                LabelID = newLabelId,
                                Name = obj.Name,
                                IsActive = obj.IsActive,
                                CreatedBy = obj.CreatedBy
                            };

                            return new JsonResult(new
                            {
                                success = true,
                                message = "Thêm label thành công!",
                                label = createdLabel  // ✅ Trả về object label
                            });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm label." });
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
        [Route("DeleteLabels")]
        public async Task<IActionResult> DeleteLabels([FromQuery] int LabelID)
        {
            try
            {
                if (LabelID <= 0)
                {
                    return new JsonResult(new { success = false, message = "LabelID không hợp lệ." });
                }

                // Kiểm tra xem Label có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Labels WHERE LabelID = @LabelID";
                string deleteQuery = "DELETE FROM dbo.Labels WHERE LabelID = @LabelID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của Label
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@LabelID", LabelID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Label không tồn tại." });
                        }
                    }

                    // Xóa Label nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@LabelID", LabelID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa Label thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa Label." });
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
        [Route("UpdateLabels")]
        public async Task<IActionResult> UpdateLabels([FromQuery] int LabelID, [FromBody] Labels updateData)
        {
            try
            {
                updateData.LabelID = LabelID;
                if (updateData == null || updateData.LabelID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkLabelQuery = "SELECT COUNT(*) FROM dbo.Labels WHERE LabelID = @LabelID";
                string updateQuery = @"
            UPDATE dbo.Labels
            SET Name = @Name,
                IsActive = @IsActive,
                UpdatedAt = GETDATE()
            WHERE LabelID = @LabelID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Label có tồn tại không
                    using (SqlCommand checkCmd = new SqlCommand(checkLabelQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@LabelID", updateData.LabelID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Label không tồn tại." });
                        }
                    }

                    // Cập nhật thông tin Label
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@LabelID", updateData.LabelID);
                        updateCmd.Parameters.AddWithValue("@Name", updateData.Name ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@IsActive", updateData.IsActive);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật Label thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật Label." });
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
