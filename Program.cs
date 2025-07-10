using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Azure.Messaging.ServiceBus;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Application.Services;
using VideoTranscoder.VideoTranscoder.Worker.Services;
using VideoTranscoder.VideoTranscoder.Infrastructure.Queues;
using VideoTranscoder.VideoTranscoder.Infrastructure.Storage;
using VideoTranscoder.VideoTranscoder.Infrastructure.Persistance;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region 🔧 Configuration Services

// 🔹 Azure Blob Options
builder.Services.Configure<AzureOptions>(
    configuration.GetSection("AzureOptions")
);

// 🔹 BlobServiceClient (for Azure Storage)
builder.Services.AddSingleton(provider =>
{
    var azureOptions = provider
        .GetRequiredService<IConfiguration>()
        .GetSection("AzureOptions")
        .Get<AzureOptions>();

    return new BlobServiceClient(
        $"DefaultEndpointsProtocol=https;AccountName={azureOptions!.AccountName};AccountKey={azureOptions.AccountKey};EndpointSuffix=core.windows.net"
    );
});

// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
// );
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
    )
);

#endregion

#region 🔐 JWT Authentication & Authorization

// 🔹 Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<JwtService>();

// 🔹 Video Services
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

// 🔹 JWT Authentication Setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
        ),
        RoleClaimType = ClaimTypes.Role
    };
});
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthorization();

#endregion

#region 🚀 Azure Service Bus & Background Worker

// 🔹 Register ServiceBusClient
builder.Services.AddSingleton(_ =>
    new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"])
);

// 🔹 Register ServiceBusProcessor using queue name
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<ServiceBusClient>();
    var queueName = configuration["AzureServiceBus:TranscodeQueueName"];
    return client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
});

// 🔹 Transcoding Worker & Services
builder.Services.AddScoped<FFmpegService>();
// builder.Services.AddHostedService<TranscodingWorker>();
builder.Services.AddScoped<ITranscodingService, TranscodingService>();
builder.Services.AddScoped<IMessageQueueService, AzureServiceBusPublisherService>();
builder.Services.AddScoped<ICloudStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IEncodingProfileRepository, EncodingProfileRepository>();
builder.Services.AddScoped<IThumbnailRepository, ThumbnailRepository>();
builder.Services.AddScoped<ITranscodingJobRepository, TranscodingJobRepository>();
builder.Services.AddScoped<IEncodingProfileService, EncodingProfileService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();




#endregion

#region 🌐 CORS, Controllers, and Misc

// 🔹 CORS Setup for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 🔹 MVC Controllers
builder.Services.AddControllers();

#endregion

var app = builder.Build();

#region 🏁 App Middleware Pipeline

// 🔹 Seed Initial DB Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataInitializer.SeedAsync(dbContext);
}

// 🔹 Middleware Setup
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Routing
app.MapControllers();
app.MapGet("/", () => "Hello World!");

#endregion

app.Run();
