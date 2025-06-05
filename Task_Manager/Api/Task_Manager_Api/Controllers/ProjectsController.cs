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
        [Route("CheckProjectExists")]
        public JsonResult CheckProjectExists(string name, int? excludeId = null)
        {
            string query = "SELECT COUNT(1) FROM dbo.Projects WHERE Name = @Name";

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
        [Route("AddProjects")]
        public async Task<IActionResult> AddProjects([FromBody] Projects obj)
        {
            try
            {
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @UserID";

                string insertProjectQuery = @"
            INSERT INTO dbo.Projects (Name, Description, CreatedBy, CreatedAt)
            OUTPUT INSERTED.ProjectID
            VALUES (@Name, @Description, @CreatedBy, GETDATE())";

                string insertProjectUserQuery = @"
            INSERT INTO dbo.Project_Users (ProjectID, UserID, RoleInProject)
            VALUES (@ProjectID, @UserID, 'Administrator')";

                string insertDefaultIssueTypesQuery = @"
            INSERT INTO dbo.Project_Issue_Types (ProjectID, TypeID)
            VALUES (@ProjectID, @BugTypeID), (@ProjectID, @TaskTypeID)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // 1. Kiểm tra CreatedBy có tồn tại
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, myCon))
                    {
                        checkUserCmd.Parameters.AddWithValue("@UserID", obj.CreatedBy);
                        int userExists = (int)await checkUserCmd.ExecuteScalarAsync();
                        if (userExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "Người tạo không tồn tại." });
                        }
                    }

                    // 2. Thêm project và lấy ProjectID mới tạo
                    int newProjectId;
                    using (SqlCommand insertCmd = new SqlCommand(insertProjectQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@Name", obj.Name ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Description", obj.Description ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@CreatedBy", obj.CreatedBy);

                        object result = await insertCmd.ExecuteScalarAsync();
                        newProjectId = Convert.ToInt32(result);
                    }

                    // 3. Thêm CreatedBy vào Project_Users
                    using (SqlCommand addUserCmd = new SqlCommand(insertProjectUserQuery, myCon))
                    {
                        addUserCmd.Parameters.AddWithValue("@ProjectID", newProjectId);
                        addUserCmd.Parameters.AddWithValue("@UserID", obj.CreatedBy);
                        await addUserCmd.ExecuteNonQueryAsync();
                    }

                    // 4. Thêm mặc định Bug (TypeID = 1) và Task (TypeID = 2) vào Project_Issue_Types
                    using (SqlCommand addDefaultIssueTypesCmd = new SqlCommand(insertDefaultIssueTypesQuery, myCon))
                    {
                        addDefaultIssueTypesCmd.Parameters.AddWithValue("@ProjectID", newProjectId);
                        addDefaultIssueTypesCmd.Parameters.AddWithValue("@BugTypeID", 1);  // đảm bảo ID này đúng
                        addDefaultIssueTypesCmd.Parameters.AddWithValue("@TaskTypeID", 2); // đảm bảo ID này đúng
                        await addDefaultIssueTypesCmd.ExecuteNonQueryAsync();
                    }

                    return new JsonResult(new { success = true, message = "Thêm project thành công!", projectID = newProjectId });
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

        [HttpGet]
        [Route("GetProjectsById")]
        public JsonResult GetProjectsById(string ProjectID)
        {
            string query = "SELECT * FROM dbo.Projects WHERE ProjectID = @ProjectID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", ProjectID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, project = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "StatusID không tồn tại!" });
            }
        }

        [HttpGet]
        [Route("GetProjectsByUserId")]
        public JsonResult GetProjectsByUserId(string userId)
        {
            string query = @"
        SELECT p.*
        FROM dbo.Projects p
        INNER JOIN dbo.Project_Users pu ON p.ProjectID = pu.ProjectID
        WHERE pu.UserID = @UserID";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserID", userId);
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
