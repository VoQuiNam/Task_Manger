using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Data;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/roles")]
    public class RolesController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public RolesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetRoles")]
        public JsonResult GetRoles()
        {
            string query = "select * from dbo.Roles";
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
        [Route("AddRoles")]
        public async Task<IActionResult> AddRoles([FromForm] Roles obj)
        {

            try
            {
                string query = @"INSERT INTO dbo.Roles (RoleName) 
                     VALUES (@RoleName)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@RoleName", obj.RoleName);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm thành công!" });
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
        [Route("DeleteRoles")]
        public JsonResult DeleteRoles(int id)
        {
            if (id <= 0) // Kiểm tra nếu id rỗng hoặc không hợp lệ
            {
                return new JsonResult(new { success = false, message = "Vui lòng cung cấp RoleID hợp lệ." });
            }

            string queryCheck = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
            string queryDelete = "DELETE FROM dbo.Roles WHERE RoleID = @RoleID";

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                // Kiểm tra RoleID có tồn tại không
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@RoleID", id);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                    }
                }

                // Nếu tồn tại, tiến hành xóa
                using (SqlCommand deleteCommand = new SqlCommand(queryDelete, myCon))
                {
                    deleteCommand.Parameters.AddWithValue("@RoleID", id);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            return new JsonResult(new { success = true, message = "Xóa thành công!" });
        }

        [HttpPut]
        [Route("UpdateRoles")]
        public async Task<IActionResult> UpdateRoles([FromQuery] int RoleID, [FromForm] Roles? obj)
        {
            try
            {
                obj.RoleID = RoleID;
                // Kiểm tra dữ liệu đầu vào:
                // RoleID được lấy từ query phải > 0, và obj không được null, RoleName phải có giá trị
                if (RoleID <= 0 || obj == null || string.IsNullOrWhiteSpace(obj.RoleName))
                {
                    return new JsonResult(new { success = false, message = "RoleID hoặc RoleName không hợp lệ." });
                }

                // Truy vấn kiểm tra RoleID có tồn tại trong bảng Roles không
                string queryCheck = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                // Truy vấn cập nhật RoleName theo RoleID
                string queryUpdate = "UPDATE dbo.Roles SET RoleName = @RoleName WHERE RoleID = @RoleID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của RoleID
                    using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                    {
                        checkCommand.Parameters.AddWithValue("@RoleID", RoleID);
                        int count = (int)await checkCommand.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                        }
                    }

                    // Cập nhật RoleName
                    using (SqlCommand updateCommand = new SqlCommand(queryUpdate, myCon))
                    {
                        updateCommand.Parameters.AddWithValue("@RoleID", RoleID);
                        updateCommand.Parameters.AddWithValue("@RoleName", obj.RoleName);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật RoleName thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không có dữ liệu nào được cập nhật." });
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
