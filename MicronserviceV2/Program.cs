using JWT_Authen.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register ApplicationDbContext as a singleton
builder.Services.AddSingleton<ApplicationDbContext>(sp =>
{
    string connectionString = "mongodb://localhost:27017";
    string databaseName = "NewDatabaseName"; // Change this to your new database name
    return new ApplicationDbContext(connectionString, databaseName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Define your connection string and database name
string connectionString = "mongodb://localhost:27017";
string databaseName = "NewDatabaseName"; // Change this to your new database name

// Create an instance of the ApplicationDbContext
var dbContext = new ApplicationDbContext(connectionString, databaseName);

// Optionally, seed the database with initial data


app.Run();
