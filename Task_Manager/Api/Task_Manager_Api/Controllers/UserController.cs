using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Task_Manager_Api.Models;
using BCrypt.Net;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


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
                //Tạo một đối tượng SqlCommand để thực thi câu lệnh SQL.
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
        public async Task<IActionResult> AddUser([FromBody] Users obj, [FromQuery] int RoleID)
        {
            try
            {
                // Gán RoleID từ query vào obj
                obj.RoleID = RoleID;
                // Sinh mã ngẫu nhiên 6 ký tự cho User_ID
                obj.UserID = GenerateRandomUserID();

                if (string.IsNullOrWhiteSpace(obj.FullName) ||
                    string.IsNullOrWhiteSpace(obj.Email) ||
                    string.IsNullOrWhiteSpace(obj.Password))
                {
                    return new JsonResult(new { success = false, message = "Hãy điền đầy đủ thông tin." });
                }

                if (obj.FullName.Length < 4)
                {
                    return new JsonResult(new { success = false, message = "FullName phải ít nhất 4 ký tự." });
                }
                if (obj.Password.Length < 4)
                {
                    return new JsonResult(new { success = false, message = "Password phải ít nhất 4 ký tự." });
                }

                // Ràng buộc: Kiểm tra định dạng email
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(obj.Email, emailPattern))
                {
                    return new JsonResult(new { success = false, message = "Email không đúng định dạng." });
                }

                // Kiểm tra dữ liệu đầu vào
                if (obj.RoleID <= 0 ||
                    string.IsNullOrWhiteSpace(obj.UserID))
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                // Kiểm tra RoleID có tồn tại không
                string checkRoleQuery = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                string insertQuery = @"INSERT INTO dbo.Users (User_ID, FullName, Email, Password, RoleID, CreatedAt) 
                               VALUES (@User_ID, @FullName, @Email, @Password, @RoleID, GETDATE())";

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
                        insertCmd.Parameters.AddWithValue("@User_ID", obj.UserID);
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

        [HttpDelete]
        [Route("DeleteUser")]
        public JsonResult DeleteUser(int id)
        {
            if (id <= 0) // Kiểm tra nếu id rỗng hoặc không hợp lệ
            {
                return new JsonResult(new { success = false, message = "Vui lòng cung cấp id hợp lệ." });
            }

            string queryCheck = "SELECT COUNT(*) FROM dbo.Users WHERE id = @id";
            string queryDelete = "DELETE FROM dbo.Users WHERE id = @id";

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                // Kiểm tra RoleID có tồn tại không
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@id", id);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        return new JsonResult(new { success = false, message = "id không tồn tại." });
                    }
                }

                // Nếu tồn tại, tiến hành xóa
                using (SqlCommand deleteCommand = new SqlCommand(queryDelete, myCon))
                {
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            return new JsonResult(new { success = true, message = "Xóa thành công!" });
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(
     [FromForm] Users obj,
     [FromQuery] string User_ID,  // Mã user dạng VARCHAR(6)
     [FromQuery] int RoleID       // RoleID dạng int
 )
        {
            try
            {
                // Gán User_ID & RoleID vào obj
                obj.UserID = User_ID;
                obj.RoleID = RoleID;

                if (string.IsNullOrWhiteSpace(obj.FullName) ||
                   string.IsNullOrWhiteSpace(obj.Email) ||
                   string.IsNullOrWhiteSpace(obj.Password))
                {
                    return new JsonResult(new { success = false, message = "Hãy điền đầy đủ thông tin." });
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(obj.UserID) ||
                    obj.RoleID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                // Ràng buộc 1: FullName >= 4 ký tự
                if (obj.FullName.Length < 4)
                {
                    return new JsonResult(new { success = false, message = "FullName phải ít nhất 4 ký tự." });
                }

                // Ràng buộc 2: Password >= 4 ký tự
                if (obj.Password.Length < 4)
                {
                    return new JsonResult(new { success = false, message = "Password phải ít nhất 4 ký tự." });
                }

                // Ràng buộc 3: Email phải đúng định dạng
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(obj.Email, emailPattern))
                {
                    return new JsonResult(new { success = false, message = "Email không đúng định dạng." });
                }

                // Kiểm tra xem User_ID có tồn tại không
                string checkUserQuery = "SELECT COUNT(*) FROM dbo.Users WHERE User_ID = @User_ID";
                // Kiểm tra xem RoleID có tồn tại không
                string checkRoleQuery = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                // Lệnh cập nhật
                string updateQuery = @"UPDATE dbo.Users
                               SET FullName = @FullName, Email = @Email, Password = @Password, RoleID = @RoleID
                               WHERE User_ID = @User_ID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // 1) Kiểm tra User_ID
                    using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, myCon))
                    {
                        checkUserCmd.Parameters.AddWithValue("@User_ID", obj.UserID);
                        int userExists = (int)await checkUserCmd.ExecuteScalarAsync();
                        if (userExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "User không tồn tại." });
                        }
                    }

                    // 2) Kiểm tra RoleID
                    using (SqlCommand checkRoleCmd = new SqlCommand(checkRoleQuery, myCon))
                    {
                        checkRoleCmd.Parameters.AddWithValue("@RoleID", obj.RoleID);
                        int roleExists = (int)await checkRoleCmd.ExecuteScalarAsync();
                        if (roleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                        }
                    }

                    // 3) Băm mật khẩu
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);

                    // 4) Thực hiện UPDATE
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@FullName", obj.FullName);
                        updateCmd.Parameters.AddWithValue("@Email", obj.Email);
                        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        updateCmd.Parameters.AddWithValue("@RoleID", obj.RoleID);
                        updateCmd.Parameters.AddWithValue("@User_ID", obj.UserID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật User thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật User." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Email và Password không được để trống." });
            }

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                string query = "SELECT User_ID, FullName, Email, Password, RoleID FROM dbo.Users WHERE Email = @Email";

                using (SqlCommand cmd = new SqlCommand(query, myCon))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string hashedPassword = reader["Password"].ToString();
                        if (BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword))
                        {
                            var token = GenerateJwtToken(reader["User_ID"].ToString(), reader["RoleID"].ToString());

                            return Ok(new
                            {
                                success = true,
                                message = "Đăng nhập thành công!",
                                token,
                                roleId = reader["RoleID"].ToString()
                            });
                        }
                        else
                        {
                            return Unauthorized(new { success = false, message = "Tài khoản hoặc Mật khẩu không đúng." });
                        }
                    }
                }
            }

            return Unauthorized(new { success = false, message = "Email không tồn tại." });
        }

        private string GenerateJwtToken(string userId, string roleId)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new Exception("JWT Key is missing or too short in appsettings.json");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("role", roleId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        // Hàm sinh mã ngẫu nhiên 6 ký tự (chữ hoa và số)
        private string GenerateRandomUserID()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                               .Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
