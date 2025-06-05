using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/issue_types")]
    public class Issue_TypesController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public Issue_TypesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetIssueTypes")]
        public JsonResult GetIssueTypes()
        {
            string query = "select * from dbo.Issue_Types";
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
        [Route("CheckIssueTypesExists")]
        public JsonResult CheckIssueTypesExists(string name, int? excludeId = null)
        {
            string query = "SELECT COUNT(1) FROM dbo.Issue_Types WHERE Name = @Name";
            if (excludeId.HasValue)
            {
                query += " AND TypeID <> @ExcludeId";
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
        [Route("AddIssueType")]
        public async Task<IActionResult> AddIssueType([FromForm] Issue_Types obj)
        {
            try
            {
                string query = @"INSERT INTO dbo.Issue_Types (Name) 
                         OUTPUT INSERTED.TypeID
                         VALUES (@Name)"; // thêm OUTPUT để lấy ID vừa insert

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@Name", obj.Name);

                        // Lấy TypeID sau khi insert
                        var insertedTypeId = (int?)await myCommand.ExecuteScalarAsync();

                        if (insertedTypeId.HasValue)
                        {
                            return new JsonResult(new
                            {
                                success = true,
                                message = "Thêm thành công!",
                                typeID = insertedTypeId.Value // Trả về luôn ID mới tạo
                            });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Thêm thất bại" });
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
        [Route("DeleteIssueTypes")]
        public JsonResult DeleteIssueTypes([FromQuery] int typeID)
        {
            if (typeID <= 0)
            {
                return new JsonResult(new { success = false, message = "Vui lòng cung cấp TypeID hợp lệ." });
            }

            string queryCheck = "SELECT COUNT(*) FROM dbo.Issue_Types WHERE TypeID = @TypeID";
            string queryDelete = "DELETE FROM dbo.Issue_Types WHERE TypeID = @TypeID";

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@TypeID", typeID);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        return new JsonResult(new { success = false, message = "TypeID không tồn tại." });
                    }
                }

                using (SqlCommand deleteCommand = new SqlCommand(queryDelete, myCon))
                {
                    deleteCommand.Parameters.AddWithValue("@TypeID", typeID);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            return new JsonResult(new { success = true, message = "Xóa loại issue thành công!" });
        }


        [HttpPut]
        [Route("UpdateIssueTypes")]
        public async Task<IActionResult> UpdateIssueTypes([FromBody] Issue_Types updateData)
        {
            try
            {
                if (updateData == null || updateData.TypeID <= 0 || string.IsNullOrWhiteSpace(updateData.Name))
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkQuery = "SELECT COUNT(*) FROM dbo.Issue_Types WHERE TypeID = @TypeID";
                string updateQuery = @"UPDATE dbo.Issue_Types
                               SET Name = @Name
                               WHERE TypeID = @TypeID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra tồn tại của TypeID
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TypeID", updateData.TypeID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "TypeID không tồn tại." });
                        }
                    }

                    // Cập nhật Name
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@Name", updateData.Name);
                        updateCmd.Parameters.AddWithValue("@TypeID", updateData.TypeID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật Issue Type thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật Issue Type." });
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
        [Route("GetIssueTypeById")]
        public JsonResult GetIssueTypeById(int typeID)
        {
            string query = "SELECT * FROM dbo.Issue_Types WHERE TypeID = @TypeID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@TypeID", typeID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, issueType = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Issue Type không tồn tại!" });
            }
        }

    }
}
