using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/projects")]
    public class ProjectsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public ProjectsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetProjects")]
        public JsonResult GetProjects()
        {
            string query = "select * from dbo.Projects";
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
                query += " AND ProjectID <> @ExcludeId"; // Giả sử bảng có cột ID (khóa chính)
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
        [Route("AddProjects")]
        public async Task<IActionResult> AddProjects([FromBody] Projects obj)
        {
            try
            {
                // Kiểm tra CreatedBy có hợp lệ không


                // Kiểm tra người tạo có tồn tại trong bảng Users
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @UserID";
                string insertQuery = @"
            INSERT INTO dbo.Projects 
                (Name, Description, CreatedBy, CreatedAt) 
            VALUES 
                (@Name, @Description, @CreatedBy, GETDATE())";

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
                        insertCmd.Parameters.AddWithValue("@Description", obj.Description ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm project thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm project." });
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
        [Route("DeleteProjects")]
        public async Task<IActionResult> DeleteProjects([FromQuery] int ProjectID)
        {
            try
            {
                if (ProjectID <= 0)
                {
                    return new JsonResult(new { success = false, message = "ProjectID không hợp lệ." });
                }

                // Kiểm tra xem Label có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string deleteQuery = "DELETE FROM dbo.Projects WHERE ProjectID = @ProjectID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của Label
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Project không tồn tại." });
                        }
                    }

                    // Xóa Label nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@ProjectID", ProjectID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa Project thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa Project." });
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
        [Route("UpdateProjects")]
        public async Task<IActionResult> UpdateProjects([FromQuery] int ProjectID, [FromBody] Projects updateData)
        {
            try
            {
                updateData.ProjectID = ProjectID;
                if (updateData == null || updateData.ProjectID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkLabelQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string updateQuery = @"
            UPDATE dbo.Projects
            SET Name = @Name,
                Description = @Description
            WHERE ProjectID = @ProjectID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Label có tồn tại không
                    using (SqlCommand checkCmd = new SqlCommand(checkLabelQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Project không tồn tại." });
                        }
                    }

                    // Cập nhật thông tin Label
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        updateCmd.Parameters.AddWithValue("@Name", updateData.Name ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Description", updateData.Description ?? (object)DBNull.Value);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật Project thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật Project." });
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
