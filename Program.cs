using System.Net;

var builder = WebApplication.CreateBuilder(args);

// var certPath = "C:\\Certificates\\thesafetechsolutions.in-certificate.pfx";
// var certPassword = "8059235170";

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.Listen(IPAddress.Parse("136.243.73.35"), 5000);
// });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5000);
});

DotNetEnv.Env.Load();

builder.Services.AddSingleton<MT5Connection>();
builder.Services.AddSingleton<MT5AccountandData>();
builder.Services.AddSingleton<BackgroundWorkerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService(provider => provider.GetRequiredService<BackgroundWorkerService>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins(
            // "https://credit-setller-frontend.vercel.app"
            "http://localhost:5173"
             //  "http://136.243.73.35:5173"
             )
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetRequiredService<MT5Connection>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
