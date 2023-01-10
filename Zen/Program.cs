using Microsoft.AspNetCore.SignalR;

const string ALLOWED_ORIGINS_POLICY = "ZenOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: ALLOWED_ORIGINS_POLICY,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:4200",
                                              "http://localhost:7080")
                                                .AllowAnyHeader()
                                                .AllowAnyMethod();
                      });
});
builder.Services.AddSingleton<IJwtManager, JwtManager>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/custom")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors(ALLOWED_ORIGINS_POLICY);
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Sample JWT Bearer authentication");
app.MapGet("/doaction", [Authorize(Roles = "Super")] () => new { message = "Protected action succeeded" });
app.MapGet("/token", (IJwtManager jwtManager) => new { token = jwtManager.GetToken() });

app.MapGet("/send", (IHubContext<CustomHub> customHub) =>
{
    customHub.Clients.All.SendAsync("client_function_name", new Data(100, "Dummy Data"));
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CustomHub>("/custom");
    endpoints.MapHub<GroupHub>("/groups");
});

app.Run();
