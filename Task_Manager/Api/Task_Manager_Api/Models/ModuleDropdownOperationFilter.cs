using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Any;

public class ModuleDropdownOperationFilter : IOperationFilter
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ModuleDropdownOperationFilter(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == "AddRoleModule" || context.MethodInfo.Name == "UpdateRoleModule")
        {
            //Tạo HTTP client thông qua _httpClientFactory
            var client = _httpClientFactory.CreateClient();
            var response = client.GetStringAsync("http://localhost:5260/api/modules/GetModules").Result;

            if (string.IsNullOrWhiteSpace(response))
            {
                Console.WriteLine("Không lấy được danh sách modules.");
                return;
            }

            //Parse JSON thành một JArray chứa danh sách roles.
            var modules = JArray.Parse(response);
            Console.WriteLine("Danh sách modules: " + modules.ToString());

            var modulesEnum = modules.Select(module => new OpenApiString(module["ModuleID"].ToString()))
                                .Cast<IOpenApiAny>()
                                .ToList();

            // Kiểm tra nếu RoleID chưa có thì thêm vào danh sách tham số
            if (!operation.Parameters.Any(p => p.Name == "ModuleID"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "ModuleID",
                    In = ParameterLocation.Query,  // Lấy RoleID từ query
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = modulesEnum// Swagger hiển thị danh sách roles dưới dạng dropdown.
                    },
                    Required = true
                });

                Console.WriteLine("Đã thêm ModuleID vào danh sách tham số.");
            }

            // Cập nhật lại RoleID nếu đã tồn tại
            var moduleParam = operation.Parameters.FirstOrDefault(p => p.Name == "ModuleID");

            if (moduleParam != null)
            {
                moduleParam.Schema.Type = "string";
                moduleParam.Schema.Enum = modulesEnum;
                Console.WriteLine("RoleID được cập nhật thành dropdown.");
            }
            else
            {
                Console.WriteLine("Không tìm thấy tham số ModuleID.");
            }
        }
    }
}

