using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Any;

public class RoleDropdownOperationFilter : IOperationFilter
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RoleDropdownOperationFilter(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == "AddUser" || context.MethodInfo.Name == "UpdateUser" || context.MethodInfo.Name == "UpdateRoles")
        {
            //Tạo HTTP client thông qua _httpClientFactory
            var client = _httpClientFactory.CreateClient();
            var response = client.GetStringAsync("http://localhost:5260/api/roles/GetRoles").Result;

            if (string.IsNullOrWhiteSpace(response))
            {
                Console.WriteLine("Không lấy được danh sách roles.");
                return;
            }

            //Parse JSON thành một JArray chứa danh sách roles.
            var roles = JArray.Parse(response);
            Console.WriteLine("Danh sách roles: " + roles.ToString());

            var roleEnum = roles.Select(role => new OpenApiString(role["RoleID"].ToString()))
                                .Cast<IOpenApiAny>()
                                .ToList();

            // Kiểm tra nếu RoleID chưa có thì thêm vào danh sách tham số
            if (!operation.Parameters.Any(p => p.Name == "RoleID"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "RoleID",
                    In = ParameterLocation.Query,  // Lấy RoleID từ query
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = roleEnum// Swagger hiển thị danh sách roles dưới dạng dropdown.
                    },
                    Required = true
                });

                Console.WriteLine("Đã thêm RoleID vào danh sách tham số.");
            }

            // Cập nhật lại RoleID nếu đã tồn tại
            var roleParam = operation.Parameters.FirstOrDefault(p => p.Name == "RoleID");

            if (roleParam != null)
            {
                roleParam.Schema.Type = "string";
                roleParam.Schema.Enum = roleEnum;
                Console.WriteLine("RoleID được cập nhật thành dropdown.");
            }
            else
            {
                Console.WriteLine("Không tìm thấy tham số RoleID.");
            }
        }
    }
} 

