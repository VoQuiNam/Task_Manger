using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Task_Manager_Api.Models;

namespace Task_Manager_Api.Controllers
{
    [Route("api/comments")]
    public class CommentsController : Controller
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public CommentsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env; // Initialize the environment
        }

        [HttpGet]
        [Route("GetComments")]
        public JsonResult GetComments()
        {
            string query = "select * from dbo.Comments";
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
        [Route("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] Comments obj)
        {
            try
            {
                // Kiểm tra cơ bản
                if (obj == null || obj.TaskID <= 0 || string.IsNullOrWhiteSpace(obj.UserID) || string.IsNullOrWhiteSpace(obj.Content))
                {
                    return new JsonResult(new { success = false, message = "Thông tin comment không hợp lệ." });
                }

                string query = @"
            INSERT INTO dbo.Comments (TaskID, UserID, Content, CreatedAt, ParentCommentID)
            VALUES (@TaskID, @UserID, @Content, GETDATE(), @ParentCommentID)";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@TaskID", obj.TaskID);
                        myCommand.Parameters.AddWithValue("@UserID", obj.UserID);
                        myCommand.Parameters.AddWithValue("@Content", obj.Content);

                        // 👇 xử lý thêm ParentCommentID
                        if (obj.ParentCommentID == 0)
                            myCommand.Parameters.AddWithValue("@ParentCommentID", DBNull.Value);
                        else
                            myCommand.Parameters.AddWithValue("@ParentCommentID", obj.ParentCommentID);

                        int rowsAffected = await myCommand.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Thêm comment thành công!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Thêm comment thất bại." });
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
        [Route("DeleteComment")]
        public JsonResult DeleteComment([FromQuery] int commentID)
        {
            if (commentID <= 0)
            {
                return new JsonResult(new { success = false, message = "Vui lòng cung cấp CommentID hợp lệ." });
            }

            string queryCheck = "SELECT COUNT(*) FROM dbo.Comments WHERE CommentID = @CommentID";

            // Sử dụng CTE để xóa đệ quy tất cả comment con
            string queryDelete = @"
        WITH CommentTree AS (
            SELECT CommentID FROM dbo.Comments WHERE CommentID = @CommentID
            UNION ALL
            SELECT c.CommentID
            FROM dbo.Comments c
            INNER JOIN CommentTree ct ON c.ParentCommentID = ct.CommentID
        )
        DELETE FROM dbo.Comments WHERE CommentID IN (SELECT CommentID FROM CommentTree);";

            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                // Kiểm tra xem comment tồn tại
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@CommentID", commentID);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        return new JsonResult(new { success = false, message = "CommentID không tồn tại." });
                    }
                }

                // Nếu tồn tại, xóa cả comment và các con của nó
                using (SqlCommand deleteCommand = new SqlCommand(queryDelete, myCon))
                {
                    deleteCommand.Parameters.AddWithValue("@CommentID", commentID);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            return new JsonResult(new { success = true, message = "Xóa comment và các phản hồi con thành công!" });
        }

        [HttpPut]
        [Route("UpdateComment")]
        public async Task<IActionResult> UpdateComment([FromQuery] int commentID, [FromBody] Comments? obj)
        {
            try
            {
                obj.CommentID = commentID;

                // Kiểm tra dữ liệu đầu vào: CommentID phải hợp lệ và Content không được rỗng
                if (commentID <= 0 || obj == null || string.IsNullOrWhiteSpace(obj.Content))
                {
                    return new JsonResult(new { success = false, message = "CommentID hoặc nội dung comment không hợp lệ." });
                }

                // Truy vấn kiểm tra CommentID có tồn tại không
                string queryCheck = "SELECT COUNT(*) FROM dbo.Comments WHERE CommentID = @CommentID";

                // Truy vấn cập nhật Comment
                string queryUpdate = @"
            UPDATE dbo.Comments
            SET Content = @Content,
                CreatedAt = GETDATE()
            WHERE CommentID = @CommentID";

                string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    await myCon.OpenAsync();

                    // Kiểm tra sự tồn tại của CommentID
                    using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                    {
                        checkCommand.Parameters.AddWithValue("@CommentID", commentID);
                        int count = (int)await checkCommand.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            return new JsonResult(new { success = false, message = "CommentID không tồn tại." });
                        }
                    }

                    // Cập nhật nội dung comment
                    using (SqlCommand updateCommand = new SqlCommand(queryUpdate, myCon))
                    {
                        updateCommand.Parameters.AddWithValue("@CommentID", commentID);
                        updateCommand.Parameters.AddWithValue("@Content", obj.Content);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Cập nhật comment thành công!" });
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


        [HttpGet]
        [Route("GetCommentById")]
        public JsonResult GetCommentById(int commentID)
        {
            string query = "SELECT * FROM dbo.Comments WHERE CommentID = @CommentID";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@CommentID", commentID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, comment = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Comment không tồn tại!" });
            }
        }

        [HttpGet]
        [Route("GetCommentsByTask")]
        public JsonResult GetCommentsByTask(int taskID)
        {
            string query = @"
        SELECT c.CommentID, c.TaskID, c.UserID, u.FullName, c.Content, c.CreatedAt, c.ParentCommentID
        FROM dbo.Comments c
        INNER JOIN dbo.Users u ON c.UserID = u.User_ID
        WHERE c.TaskID = @TaskID
        ORDER BY c.CreatedAt ASC";

            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("TaskManagement");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@TaskID", taskID);
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }
                myCon.Close();
            }

            if (table.Rows.Count > 0)
            {
                return new JsonResult(new { success = true, comments = table });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Không có comment nào cho task này." });
            }
        }


    }
}
