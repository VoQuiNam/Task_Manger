using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Task_Manager_Api.Models
{
    public class LabelDropdownOperationFilter : IOperationFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LabelDropdownOperationFilter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Giả sử bạn áp dụng filter này cho endpoint "UpdateUser" (hoặc endpoint nào cần chọn User_ID)
            if (context.MethodInfo.Name == "UpdateLabels")
            {
                // Gọi API lấy danh sách người dùng (User_ID và FullName)
                var client = _httpClientFactory.CreateClient();
                var response = client.GetStringAsync("http://localhost:5260/api/labels/GetLabels").Result;
                if (string.IsNullOrWhiteSpace(response))
                {
                    Console.WriteLine("Không lấy được danh sách nhãn.");
                    return;
                }

                var lables = JArray.Parse(response);

                // Tạo danh sách giá trị thật: User_ID dưới dạng chuỗi
                var lablesEnum = lables.Select(label => new OpenApiString(label["LabelID"].ToString()))
                                    .Cast<IOpenApiAny>()
                                    .ToList();


                // Kiểm tra nếu RoleID chưa có thì thêm vào danh sách tham số
                if (!operation.Parameters.Any(p => p.Name == "LabelID"))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "LabelID",
                        In = ParameterLocation.Query,  // Lấy RoleID từ query
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Enum = lablesEnum// Swagger hiển thị danh sách roles dưới dạng dropdown.
                        },
                        Required = true
                    });

                    Console.WriteLine("Đã thêm LabelID vào danh sách tham số.");
                }

                // Cập nhật lại RoleID nếu đã tồn tại
                var labelParam = operation.Parameters.FirstOrDefault(p => p.Name == "LabelID");

                if (labelParam != null)
                {
                    labelParam.Schema.Type = "string";
                    labelParam.Schema.Enum = lablesEnum;
                    Console.WriteLine("LabelID được cập nhật thành dropdown.");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy tham số LabelID.");
                }

            }
        }
    }
}
