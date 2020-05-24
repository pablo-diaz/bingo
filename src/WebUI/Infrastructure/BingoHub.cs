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
            var gameNameClaimFound = Context.User.Claims.FirstOrDefault(claim => claim.Type == "GameName");
            if (gameNameClaimFound == null)
                return;
            
            await Groups.AddToGroupAsync(Context.ConnectionId, gameNameClaimFound.Value);
            await base.OnConnectedAsync();
        }

        public async Task SendBallPlayedMessage(string inGameName, BallDTO ballPlayed)
        {
            if (Clients == null)
                return;

            var gropuFound = Clients.Group(inGameName);
            if (gropuFound == null)
                return;

            await gropuFound.SendAsync("OnBallPlayedMessage", ballPlayed);
        }

        public async Task SendWinnerMessage(string inGameName, string winnerName)
        {
            if (Clients == null)
                return;

            var gropuFound = Clients.Group(inGameName);
            if (gropuFound == null)
                return;

            await gropuFound.SendAsync("OnSetWinnerMessage", winnerName);
        }
    }
}
