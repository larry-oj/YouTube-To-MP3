using DotNetConverter.Services;
using DotNetConverter.Services.Queues;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IVideoQueue, VideoQueue>();
builder.Services.AddHostedService<QueuedVideoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }

app.UseHttpsRedirection();
app.MapControllers();

app.Run();