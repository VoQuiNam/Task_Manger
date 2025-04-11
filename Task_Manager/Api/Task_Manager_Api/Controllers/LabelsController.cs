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

        [HttpGet]
        [Route("CheckUserExists")]
        public JsonResult CheckUserExists(string userid, int? excludeId = null)
        {
            string query = "SELECT COUNT(1) FROM dbo.Labels WHERE CreatedBy = @CreatedBy";

            if (excludeId.HasValue)
            {
                query += " AND LabelID <> @ExcludeId"; // Giả sử bảng có cột ID (khóa chính)
            }

            bool exists = false;
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@CreatedBy", userid);

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
        [Route("AddLabels")]
        public async Task<IActionResult> AddLabels([FromBody] Labels obj)
        {
            try
            {
                // Kiểm tra CreatedBy có hợp lệ không
            

                // Kiểm tra người tạo có tồn tại trong bảng Users
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @UserID";
                string insertQuery = @"
            INSERT INTO dbo.Labels 
                (Name, ColorCode, Description, IsActive, CreatedBy, CreatedAt, UpdatedAt) 
            VALUES 
                (@Name, @ColorCode, @Description, @IsActive, @CreatedBy, GETDATE(), NULL)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra CreatedBy tồn tại
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, myCon))
                    {
                        checkUserCmd.Parameters.AddWithValue("@UserID", obj.CreatedBy); // ✅ Đúng

                        int userExists = (int)await checkUserCmd.ExecuteScalarAsync();
                        if (userExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Người tạo không tồn tại." });
                        }
                    }

                    // Insert Label mới
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@Name", obj.Name ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ColorCode", obj.ColorCode ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Description", obj.Description ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@IsActive", obj.IsActive);
                        insertCmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm label thành công!" });
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
                ColorCode = @ColorCode,
                Description = @Description,
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
                        updateCmd.Parameters.AddWithValue("@ColorCode", updateData.ColorCode ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Description", updateData.Description ?? (object)DBNull.Value);
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
