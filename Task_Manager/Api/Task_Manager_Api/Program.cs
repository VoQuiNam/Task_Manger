using Newtonsoft.Json.Serialization;
using Task_Manager_Api.Models;


//Tạo một ứng dụng Web API sử dụng WebApplication.CreateBuilder(args).
var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ controller, cho phép ứng dụng xử lý các yêu cầu HTTP bằng các controller.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//Cấu hình Swagger để tạo tài liệu API.
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<IgnoreRoleNameOnCreateUser>();
    c.OperationFilter<RoleDropdownOperationFilter>();
    c.OperationFilter<UserDropdownOperationFilter>();
});

//Đăng ký HttpClient để gửi yêu cầu HTTP đến các dịch vụ bên ngoài.
builder.Services.AddHttpClient();


//Nếu một đối tượng có quan hệ vòng lặp, Newtonsoft.Json sẽ bỏ qua phần lặp thay vì gây lỗi
builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling
= Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

var app = builder.Build();

//Cấu hình CORS cho phép mọi origin, header được gửi đến API.
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyOrigin());

// Nếu ứng dụng chạy ở môi trường phát triển (Development):
/*Hiển thị trang lỗi chi tiết khi có lỗi.
Kích hoạt Swagger để xem tài liệu API.*/
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

//Bật chức năng Authorization, giúp kiểm soát quyền truy cập vào API.
app.UseAuthorization();

//Ánh xạ các controller để xử lý các yêu cầu HTTP.
app.MapControllers();

app.Run();
