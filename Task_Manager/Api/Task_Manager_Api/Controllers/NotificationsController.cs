using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/notifications")]
    public class NotificationsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public NotificationsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetNotifications")]
        public JsonResult GetNotifications()
        {
            string query = "select * from dbo.Notifications";
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
        [Route("AddNotification")]
        public async Task<IActionResult> AddNotification([FromBody] Notifications obj)
        {
            try
            {
                string insertQuery = @"
        INSERT INTO dbo.Notifications (UserID, Message, CreatedAt)
        VALUES (@UserID, @Message, GETDATE())";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, myCon))
                    {
                        insertCmd.Parameters.AddWithValue("@UserID", obj.UserID ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Message", obj.Message ?? (object)DBNull.Value);

                        int rowsAffected = await insertCmd.ExecuteNonQueryAsync();

                        return new JsonResult(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Thêm thông báo thành công!" : "Không thể thêm thông báo."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification([FromQuery] int NotificationID, [FromBody] Notifications obj)
        {
            try
            {
                string checkQuery = "SELECT COUNT(*) FROM dbo.Notifications WHERE NotificationID = @NotificationID";
                string updateQuery = @"
        UPDATE dbo.Notifications 
        SET UserID = @UserID, 
            Message = @Message
        WHERE NotificationID = @NotificationID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra tồn tại
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, myCon))
                    {
                        checkCmd.Parameters.AddWithValue("@NotificationID", NotificationID);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists == 0)
                            return new JsonResult(new { success = false, message = "Thông báo không tồn tại." });
                    }

                    // Cập nhật
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, myCon))
                    {
                        updateCmd.Parameters.AddWithValue("@NotificationID", NotificationID);
                        updateCmd.Parameters.AddWithValue("@UserID", obj.UserID ?? (object)DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Message", obj.Message ?? (object)DBNull.Value);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        return new JsonResult(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Cập nhật thông báo thành công!" : "Không thể cập nhật."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery] int NotificationID)
        {
            try
            {
                string deleteQuery = "DELETE FROM dbo.Notifications WHERE NotificationID = @NotificationID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, myCon))
                    {
                        deleteCmd.Parameters.AddWithValue("@NotificationID", NotificationID);

                        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                        return new JsonResult(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Xoá thông báo thành công!" : "Thông báo không tồn tại hoặc không thể xoá."
                        });
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
