using SmartFileUploadService.Services;
using SmartFileUploadService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // ��� ������������ API ����� Swagger UI

// ������������ ���� ��������� �������
builder.Services.AddScoped<IVirusScanner, VirusScanService>();
builder.Services.AddSingleton<FileStorageService>(); // Singleton, �.�. �� ����� ���������, ����� �������

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();