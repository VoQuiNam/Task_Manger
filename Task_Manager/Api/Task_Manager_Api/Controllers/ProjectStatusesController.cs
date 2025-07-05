using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/ProjectStatuses")]
    public class ProjectStatusesController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public ProjectStatusesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }


        [HttpGet]
        [Route("GetProjectStatuses")]
        public JsonResult GetProjectUsers()
        {
            string query = "select * from dbo.Project_Statuses";
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
        [Route("AddProjectStatus")]
        public async Task<IActionResult> AddProjectStatus([FromBody] Project_Statuses obj)
        {
            try
            {
                int projectID = obj.ProjectID;
                int statusID = obj.StatusID;

                if (projectID <= 0 || statusID <= 0)
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc StatusID không hợp lệ." });
                }

                string checkProjectQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string checkStatusQuery = "SELECT COUNT(*) FROM dbo.Task_Status WHERE StatusID = @StatusID";
                string insertQuery = @"INSERT INTO Project_Statuses (ProjectID, StatusID)
                               VALUES (@ProjectID, @StatusID)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra Project tồn tại
                    using (SqlCommand checkProjectCmd = new SqlCommand(checkProjectQuery, myCon))
                    {
                        checkProjectCmd.Parameters.AddWithValue("@ProjectID", projectID);
                        int projectExists = (int)await checkProjectCmd.ExecuteScalarAsync();
                        if (projectExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectID không tồn tại." });
                        }
                    }

                    // Kiểm tra Status tồn tại
                    using (SqlCommand checkStatusCmd = new SqlCommand(checkStatusQuery, myCon))
                    {
                        checkStatusCmd.Parameters.AddWithValue("@StatusID", statusID);
                        int statusExists = (int)await checkStatusCmd.ExecuteScalarAsync();
                        if (statusExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "StatusID không tồn tại." });
                        }
                    }

                    // Thêm vào Project_Statuses
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@ProjectID", projectID);
                        insertCmd.Parameters.AddWithValue("@StatusID", statusID);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm ProjectStatus thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm ProjectStatus." });
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
        [Route("DeleteProjectStatuses")]
        public async Task<IActionResult> DeleteProjectStatuses([FromQuery] int ProjectID, [FromQuery] int StatusID)
        {
            try
            {
                if (ProjectID <= 0 || StatusID <= 0)
                {
                    return new JsonResult(new { success = false, message = "ProjectID hoặc StatusID không hợp lệ." });
                }

                string checkQuery = "SELECT COUNT(*) FROM dbo.Project_Statuses WHERE ProjectID = @ProjectID AND StatusID = @StatusID";
                string deleteQuery = "DELETE FROM dbo.Project_Statuses WHERE ProjectID = @ProjectID AND StatusID = @StatusID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra xem dòng có tồn tại không
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        checkCmd.Parameters.AddWithValue("@StatusID", StatusID);

                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectStatus không tồn tại." });
                        }
                    }

                    // Thực hiện xóa nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@ProjectID", ProjectID);
                        deleteCmd.Parameters.AddWithValue("@StatusID", StatusID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa ProjectStatus thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa ProjectStatus." });
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
        [Route("UpdateProjectStatuses")]
        public async Task<IActionResult> UpdateProjectStatuses([FromBody] Project_Statuses updateData)
        {
            try
            {
                if (updateData == null || updateData.ProjectID <= 0 || updateData.StatusID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkProjectQuery = "SELECT COUNT(*) FROM dbo.Projects WHERE ProjectID = @ProjectID";
                string checkStatusQuery = "SELECT COUNT(*) FROM dbo.Task_Status WHERE StatusID = @StatusID";
                string checkExistQuery = "SELECT COUNT(*) FROM dbo.Project_Statuses WHERE ProjectID = @ProjectID AND StatusID = @StatusID";

                string updateQuery = @"UPDATE dbo.Project_Statuses
                               SET StatusID = @StatusID
                               WHERE ProjectStatusID = @ProjectStatusID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra tồn tại của Project
                    using (SqlCommand checkProjectCmd = new SqlCommand(checkProjectQuery, myCon))
                    {
                        checkProjectCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        int projectExists = (int)await checkProjectCmd.ExecuteScalarAsync();
                        if (projectExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectID không tồn tại." });
                        }
                    }

                    // Kiểm tra tồn tại của Status
                    using (SqlCommand checkStatusCmd = new SqlCommand(checkStatusQuery, myCon))
                    {
                        checkStatusCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        int statusExists = (int)await checkStatusCmd.ExecuteScalarAsync();
                        if (statusExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "StatusID không tồn tại." });
                        }
                    }

                    // Kiểm tra bản ghi hiện tại có tồn tại không (tránh update không tồn tại)
                    using (SqlCommand checkExistCmd = new SqlCommand(checkExistQuery, myCon))
                    {
                        checkExistCmd.Parameters.AddWithValue("@ProjectID", updateData.ProjectID);
                        checkExistCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        int exists = (int)await checkExistCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ProjectStatuses không tồn tại." });
                        }
                    }

                    // Cập nhật StatusID
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@StatusID", updateData.StatusID);
                        updateCmd.Parameters.AddWithValue("@ProjectStatusID", updateData.ProjectStatusID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật ProjectStatus thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật ProjectStatus." });
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
        [Route("GetProjectStatusesByProjectId")]
        public JsonResult GetProjectStatusesByProjectId(int ProjectID)
        {
            string query = @"
        SELECT ps.ProjectStatusID, ps.ProjectID, ps.StatusID, ts.Name AS StatusName, ts.ColorCode
        FROM dbo.Project_Statuses ps
        JOIN dbo.Task_Status ts ON ps.StatusID = ts.StatusID
        WHERE ps.ProjectID = @ProjectID";

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
                return new JsonResult(new { success = true, projectStatus = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy status trong dự án!" });
            }
        }



    }
}
