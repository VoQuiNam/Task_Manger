using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/rolemodules")]
    public class RoleModuleController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public RoleModuleController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetRoleModule")]
        public JsonResult GetRoleModule()
        {
            string query = "select * from dbo.Role_Modules";
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
        [Route("CheckRoleAndModuleExists")]
        public JsonResult CheckRoleAndModuleExists(int roleid, int moduleid, int? excludeId = null)
        {
            string query = "SELECT COUNT(1) FROM dbo.Role_Modules WHERE RoleID = @RoleID AND ModuleID = @ModuleID";

            if (excludeId.HasValue)
            {
                query += " AND RoleModuleID <> @ExcludeId"; // Giả sử bảng có cột ID (khóa chính)
            }

            bool exists = false;
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@RoleID", roleid);
                    myCommand.Parameters.AddWithValue("@ModuleID", moduleid);

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
        [Route("AddRoleModule")]
        public async Task<IActionResult> AddRoleModule([FromBody] RoleModules obj)

        {
            try
            {
                int RoleID = obj.RoleID;
                int ModuleID = obj.ModuleID;


                // Kiểm tra RoleID và ModuleID hợp lệ
                if (RoleID <= 0 || ModuleID <= 0)
                {
                    return new JsonResult(new { success = false, message = "RoleID hoặc ModuleID không hợp lệ." });
                }

                // Kiểm tra RoleID và ModuleID có tồn tại không
                string checkRoleQuery = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                string checkModuleQuery = "SELECT COUNT(*) FROM dbo.Modules WHERE ModuleID = @ModuleID";
                string insertQuery = @"INSERT INTO Role_modules (RoleID, ModuleID, CanView, CanCreate, CanEdit, CanDelete) 
                               VALUES (@RoleID, @ModuleID, @CanView, @CanCreate, @CanEdit, @CanDelete)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của RoleID
                    using (SqlCommand checkRoleCmd = new SqlCommand(checkRoleQuery, myCon))
                    {
                        checkRoleCmd.Parameters.AddWithValue("@RoleID", RoleID);
                        int roleExists = (int)await checkRoleCmd.ExecuteScalarAsync();
                        if (roleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                        }
                    }

                    // Kiểm tra sự tồn tại của ModuleID
                    using (SqlCommand checkModuleCmd = new SqlCommand(checkModuleQuery, myCon))
                    {
                        checkModuleCmd.Parameters.AddWithValue("@ModuleID", ModuleID);
                        int moduleExists = (int)await checkModuleCmd.ExecuteScalarAsync();
                        if (moduleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ModuleID không tồn tại." });
                        }
                    }

                    // Chèn RoleModule vào database
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@RoleID", RoleID);
                        insertCmd.Parameters.AddWithValue("@ModuleID", ModuleID);
                        insertCmd.Parameters.AddWithValue("@CanView", obj.CanView);
                        insertCmd.Parameters.AddWithValue("@CanCreate", obj.CanCreate);
                        insertCmd.Parameters.AddWithValue("@CanEdit", obj.CanEdit);
                        insertCmd.Parameters.AddWithValue("@CanDelete", obj.CanDelete);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm RoleModule thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể thêm RoleModule." });
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
        [Route("DeleteRoleModule")]
        public async Task<IActionResult> DeleteRoleModule([FromQuery] int RoleModuleID)
        {
            try
            {
                if (RoleModuleID <= 0)
                {
                    return new JsonResult(new { success = false, message = "RoleModuleID không hợp lệ." });
                }

                // Kiểm tra xem RoleModule có tồn tại không
                string checkQuery = "SELECT COUNT(*) FROM dbo.Role_Modules WHERE RoleModuleID = @RoleModuleID";
                string deleteQuery = "DELETE FROM dbo.Role_Modules WHERE RoleModuleID = @RoleModuleID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của RoleModule
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@RoleModuleID", RoleModuleID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleModule không tồn tại." });
                        }
                    }

                    // Xóa RoleModule nếu tồn tại
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@RoleModuleID", RoleModuleID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Xóa RoleModule thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể xóa RoleModule." });
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
        [Route("UpdateModuleRole")]
        public async Task<IActionResult> UpdateModuleRole([FromBody] RoleModules updateData)
        {
            try
            {
                if (updateData == null || updateData.RoleModuleID <= 0 || updateData.RoleID <= 0 || updateData.ModuleID <= 0)
                {
                    return new JsonResult(new { success = false, message = "Thông tin không hợp lệ." });
                }

                string checkRoleModuleQuery = "SELECT COUNT(*) FROM dbo.Role_Modules WHERE RoleModuleID = @RoleModuleID";
                string checkRoleQuery = "SELECT COUNT(*) FROM dbo.Roles WHERE RoleID = @RoleID";
                string checkModuleQuery = "SELECT COUNT(*) FROM dbo.Modules WHERE ModuleID = @ModuleID";

                string updateQuery = @"UPDATE dbo.Role_Modules
                              SET RoleID = @RoleID, ModuleID = @ModuleID, 
                                  CanView = @CanView, CanCreate = @CanCreate, 
                                  CanEdit = @CanEdit, CanDelete = @CanDelete
                              WHERE RoleModuleID = @RoleModuleID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlCommand checkRoleModuleCmd = new SqlCommand(checkRoleModuleQuery, myCon))
                    {
                        checkRoleModuleCmd.Parameters.AddWithValue("@RoleModuleID", updateData.RoleModuleID);
                        int roleModuleExists = (int)await checkRoleModuleCmd.ExecuteScalarAsync();
                        if (roleModuleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleModule không tồn tại." });
                        }
                    }

                    using (SqlCommand checkRoleCmd = new SqlCommand(checkRoleQuery, myCon))
                    {
                        checkRoleCmd.Parameters.AddWithValue("@RoleID", updateData.RoleID);
                        int roleExists = (int)await checkRoleCmd.ExecuteScalarAsync();
                        if (roleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "RoleID không tồn tại." });
                        }
                    }

                    using (SqlCommand checkModuleCmd = new SqlCommand(checkModuleQuery, myCon))
                    {
                        checkModuleCmd.Parameters.AddWithValue("@ModuleID", updateData.ModuleID);
                        int moduleExists = (int)await checkModuleCmd.ExecuteScalarAsync();
                        if (moduleExists == 0)
                        {
                            return new JsonResult(new { success = false, message = "ModuleID không tồn tại." });
                        }
                    }

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@RoleID", updateData.RoleID);
                        updateCmd.Parameters.AddWithValue("@ModuleID", updateData.ModuleID);
                        updateCmd.Parameters.AddWithValue("@CanView", updateData.CanView);
                        updateCmd.Parameters.AddWithValue("@CanCreate", updateData.CanCreate);
                        updateCmd.Parameters.AddWithValue("@CanEdit", updateData.CanEdit);
                        updateCmd.Parameters.AddWithValue("@CanDelete", updateData.CanDelete);
                        updateCmd.Parameters.AddWithValue("@RoleModuleID", updateData.RoleModuleID);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật RoleModule thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Không thể cập nhật RoleModule." });
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
        [Route("GetRoleModuleById")]
        public JsonResult GetRoleModuleById(string RoleModuleID)
        {
            string query = "SELECT * FROM dbo.Role_modules WHERE RoleModuleID = @RoleModuleID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@RoleModuleID", RoleModuleID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, rolemodule = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Role không tồn tại!" });
            }
        }




    }
}
