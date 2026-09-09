using HikvisionBackend1.Data;
using HikvisionBackend1.Interface;
using HikvisionBackend1.Interfaces;
using HikvisionBackend1.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5018");

builder.Services.AddControllers();

builder.Services.AddHttpClient<GroqPlateService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuth, Authservice>();
builder.Services.AddScoped<ICamera, Cameraservice>();
builder.Services.AddScoped<IHikvisionBarrierService, HikvisionBarrierService>();

builder.Services.AddScoped<VehicleDetectionService>();
builder.Services.AddScoped<VehicleMatchingService>();
builder.Services.AddScoped<VehicleTriggerService>();
builder.Services.AddScoped<RegisteredVehicleService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(120);
        }));

// Hikvision SDK
builder.Services.AddSingleton<HikvisionSdkService>();

// Multi-camera manager
builder.Services.AddSingleton<HikvisionCameraManager>();

// Automatic camera connection/reconnection
builder.Services.AddHostedService<HikvisionCameraConnectionWorker>();
//builder.Services.AddHostedService<VehicleImageWatcher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// ============================================================
// HIKVISION SDK + MULTI CAMERA INITIALIZATION
// ============================================================

var sdk = app.Services
    .GetRequiredService<HikvisionSdkService>();

sdk.TestSdk();


// ============================================================
// HTTP PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactPolicy");

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();