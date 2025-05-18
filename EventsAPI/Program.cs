using EventsAPI.Models;

namespace EventsAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var events = new List<EventItem>();
        var nextId = 1;
        
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://myapp.example.com")
                        .AllowAnyMethod() // GET, POST, PUT, DELETE
                        .AllowAnyHeader() // Любой заголовок
                        .AllowCredentials(); // Если нужно передавать куки / авторизированные заголовки
                });
            });

        // Config
        var app = builder.Build();
        
        app.UseCors("AllowFrontend"); // Активируем CORS
        
        // Затем привычные middleware
        app.UseRouting();
        //app.MapControllers();
        // в minimal api эти промежуточные по не используем
        
        app.MapGet("/api/events", () => Results.Ok(events));
        
        // GET /api/events/{id}
        app.MapGet("/api/events/{id:int}", (int id) =>
        {
            var eventItem = events.FirstOrDefault(e => e.Id == id);
            return eventItem is not null ? Results.Ok(eventItem) : Results.NotFound();
        });
        
        // POST /api/events
        app.MapPost("/api/events", (EventItem newEv) =>
        {
            newEv.Id = nextId++;
            events.Add(newEv);
            return Results.Created($"api/events/{newEv.Id}", newEv);
        });
        
        // PUT /api/events/{id}
        app.MapPut("/api/events/{id:int}", (int id, EventItem upd) =>
        {
            var eventItem = events.FirstOrDefault(e => e.Id == id);
            if (eventItem is null) return Results.NotFound();
            
            eventItem.Title = upd.Title;
            eventItem.EventDate = upd.EventDate;
            
            return Results.NoContent();
        });
        
        // DELETE /api/events/{id}
        app.MapDelete("/api/events/{id:int}", (int id) =>
        {
            var eventItem = events.FirstOrDefault(e => e.Id == id);
            if (eventItem is null) return Results.NotFound();
            
            events.Remove(eventItem);
            return Results.NoContent();
        });
        
        // Minimal API
        app.Run();
    }
}