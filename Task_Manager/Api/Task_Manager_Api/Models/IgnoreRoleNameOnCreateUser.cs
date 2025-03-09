using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Task_Manager_Api.Models
{
    public class IgnoreRoleNameOnCreateUser : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.Name == "AddUser" || context.MethodInfo.Name == "UpdateUser")
            {
                if (operation.RequestBody?.Content?.ContainsKey("multipart/form-data") == true)
                {
                    var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

                    // Kiểm tra nếu Role.RoleName tồn tại, thì xóa nó khỏi schema properties
                    if (schema.Properties.ContainsKey("Role.RoleName"))
                    {
                        schema.Properties.Remove("Role.RoleName");
                    }
                }
            }
        }
    }

}
