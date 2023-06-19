using Microsoft.AspNetCore.SignalR.Client;
using ScoreBoard.Shared;

namespace GameConsole
{
    public partial class Program
    {
        public class HubClient
        {
            private HubConnection hubConnection;
            private readonly string _huburl;

            public HubClient(string huburl)
            {
                _huburl = huburl;
            }

            public async ValueTask ConnectToMainBoard()
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(_huburl)
                    .WithAutomaticReconnect()
                   .ConfigureLogging(logging => {
                       logging.SetMinimumLevel(LogLevel.Information);
                       logging.AddConsole();
                   })
                    .Build();
               

                await hubConnection.StartAsync();
            }
            public async Task StartNewGame(int number, string playerName = "")
            {


                await hubConnection.InvokeAsync("StartNewGame", number, playerName);

            }
            public async Task EndGame(int number)
            {

                await hubConnection.InvokeAsync("EndGame", number);

            }
            public async Task<GameConsoleInfo> RegisterNewConsole(int number, string playerName = "")
            {
                var consoleData = new GameConsoleInfo
                {
                    Number = number,

                    Status = Status.Running,
                    Type = ConsoleType.New,
                    PlayerName = playerName
                };

                await hubConnection.InvokeAsync("RegisterConsole", consoleData);
                return consoleData;
            }

            public async Task<GameConsoleInfo> RegisterOldConsole(int number, string url, string playerName = "")
            {
                var consoleData = new GameConsoleInfo
                {
                    Number = number,
                    Status = Status.Running,
                    Type = ConsoleType.Old,
                    Url = url,
                    PlayerName = playerName
                };

                await hubConnection.InvokeAsync("RegisterConsole", consoleData);
                return consoleData;
            }

            public async ValueTask IncreaseScore(int consoleNumber, int scoreIncrease)
            {
                await hubConnection.InvokeAsync("IncreaseScore", consoleNumber, scoreIncrease);
            }

        }
    }




}