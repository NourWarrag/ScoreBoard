using ScoreBoard.Shared;

namespace GameConsole
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            HubClient consoleApp = new(builder.Configuration["HubUrl"]);
            GameConsoleInfo gameConsole;

            var task = consoleApp.ConnectToMainBoard();
            task.AsTask().Wait();

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: GameConsole.exe <consoleType> <consoleNumber> <Optional:playerName>");
                return;
            }

            var consoleType = args[0];
            var consoleNumber = int.Parse(args[1]);

            if (consoleType == "new")
            {
                var playerName = args.Length >= 3 ? args[2] : "player";

                gameConsole = consoleApp.RegisterNewConsole(consoleNumber, playerName).Result;
            }
            else if (consoleType == "old")
            {
                if (args.Length <= 2)
                {
                    Console.WriteLine("Usage: dotnet run <consoleType> <consoleNumber> <url> <Optional:playerName>");
                    return;
                }
                var url = args[2];
                var playerName = args.Length >= 4 ? args[3] : "player";

                gameConsole = consoleApp.RegisterOldConsole(consoleNumber, url, playerName).Result;
            }
            else
            {
                Console.WriteLine("Invalid console type. Must be 'new' or 'old'.");
                return;
            }


            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton(consoleApp);
            builder.Services.AddSingleton(gameConsole);
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
                    });
            });

            builder.WebHost.UseUrls("http://127.0.0.1:0");

            var app = builder.Build();

            app.Lifetime.ApplicationStopped.Register(async () =>
            {
                await consoleApp.EndGame(gameConsole.Number);
            });

            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();
            app.MapBlazorHub();

            app.MapGet("/gameconsole/score", () => new ConsoleScoreRespone { Score = gameConsole.Score, Number = gameConsole.Number , Status = gameConsole.Status, PlayerName = gameConsole.PlayerName });


            app.MapPost("/gameconsole/IncreaseScore", async () =>
            {
                gameConsole.Score++;
                if (gameConsole.Type == ConsoleType.New)
                    await consoleApp.IncreaseScore(gameConsole.Number, 1);
            });


            app.MapPost("/gameconsole/StartGame", async () =>
            {
                gameConsole.PlayerName = "";
                await consoleApp.StartNewGame(gameConsole.Number, gameConsole.PlayerName);
            });


            app.MapPost("/gameconsole/EndGame", async () =>
            {
                await consoleApp.EndGame(gameConsole.Number);

            });


            app.MapFallbackToPage("/_Host");
            app.Run();
        }

    }




}