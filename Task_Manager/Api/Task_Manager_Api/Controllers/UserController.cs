using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Task_Manager_Api.Models;
using BCrypt.Net;


namespace Task_Manager_Api.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public UserController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }
        [HttpGet]
        [Route("GetUsers")]
        public JsonResult GetUsers()
        {
            string query = "select * from dbo.Users";
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
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromForm] Users obj, [FromQuery] int RoleID)
        {
            try
            {
                // Gán RoleID từ query vào obj
                obj.RoleID = RoleID;

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(obj.FullName) ||
                    string.IsNullOrWhiteSpace(obj.Email) ||
                    string.IsNullOrWhiteSpace(obj.Password) ||
                    obj.RoleID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                // Kiểm tra RoleID có tồn tại không
                string checkRoleQuery = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                string insertQuery = @"INSERT INTO dbo.Users (FullName, Email, Password, RoleID, CreatedAt) 
                               VALUES (@FullName, @Email, @Password, @RoleID, GETDATE())";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra RoleID
                    using (SqlCommand checkRoleCmd = new SqlCommand(checkRoleQuery, myCon))
                    {
                        checkRoleCmd.Parameters.AddWithValue("@RoleID", obj.RoleID);
                        int roleExists = (int)await checkRoleCmd.ExecuteScalarAsync();

                        if (roleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                        }
                    }

                    // Hash mật khẩu trước khi lưu
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);

                    // Chèn User vào database
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@FullName", obj.FullName);
                        insertCmd.Parameters.AddWithValue("@Email", obj.Email);
                        insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        insertCmd.Parameters.AddWithValue("@RoleID", obj.RoleID);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm User thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm User." });
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
