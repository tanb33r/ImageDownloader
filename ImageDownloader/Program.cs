using ImageDownloader.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IImageDownloadService, ImageDownloadService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<IImageDownloadService, ImageDownloadService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapControllers();

app.Run();
