using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ScoreBoard.Server.Data;
using ScoreBoard.Shared;

namespace ScoreBoard.Server.Hubs
{
    public class ScoreboardHub : Hub
    {
         
        private readonly IServiceScopeFactory _scopeFactory;

        public ScoreboardHub( IServiceScopeFactory scopeFactory)
        {
            
            _scopeFactory = scopeFactory;
        }

        public async Task RegisterConsole(GameConsoleInfo consoleData)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var console = dbContext.GameConsoles.FirstOrDefault(i => i.Number == consoleData.Number);
            if (console == null)
            {
                dbContext.Add(consoleData);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                console.Url = consoleData.Url;
                console.PlayerName = consoleData.PlayerName;
                console.Score = consoleData.Score;
                console.Status = consoleData.Status;
                dbContext.Update(console);
                await dbContext.SaveChangesAsync();
            }


            var data = await dbContext.GameConsoles.ToListAsync();
            await Clients.All.SendAsync("UpdateScoreboard", data);
        }

        public async Task IncreaseScore(int consoleNumber, int scoreIncrease)
        {

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var console = dbContext.GameConsoles.FirstOrDefault(i => i.Number == consoleNumber);

            if (console != null)
            {
                if (console.Status == Status.Running)
                {
                    console.Score += scoreIncrease;
                    dbContext.Update(console);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    console.Score = scoreIncrease;
                    console.Status = Status.Running;
                    dbContext.Update(console);
                    await dbContext.SaveChangesAsync();
                }
                var data = await dbContext.GameConsoles.ToListAsync();
                await Clients.All.SendAsync("UpdateScoreboard", data);
            }
        }
        public async Task StartNewGame(int consoleNumber, string playerName)
        {

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var console = dbContext.GameConsoles.FirstOrDefault(i => i.Number == consoleNumber);

            if (console != null)
            {

                console.Score = 0;
                console.PlayerName = playerName;
                console.Status = Status.Running;
                dbContext.Update(console);
                await dbContext.SaveChangesAsync();
            }
            var data = await dbContext.GameConsoles.ToListAsync();
            await Clients.All.SendAsync("UpdateScoreboard", data);
        }
        public async Task EndGame(int consoleNumber)
        {

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var console = dbContext.GameConsoles.FirstOrDefault(i => i.Number == consoleNumber);

            if (console != null)
            {
                console.Status = Status.Stopped;
                dbContext.Update(console);
                await dbContext.SaveChangesAsync();
            }
            var data = await dbContext.GameConsoles.ToListAsync();
            await Clients.All.SendAsync("UpdateScoreboard", data);
        }
    }
}

