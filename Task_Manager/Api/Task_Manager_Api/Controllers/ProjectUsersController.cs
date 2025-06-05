using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/ProjectUsers")]
    public class ProjectUsersController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public ProjectUsersController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetProjectUsers")]
        public JsonResult GetProjectUsers()
        {
            string query = "select * from dbo.Project_Users";
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
        [Route("GetProjectUsersByProjectId")]
        public JsonResult GetProjectUsersByProjectId(int projectId)
        {
            string query = "SELECT * FROM dbo.Project_Users WHERE ProjectID = @ProjectID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", projectId);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            return new JsonResult(table);
        }


        [HttpPost]
        [Route("AddProjectUser")]
        public async Task<IActionResult> AddProjectUser([FromBody] Project_Users obj)
        {
            try
            {
                int ProjectID = obj.ProjectID;
                string UserID = obj.UserID;  // UserID là định danh của người dùng
                string RoleInProject = obj.RoleInProject;  // Ví dụ: 'Owner', 'Editor', 'Viewer'

                // Kiểm tra ProjectID và UserID hợp lệ
                if (ProjectID <= 0 || string.IsNullOrWhiteSpace(UserID))
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc UserID không hợp lệ." });
                }

                // Kiểm tra ProjectID có tồn tại không
                string checkProjectQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @UserID";
                string insertQuery = @"INSERT INTO Project_Users (ProjectID, UserID, RoleInProject) 
                               VALUES (@ProjectID, @UserID, @RoleInProject)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của ProjectID
                    using (SqlCommand checkProjectCmd = new SqlCommand(checkProjectQuery, myCon))
                    {
                        checkProjectCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        int projectExists = (int)await checkProjectCmd.ExecuteScalarAsync();
                        if (projectExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectID không tồn tại." });
                        }
                    }

                    // Kiểm tra sự tồn tại của UserID
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, myCon))
                    {
                        checkUserCmd.Parameters.AddWithValue("@UserID", UserID);
                        int userExists = (int)await checkUserCmd.ExecuteScalarAsync();
                        if (userExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "UserID không tồn tại." });
                        }
                    }

                    // Chèn ProjectUser vào database
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        insertCmd.Parameters.AddWithValue("@UserID", UserID);
                        insertCmd.Parameters.AddWithValue("@RoleInProject", RoleInProject);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm ProjectUser thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm ProjectUser." });
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
        [Route("DeleteProjectUser")]
        public async Task<IActionResult> DeleteProjectUser([FromQuery] int ProjectID, [FromQuery] string UserID)
        {
            try
            {
                // Kiểm tra ProjectID và UserID hợp lệ
                if (ProjectID <= 0 || string.IsNullOrWhiteSpace(UserID))
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc UserID không hợp lệ." });
                }

                // Kiểm tra xem ProjectUser có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Project_Users WHERE ProjectID = @ProjectID AND UserID = @UserID";
                string deleteQuery = "DELETE FROM dbo.Project_Users WHERE ProjectID = @ProjectID AND UserID = @UserID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của ProjectUser
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        checkCmd.Parameters.AddWithValue("@UserID", UserID);

                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectUser không tồn tại." });
                        }
                    }

                    // Xóa ProjectUser nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        deleteCmd.Parameters.AddWithValue("@UserID", UserID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa ProjectUser thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa ProjectUser." });
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
        [Route("UpdateProjectUser")]
        public async Task<IActionResult> UpdateProjectUser([FromBody] Project_Users updateData)
        {
            try
            {
                if (updateData == null || updateData.ProjectID <= 0 || string.IsNullOrWhiteSpace(updateData.UserID) || string.IsNullOrWhiteSpace(updateData.RoleInProject))
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                // Kiểm tra sự tồn tại của ProjectID và UserID
                string checkProjectQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @UserID";
                string checkProjectUserQuery = "SELECT COUNT(*) FROM dbo.Project_Users WHERE ProjectID = @ProjectID AND UserID = @UserID";

                string updateQuery = @"UPDATE dbo.Project_Users
                              SET RoleInProject = @RoleInProject
                              WHERE ProjectID = @ProjectID AND UserID = @UserID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của ProjectID
                    using (SqlCommand checkProjectCmd = new SqlCommand(checkProjectQuery, myCon))
                    {
                        checkProjectCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        int projectExists = (int)await checkProjectCmd.ExecuteScalarAsync();
                        if (projectExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectID không tồn tại." });
                        }
                    }

                    // Kiểm tra sự tồn tại của UserID
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, myCon))
                    {
                        checkUserCmd.Parameters.AddWithValue("@UserID", updateData.UserID);
                        int userExists = (int)await checkUserCmd.ExecuteScalarAsync();
                        if (userExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "UserID không tồn tại." });
                        }
                    }

                    // Kiểm tra sự tồn tại của ProjectUser
                    using (SqlCommand checkProjectUserCmd = new SqlCommand(checkProjectUserQuery, myCon))
                    {
                        checkProjectUserCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        checkProjectUserCmd.Parameters.AddWithValue("@UserID", updateData.UserID);
                        int projectUserExists = (int)await checkProjectUserCmd.ExecuteScalarAsync();
                        if (projectUserExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectUser không tồn tại." });
                        }
                    }

                    // Cập nhật RoleInProject nếu tất cả kiểm tra thành công
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@RoleInProject", updateData.RoleInProject);
                        updateCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        updateCmd.Parameters.AddWithValue("@UserID", updateData.UserID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật ProjectUser thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật ProjectUser." });
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
        [Route("GetProjectUserById")]
        public JsonResult GetProjectUserById(int ProjectID, string UserID)
        {
            // Câu truy vấn SQL để lấy thông tin người dùng trong dự án
            string query = "SELECT * FROM dbo.Project_Users WHERE ProjectID = @ProjectID AND UserID = @UserID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Thêm tham số vào câu truy vấn SQL
                    myCommand.Parameters.AddWithValue("@ProjectID", ProjectID);
                    myCommand.Parameters.AddWithValue("@UserID", UserID);

                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            // Kiểm tra xem có dữ liệu không và trả về kết quả
            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, projectuser = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Dự án hoặc người dùng không tồn tại!" });
            }
        }

        [HttpGet]
        [Route("CheckUserExists")]
        public async Task<IActionResult> CheckUserExists(int projectId, string userId, string excludeUserId = null)
        {
            try
            {
                string query = @"SELECT COUNT(*) FROM dbo.Project_Users 
                         WHERE ProjectID = @ProjectID AND UserID = @UserID";

                if (!string.IsNullOrEmpty(excludeUserId))
                {
                    query += " AND UserID != @ExcludeUserID";
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, myCon))
                    {
                        cmd.Parameters.AddWithValue("@ProjectID", projectId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        if (!string.IsNullOrEmpty(excludeUserId))
                            cmd.Parameters.AddWithValue("@ExcludeUserID", excludeUserId);

                        int exists = (int)await cmd.ExecuteScalarAsync();

                        return Ok(new { exists = exists > 0 });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllUsersInProject")]
        public JsonResult GetAllUsersInProject(int projectId)
        {
            string query = @"
        SELECT 
            u.User_ID,
            u.FullName,
            u.Email,
            pu.RoleInProject
        FROM dbo.Project_Users pu
        INNER JOIN dbo.Users u ON pu.UserID = u.User_ID
        WHERE pu.ProjectID = @ProjectID";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", projectId);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            return new JsonResult(table);
        }



    }
}
