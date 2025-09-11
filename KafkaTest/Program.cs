using Kafka.Configuration.Producer;
using Kafka.Entities.Configuration.Kafka;
using Kafka.Producer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Settings BEGIN
builder.AddKafkaProducer();
// Settings END


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
