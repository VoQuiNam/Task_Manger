using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Task_Manager_Api.Models
{
    public class UserDropdownOperationFilter : IOperationFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserDropdownOperationFilter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Giả sử bạn áp dụng filter này cho endpoint "UpdateUser" (hoặc endpoint nào cần chọn User_ID)
            if (context.MethodInfo.Name == "UpdateUser")
            {
                // Gọi API lấy danh sách người dùng (User_ID và FullName)
                var client = _httpClientFactory.CreateClient();
                var response = client.GetStringAsync("http://localhost:5260/api/users/GetUsers").Result;
                if (string.IsNullOrWhiteSpace(response))
                {
                    Console.WriteLine("Không lấy được danh sách người dùng.");
                    return;
                }

                var users = JArray.Parse(response);
                Console.WriteLine("Danh sách người dùng: " + users.ToString());

                // Tạo danh sách giá trị thật: User_ID dưới dạng chuỗi
                var userEnum = users.Select(user => new OpenApiString(user["User_ID"].ToString()))
                                    .Cast<IOpenApiAny>()
                                    .ToList();

                // Tạo mảng chứa nhãn hiển thị: "FullName (User_ID)"
                var userEnumNames = new OpenApiArray();
                foreach (var user in users)
                {
                    string displayLabel = $"{user["FullName"]} ({user["User_ID"]})";
                    userEnumNames.Add(new OpenApiString(displayLabel));
                }

                // Tìm hoặc thêm tham số "User_ID"
                var param = operation.Parameters.FirstOrDefault(p => p.Name == "User_ID");
                if (param == null)
                {
                    param = new OpenApiParameter
                    {
                        Name = "User_ID",
                        In = ParameterLocation.Query,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            // Dùng string để hiển thị dropdown với enum
                            Type = "string",
                            Enum = userEnum
                        },
                        Description = "Chọn người dùng theo tên (sẽ gửi giá trị User_ID)"
                    };
                    operation.Parameters.Add(param);
                }
                else
                {
                    param.Schema.Type = "string";
                    param.Schema.Enum = userEnum;
                }
                // Gán extension x-enumNames để hiển thị nhãn (nếu Swagger UI hỗ trợ)
                param.Schema.Extensions["x-enumNames"] = userEnumNames;

                Console.WriteLine("User_ID được cập nhật thành dropdown.");
            }
        }
    }
}
