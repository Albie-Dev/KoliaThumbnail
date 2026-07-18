namespace Kolia.Thumbnail.API.Extensions
{
  public static class ApiDocumentationExtensions
  {
    public static IApplicationBuilder UseApiDocumentation(this IApplicationBuilder app, string apiTitle = "Kolia Thumbnail API")
    {
      app.Use(async (context, next) =>
      {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (path == "/scalar")
        {
          var scheme = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? context.Request.Scheme;
          var host = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? context.Request.Host.Value;
          var serverUrl = $"{scheme}://{host}";
          var specUrl = "/openapi/v1.json";

          context.Response.ContentType = "text/html";
          await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
  <head>
    <title>{apiTitle}</title>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  </head>
  <body>
    <script
      id=""api-reference""
      data-url=""{specUrl}""></script>
    <script>
      var apiRef = document.getElementById('api-reference');
      apiRef.dataset.configuration = JSON.stringify({{
        servers: [{{ url: '{serverUrl}' }}],
        showDeveloperTools: 'always',
        mcp: {{
          name: '{apiTitle}',
          url: '{serverUrl}/mcp',
          disabled: false
        }}
      }});
    </script>
    <script src=""https://cdn.jsdelivr.net/npm/@scalar/api-reference""></script>
  </body>
</html>");
          return;
        }

        if (path == "/redoc")
        {
          var specUrl = "/openapi/v1.json";

          context.Response.ContentType = "text/html";
          await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
  <head>
    <title>{apiTitle}</title>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <link
      href=""https://fonts.googleapis.com/css?family=Montserrat:300,400,700|Roboto:300,400,700""
      rel=""stylesheet"" />
  </head>
  <body>
    <redoc spec-url=""{specUrl}""></redoc>
    <script src=""https://cdn.redoc.ly/redoc/latest/bundles/redoc.standalone.js""></script>
  </body>
</html>");
          return;
        }

        await next();
      });

      return app;
    }
  }
}