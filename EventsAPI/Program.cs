using EventsAPI.Models;
using Microsoft.AspNetCore.Diagnostics;

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
        
        app.UseMiddleware<LoggingMiddleware>();
        
        // Устанавливаем общий обработчик ошибок
        app.UseExceptionHandler("/error");
        
        // Endpoint для ошибки
        app.Map("/error", (HttpContext context, ILogger<Program> logger) =>
        {
            var innerException = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            logger.LogError(innerException, "Unhandled exception occured");
            
            return Results.Problem(detail: "Внутренняя ошибка сервера. Повторите запрос позже.", statusCode: 500);
        });

        app.MapGet("/api/events", (DateTime? from, DateTime? to, string? sort) =>
        {
            var filtered = events;
            if (from != null)
                filtered = filtered.Where(o => o.EventDate >= from).ToList();
            if (to != null)
                filtered = filtered.Where(o => o.EventDate <= to).ToList();

            if (!string.IsNullOrWhiteSpace(sort))
            {
                var s = sort.Trim().ToLowerInvariant();
                if (s == "asc")
                {
                    filtered.OrderBy(e => e.EventDate);
                }
                else if (s == "desc")
                {
                    filtered.OrderByDescending(e => e.EventDate);
                }
                else
                {
                    return Results.BadRequest("Параметр 'sort' может быть только 'asc' 'desc'.");
                }
            }
            return Results.Ok(filtered.ToList());
        });
        
        // Активируем CORS
        app.UseCors("AllowFrontend"); 
        
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
            if (newEv.EventDate < DateTime.Now)
                return Results.BadRequest("Дата события не может быть в прошлом.");
            
            newEv.Id = nextId++;
            events.Add(newEv);
            return Results.Created($"api/events/{newEv.Id}", newEv);
        });
        
        // PUT /api/events/{id}
        app.MapPut("/api/events/{id:int}", (int id, EventItem upd) =>
        {
            var element = events.Where(o => o.Id == id).FirstOrDefault();
            if (element != null)
                element.Title = upd.Title;

            return Results.Ok();
        });
        
        // DELETE /api/events/{id}
        app.MapDelete("/api/events/{id:int}", (int id) =>
        {
            var element = events.Find(o => o.Id == id);

            if (element != null)
            {
                events.Remove(element);
                return Results.Ok();
            }
            return Results.BadRequest();
        });
        
        // Minimal API
        app.Run();
    }
}