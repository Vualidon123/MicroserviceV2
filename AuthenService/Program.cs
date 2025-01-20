using AuthenService.Request;
using AuthenService.Service;
using JWT_Authen.Data;

var builder = WebApplication.CreateBuilder(args);
var ValidIssuer = builder.Configuration.GetSection("ValidIssuer").Get<string>();
var ValidAudience = builder.Configuration.GetSection("ValidAudience").Get<string>();
// Add services to the container.
var configuration = builder.Configuration;
string? connectionString = configuration.GetConnectionString("MongoDbConnection");
string? databaseName = configuration["DatabaseName"];
builder.Services.Configure<TokenRequest>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton(sp =>
{
    var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString")
        ?? "localhost:6379";
    return new RedisService(redisConnection);
});
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
    {
        throw new InvalidOperationException("Connection string or database name is not configured properly.");
    }
    return new MongoDbContext(connectionString, databaseName);
});
var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Create an instance of the ApplicationDbContext

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
