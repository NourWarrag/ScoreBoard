using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoreBoard.Server.Data;
using ScoreBoard.Shared;

namespace ScoreBoard.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameConsoleController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GameConsoleController> _logger;

        public GameConsoleController(ILogger<GameConsoleController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<GameConsoleInfo>> Get()
        {
            return await _dbContext.GameConsoles.ToListAsync();
        }

        [HttpPut("{id}/Stop")]
        public async Task<IActionResult> Stop(int id)
        {
            var console = await _dbContext.GameConsoles.FindAsync(id);
            if (console == null)
            {
                return NoContent();
            }
            console.Status = Status.Stopped;

            _dbContext.Update(console);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}