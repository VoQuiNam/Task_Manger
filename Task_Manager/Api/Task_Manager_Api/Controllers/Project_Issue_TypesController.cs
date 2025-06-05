using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/Project_Issue_Types")]
    public class Project_Issue_TypesController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public Project_Issue_TypesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetProject_Issue_Types")]
        public JsonResult GetProject_Issue_Types()
        {
            string query = "select * from dbo.Project_Issue_Types";
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
        [Route("AddProjectIssueType")]
        public async Task<IActionResult> AddProjectIssueType([FromBody] Project_Issue_Types obj)
        {
            try
            {
                int projectID = obj.ProjectID;
                int typeID = obj.TypeID;

                if (projectID <= 0 || typeID <= 0)
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc TypeID không hợp lệ." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                string checkProjectQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string checkTypeQuery = "SELECT COUNT(*) FROM dbo.Issue_Types WHERE TypeID = @TypeID";
                string insertQuery = @"INSERT INTO dbo.Project_Issue_Types (ProjectID, TypeID) 
                               VALUES (@ProjectID, @TypeID)";

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra ProjectID tồn tại
                    using (SqlCommand checkProjectCmd = new SqlCommand(checkProjectQuery, myCon))
                    {
                        checkProjectCmd.Parameters.AddWithValue("@ProjectID", projectID);
                        int projectExists = (int)await checkProjectCmd.ExecuteScalarAsync();
                        if (projectExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectID không tồn tại." });
                        }
                    }

                    // Kiểm tra TypeID tồn tại
                    using (SqlCommand checkTypeCmd = new SqlCommand(checkTypeQuery, myCon))
                    {
                        checkTypeCmd.Parameters.AddWithValue("@TypeID", typeID);
                        int typeExists = (int)await checkTypeCmd.ExecuteScalarAsync();
                        if (typeExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "TypeID không tồn tại." });
                        }
                    }

                    // Thêm bản ghi vào bảng Project_Issue_Types
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@ProjectID", projectID);
                        insertCmd.Parameters.AddWithValue("@TypeID", typeID);
                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm IssueType vào Project thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm IssueType vào Project." });
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
        [Route("DeleteProjectIssueType")]
        public async Task<IActionResult> DeleteProjectIssueType([FromQuery] int ProjectID, [FromQuery] int TypeID)
        {
            try
            {
                // Kiểm tra ProjectID và TypeID hợp lệ
                if (ProjectID <= 0 || TypeID <= 0)
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc TypeID không hợp lệ." });
                }

                // Kiểm tra xem bản ghi ProjectIssueType có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Project_Issue_Types WHERE ProjectID = @ProjectID AND TypeID = @TypeID";
                string deleteQuery = "DELETE FROM dbo.Project_Issue_Types WHERE ProjectID = @ProjectID AND TypeID = @TypeID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của bản ghi
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        checkCmd.Parameters.AddWithValue("@TypeID", TypeID);

                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectIssueType không tồn tại." });
                        }
                    }

                    // Xóa bản ghi nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        deleteCmd.Parameters.AddWithValue("@TypeID", TypeID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa ProjectIssueType thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa ProjectIssueType." });
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
        [Route("UpdateProjectIssueType")]
        public async Task<IActionResult> UpdateProjectIssueType([FromBody] Project_Issue_Types updateData)
        {
            try
            {
                if (updateData == null || updateData.ProjectIssueTypeID <= 0 || updateData.TypeID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra xem bản ghi ProjectIssueType có tồn tại
                    string checkExistQuery = "SELECT COUNT(*) FROM dbo.Project_Issue_Types WHERE ProjectIssueTypeID = @ProjectIssueTypeID";
                    using (SqlCommand checkExistCmd = new SqlCommand(checkExistQuery, myCon))
                    {
                        checkExistCmd.Parameters.AddWithValue("@ProjectIssueTypeID", updateData.ProjectIssueTypeID);
                        int exists = (int)await checkExistCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectIssueType không tồn tại." });
                        }
                    }

                    // Kiểm tra TypeID mới có tồn tại không
                    string checkTypeQuery = "SELECT COUNT(*) FROM dbo.Issue_Types WHERE TypeID = @TypeID";
                    using (SqlCommand checkTypeCmd = new SqlCommand(checkTypeQuery, myCon))
                    {
                        checkTypeCmd.Parameters.AddWithValue("@TypeID", updateData.TypeID);
                        int typeExists = (int)await checkTypeCmd.ExecuteScalarAsync();
                        if (typeExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "TypeID không tồn tại." });
                        }
                    }

                    // Cập nhật TypeID mới cho bản ghi
                    string updateQuery = @"
                UPDATE dbo.Project_Issue_Types
                SET TypeID = @NewTypeID
                WHERE ProjectIssueTypeID = @ProjectIssueTypeID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@NewTypeID", updateData.TypeID);
                        updateCmd.Parameters.AddWithValue("@ProjectIssueTypeID", updateData.ProjectIssueTypeID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật ProjectIssueType thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật ProjectIssueType." });
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
        [Route("GetProjectIssueTypeById")]
        public JsonResult GetProjectIssueTypeById(int ProjectIssueTypeID)
        {
            string query = "SELECT * FROM dbo.Project_Issue_Types WHERE ProjectIssueTypeID = @ProjectIssueTypeID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectIssueTypeID", ProjectIssueTypeID);

                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, projectIssueType = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "ProjectIssueType không tồn tại!" });
            }
        }

        [HttpGet]
        [Route("GetProject_Issue_TypesByProjectId")]
        public JsonResult GetProject_Issue_TypesByProjectId(int projectId)
        {
            string query = @"
        SELECT pit.ProjectIssueTypeID, pit.ProjectID, pit.TypeID, it.Name
        FROM dbo.Project_Issue_Types pit
        JOIN dbo.Issue_Types it ON pit.TypeID = it.TypeID
        WHERE pit.ProjectID = @ProjectID";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@ProjectID", projectId);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }




    }
}
