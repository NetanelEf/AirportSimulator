using LogicModels;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Server;
using Server.Data;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AirportContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Singleton
    );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(x => x
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(origin => true)
           .AllowCredentials());

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AirportContext>();
    ctx.Database.EnsureDeleted();
    ctx.Database.EnsureCreated();
}

var logic = new SimulatorLogic();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();  // enable EndpointRoutingMiddleware

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<FlightHub>("/flighthub");
});

app.MapControllers();

app.Run();