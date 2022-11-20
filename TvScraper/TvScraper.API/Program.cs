using Microsoft.Extensions.Logging;
using TvScraper.Database;
using TvScraper.Scraper;
using TvScraper.Scraper.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>();
builder.Services.AddSingleton<ITvMazeClient, TvMazeClient>();
builder.Services.AddScoped<IActorScraper, ActorScraper>();
builder.Services.AddScoped<IShowScraper, ShowScraper>();
builder.Services.AddSingleton<ShowScrapeService>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<ShowScrapeService>());
builder.Services.AddSingleton<ActorScrapeService>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<ActorScrapeService>());

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

app.Run();
