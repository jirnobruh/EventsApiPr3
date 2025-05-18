namespace EventsAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Config
        var app = builder.Build();
        
        // Minimal API
        app.Run();
    }
}