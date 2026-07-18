using Kolia.Thumbnail.API;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.OpenApi;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<EnumDescriptionsTransformer>();
});

builder.Services.AddKoliaThumbnailApi(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(options =>
{
    options.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseGlobalException();

app.UseApiDocumentation();

app.UseHttpsRedirection();

// app.UseAuthentication();

// app.UseAuthorization();

app.MapControllers();

app.Run();
