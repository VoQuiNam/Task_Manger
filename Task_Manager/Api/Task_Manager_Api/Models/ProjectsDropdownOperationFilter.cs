using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Task_Manager_Api.Models
{
    public class ProjectsDropdownOperationFilter : IOperationFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProjectsDropdownOperationFilter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Giả sử bạn áp dụng filter này cho endpoint "UpdateUser" (hoặc endpoint nào cần chọn User_ID)
            if (context.MethodInfo.Name == "AddProjectUser" && context.MethodInfo.Name == "UpdateProjects")
            {
                // Gọi API lấy danh sách người dùng (User_ID và FullName)
                var client = _httpClientFactory.CreateClient();
                var response = client.GetStringAsync("http://localhost:5260/api/projects/GetProjects").Result;
                if (string.IsNullOrWhiteSpace(response))
                {
                    Console.WriteLine("Không lấy được danh sách dự án.");
                    return;
                }

                var projects = JArray.Parse(response);

                // Tạo danh sách giá trị thật: User_ID dưới dạng chuỗi
                var projectsEnum = projects.Select(project => new OpenApiString(project["ProjectID"].ToString()))
                                    .Cast<IOpenApiAny>()
                                    .ToList();


                // Kiểm tra nếu RoleID chưa có thì thêm vào danh sách tham số
                if (!operation.Parameters.Any(p => p.Name == "ProjectID"))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "ProjectID",
                        In = ParameterLocation.Query,  // Lấy RoleID từ query
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Enum = projectsEnum// Swagger hiển thị danh sách roles dưới dạng dropdown.
                        },
                        Required = true
                    });

                    Console.WriteLine("Đã thêm ProjectID vào danh sách tham số.");
                }

                // Cập nhật lại RoleID nếu đã tồn tại
                var projectParam = operation.Parameters.FirstOrDefault(p => p.Name == "ProjectID");

                if (projectParam != null)
                {
                    projectParam.Schema.Type = "string";
                    projectParam.Schema.Enum = projectsEnum;
                    Console.WriteLine("ProjectID được cập nhật thành dropdown.");
                }
                else
                {
                    Console.WriteLine("Không tìm thấy tham số ProjectID.");
                }

            }
        }
    }
}
