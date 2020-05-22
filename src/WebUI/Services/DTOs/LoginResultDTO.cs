using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Core;

using WebUI.Infrastructure.DTOs;

namespace WebUI.Services.DTOs
{
    public class LoginResultDTO
    {
        public Player LoggedInPlayer { get; }
        public ReadOnlyCollection<BallDTO> BallsPlayedRecently { get; }
        public string JWTPlayerToken { get; }

        public LoginResultDTO(Player loggedInPlayer, IReadOnlyCollection<Ball> ballsPlayedRecently, 
            string jwtPlayerToken)
        {
            this.LoggedInPlayer = loggedInPlayer;
            this.JWTPlayerToken = jwtPlayerToken;
            this.BallsPlayedRecently = ballsPlayedRecently
                .Select(ball => new BallDTO { Name = ball.Name })
                .ToList()
                .AsReadOnly();
        }
    }
}
