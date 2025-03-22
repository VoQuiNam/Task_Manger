using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/modules")]
    public class ModulesController : Controller
    {

        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public ModulesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetModules")]
        public JsonResult GetModules()
        {
            string query = "select * from dbo.Modules";
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
        [Route("AddModule")]
        public async Task<IActionResult> AddModule([FromForm] Modules obj)
        {
            try
            {
                string query = @"INSERT INTO dbo.Modules 
                        (ModuleName, ParentID, Controller, Action, IsAction, Link, Icon, OrderNumber, IsActive, CreatedAt, UpdatedAt) 
                        VALUES 
                        (@ModuleName, @ParentID, @Controller, @Action, @IsAction, @Link, @Icon, @OrderNumber, @IsActive, GETDATE(), NULL)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@ModuleName", obj.ModuleName);
                        myCommand.Parameters.AddWithValue("@ParentID", (object)obj.ParentID ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@Controller", (object)obj.Controller ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@Action", (object)obj.Action ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@IsAction", obj.IsAction);
                        myCommand.Parameters.AddWithValue("@Link", (object)obj.Link ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@Icon", (object)obj.Icon ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@OrderNumber", (object)obj.OrderNumber ?? DBNull.Value);
                        myCommand.Parameters.AddWithValue("@IsActive", obj.IsActive);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm module thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Thêm module thất bại" });
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
        [Route("DeleteModule")]
        public JsonResult DeleteModule(int id)
        {
            if (id <= 0) // Kiểm tra nếu id không hợp lệ
            {
                return new JsonResult(new { success = false, message = "Vui lòng cung cấp ModuleID hợp lệ." });
            }

            string queryCheck = "SELECT COUNT(*) FROM dbo.Modules WHERE ModuleID = @ModuleID";
            string queryDelete = "DELETE FROM dbo.Modules WHERE ModuleID = @ModuleID";

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                // Kiểm tra ModuleID có tồn tại không
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@ModuleID", id);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        return new JsonResult(new { success = false, message = "ModuleID không tồn tại." });
                    }
                }

                // Nếu tồn tại, tiến hành xóa
                using (SqlCommand deleteCommand = new SqlCommand(queryDelete, myCon))
                {
                    deleteCommand.Parameters.AddWithValue("@ModuleID", id);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            return new JsonResult(new { success = true, message = "Xóa module thành công!" });
        }

        [HttpPut]
        [Route("UpdateModule")]
        public async Task<IActionResult> UpdateModule([FromQuery] int ModuleID, [FromForm] Modules? obj)
        {
            try
            {
                obj.ModuleID = ModuleID;

                // Kiểm tra dữ liệu đầu vào: ModuleID phải hợp lệ và ModuleName không được rỗng
                if (ModuleID <= 0 || obj == null || string.IsNullOrWhiteSpace(obj.ModuleName))
                {
                    return new JsonResult(new { success = false, message = "ModuleID hoặc ModuleName không hợp lệ." });
                }

                // Truy vấn kiểm tra ModuleID có tồn tại không
                string queryCheck = "SELECT COUNT(*) FROM dbo.Modules WHERE ModuleID = @ModuleID";

                // Truy vấn cập nhật Module
                string queryUpdate = @"
            UPDATE dbo.Modules 
            SET ModuleName = @ModuleName, 
                ParentID = @ParentID, 
                Controller = @Controller, 
                Action = @Action, 
                IsAction = @IsAction, 
                Link = @Link, 
                Icon = @Icon, 
                OrderNumber = @OrderNumber, 
                IsActive = @IsActive, 
                UpdatedAt = GETDATE()
            WHERE ModuleID = @ModuleID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của ModuleID
                    using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                    {
                        checkCommand.Parameters.AddWithValue("@ModuleID", ModuleID);
                        int count = (int)await checkCommand.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            return new JsonResult(new { success = false, message = "ModuleID không tồn tại." });
                        }
                    }

                    // Cập nhật thông tin Module
                    using (SqlCommand updateCommand = new SqlCommand(queryUpdate, myCon))
                    {
                        updateCommand.Parameters.AddWithValue("@ModuleID", ModuleID);
                        updateCommand.Parameters.AddWithValue("@ModuleName", obj.ModuleName);
                        updateCommand.Parameters.AddWithValue("@ParentID", (object?)obj.ParentID ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@Controller", (object?)obj.Controller ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@Action", (object?)obj.Action ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@IsAction", obj.IsAction);
                        updateCommand.Parameters.AddWithValue("@Link", (object?)obj.Link ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@Icon", (object?)obj.Icon ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@OrderNumber", (object?)obj.OrderNumber ?? DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@IsActive", obj.IsActive);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật module thành công!" });
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
