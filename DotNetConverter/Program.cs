using DotNetConverter.Data;
using DotNetConverter.Data.Repositories;
using DotNetConverter.Services;
using DotNetConverter.Services.Queues;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IVideoQueue, VideoQueue>();
builder.Services.AddHostedService<QueuedVideoService>();
builder.Services.AddHostedService<TimedCleanupService>();
builder.Services.AddDbContextFactory<ConverterDbContext>(ops =>
{
    ops.UseNpgsql(builder.Configuration.GetSection("Database:ConnectionString").Value ?? throw new ArgumentException());
});
builder.Services.AddTransient(typeof(IRepo<>), typeof(Repo<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }

// app.UseForwardedHeaders(new ForwardedHeadersOptions
// {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
// });
app.UseAuthentication();

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();