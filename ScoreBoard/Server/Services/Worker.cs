using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ScoreBoard.Server.Data;
using ScoreBoard.Server.Hubs;
using ScoreBoard.Shared;
using System;
using System.Net;
using System.Net.Http;

namespace ScoreBoard.Server.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<ScoreboardHub> _scoreboardHub;
        private readonly IServiceScopeFactory _scopeFactory;
        public Worker(ILogger<Worker> logger, IHubContext<ScoreboardHub> scoreboardHub, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scoreboardHub = scoreboardHub;
            _scopeFactory = scopeFactory;
             
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    _logger.LogInformation("Worker running at: {Time}", DateTime.Now);
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                     
                    using var httpClient = new HttpClient();
                    // get all the old consoles
                    var oldConsoles = await dbContext.GameConsoles.Where(i => i.Type == Shared.ConsoleType.Old && i.Status == Shared.Status.Running).ToListAsync();
                   
                    //get the score
                    var tasks = oldConsoles.Select(async console =>
                    {
                        var content = await httpClient.GetFromJsonAsync<ConsoleScoreRespone>(console.Url + "/gameconsole/score");
                        console.Score = content.Score;
                        console.Status = content.Status;
                        console.PlayerName = content.PlayerName;
                        dbContext.Update(console);
                        dbContext.SaveChanges();
                        return content;
                    });
                    await Task.WhenAll(tasks).ContinueWith(itemResponse => {
                         if(!itemResponse.IsCompletedSuccessfully)
                         {
                          
                             Console.WriteLine($"Error occured connecting to console");
                         }
                    });

                    //Notify to update the board 
                    var data = await dbContext.GameConsoles.ToListAsync();
                    await _scoreboardHub.Clients.All.SendAsync("UpdateConsole", data);
                    
                    
                    await Task.Delay(15000, stoppingToken);
                }

            }
        }
    }
}
