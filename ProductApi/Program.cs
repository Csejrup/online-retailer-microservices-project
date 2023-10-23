using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using MessageListener = ProductApi.Infrastructure.MessageListener;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string rabbitmqConnectionString = "host=rabbitmq";

builder.Services.AddDbContext<ProductApiContext>(opt => opt.UseInMemoryDatabase("ProductsDb"));
builder.Services.AddSingleton(RabbitHutch.CreateBus(rabbitmqConnectionString));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

builder.Services.AddSingleton<IConverter<Product, ProductDto>, ProductConverter>();
//builder.Services.AddSingleton<IMessagePublisher>(new MessagePublisher(rabbitmqConnectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<ProductApiContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}
var bus = app.Services.GetRequiredService<IBus>();

Task.Factory.StartNew(() => new MessageListener(app.Services, rabbitmqConnectionString, bus).StartAsync());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();