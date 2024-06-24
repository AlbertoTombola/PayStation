using PayStationSW;
using System.Security.Policy;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayStationSW.DataBase;
using PayStationSW.DataBase.Seeding;
using PayStationSW.RESTAPI;
using PayStationSW.Devices;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Imposta l'indirizzo IP e la porta su cui ascoltare
        builder.WebHost.UseUrls("http://10.10.20.111:6000");
        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure Entity Framework Core to use SQLite
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString)
        );

        // Registra il DeviceService, StationManager, Station e StationManagerWS nel container dei servizi
        builder.Services.AddScoped<DeviceService>();
        builder.Services.AddScoped<StationManager>();
        builder.Services.AddScoped<PayStation>();
        builder.Services.AddScoped<StationManagerWS>();

        // Configurazione del logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        // Configura WebSockets
        app.UseWebSockets();

        // Database Migrations: Check if the database exists and apply any pending migrations to ensure the database schema is up to date.
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            // Utilizzo del Singleton per ottenere l'istanza di Station
            var paymentStation = await StationManager.GetStationAsync(dbContext);
            // Nota: `paymentStation.Enable();` non è più necessario se è già gestito in `GetStationAsync`

            // Avvia l'invio di messaggi periodici
            var stationManagerWS = services.GetRequiredService<StationManagerWS>();
            stationManagerWS.StartPeriodicMessages();
        }

        // Configura il pipeline di richieste HTTP.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.Map("/ws", async context =>
            {
                await WebSocketHandler.Handle(context);
            });
        });

        app.Run();
    }
}
