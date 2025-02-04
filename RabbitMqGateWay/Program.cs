using EmpService.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddSingleton<RabbitMqProducerService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    string connectionString = "mongodb://localhost:27017";
    string databaseName = "RabbitMQDb"; // Change this to your new database naa ame
    return new MongoDbContext(connectionString, databaseName);
});
builder.Services.AddSingleton<RabitIRepository<Email>, Repository<Email>>();
builder.Services.AddSingleton<RabitIRepository<Notification>, Repository<Notification>>();
builder.Services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();
var app = builder.Build();
string connectionString = "mongodb://localhost:27017";
string databaseName = "RabbitMQDb"; // Change this to your new database name

// Create an instance of the ApplicationDbContext
var dbContext = new MongoDbContext(connectionString, databaseName);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}   

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
