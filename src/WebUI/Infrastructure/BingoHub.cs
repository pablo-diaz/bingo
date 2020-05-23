using System.Linq;
using System.Threading.Tasks;

using WebUI.Infrastructure.DTOs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.Infrastructure
{
    [Authorize]
    public class BingoHub: Hub
    {
        public override async Task OnConnectedAsync()
        {
            var gameNameClaimFound = Context.User.Claims.First(claim => claim.Type == "GameName");
            await Groups.AddToGroupAsync(Context.ConnectionId, gameNameClaimFound.Value);
            await base.OnConnectedAsync();
        }

        public async Task SendBallPlayedMessage(string inGameName, BallDTO ballPlayed)
        {
            await Clients.Group(inGameName).SendAsync("OnBallPlayedMessage", ballPlayed);
        }

        public async Task SendWinnerMessage(string inGameName, string winnerName)
        {
            await Clients.Group(inGameName).SendAsync("OnSetWinnerMessage", winnerName);
        }
    }
}
