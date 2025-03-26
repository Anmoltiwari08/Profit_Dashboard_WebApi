var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddControllers();

builder.Services.AddSingleton<MT5Connection>();
builder.Services.AddSingleton<MT5AccountandData>();
builder.Services.AddSingleton<BackgroundWorkerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService(provider => provider.GetRequiredService<BackgroundWorkerService>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins(
                        "http://localhost:5173"
             )
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetRequiredService<MT5Connection>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
