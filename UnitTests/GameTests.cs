using System.Linq;
using System.Collections.Generic;

using Core;

using CSharpFunctionalExtensions;

using FluentAssertions;

using NUnit.Framework;

namespace UnitTests
{
    public class GameTests
    {
        #region New Game tests

        [Test]
        public void WhenCreatingANewGame_ItsDefaultStateIsDraft()
        {
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.State.Should().Be(GameState.Draft);
        }

        [Test]
        public void WhenCreatingANewGame_ItsMaxBallsPerBucket_ShouldBeSet()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5);
            newGameResult.Value.WithNBallsMaxPerBoardBucket.Should().Be(5);
        }

        [Test]
        public void WhenCreatingANewGame_IfProvidingWrongMaxBallsPerBucket_ItShouldFail(
            [Values(-2, 0, 1)] short wrongValues)
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: wrongValues);
            newGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenCreatingGame_IfWrongNameProvided_ItFails(
            [Values("", null)] string withName)
        {
            var newGameResult = CreateGameWithoutPlayers(withName: withName);
            newGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenCreatingANewGame_ItShouldHaveAName()
        {
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.Name.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItDoesNotHaveAnyPlayers()
        {
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.Players.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItHasBallsConfigured()
        {
            var newGameResult = CreateGameWithoutPlayers(withTotalBallsCount: 20, withMaxNBallsPerBucket: 2);
            newGameResult.Value.BallsConfigured.Should()
                .NotBeNull()
                .And.HaveCount(20);

            var ballsToCheck = new List<Ball>() {
                Ball.Create(BallLeter.B, 1).Value,
                Ball.Create(BallLeter.B, 2).Value,
                Ball.Create(BallLeter.B, 3).Value,
                Ball.Create(BallLeter.B, 4).Value,
                Ball.Create(BallLeter.I, 5).Value,
                Ball.Create(BallLeter.I, 6).Value,
                Ball.Create(BallLeter.I, 7).Value,
                Ball.Create(BallLeter.I, 8).Value,
                Ball.Create(BallLeter.N, 9).Value,
                Ball.Create(BallLeter.N, 10).Value,
                Ball.Create(BallLeter.N, 11).Value,
                Ball.Create(BallLeter.N, 12).Value,
                Ball.Create(BallLeter.G, 13).Value,
                Ball.Create(BallLeter.G, 14).Value,
                Ball.Create(BallLeter.G, 15).Value,
                Ball.Create(BallLeter.G, 16).Value,
                Ball.Create(BallLeter.O, 17).Value,
                Ball.Create(BallLeter.O, 18).Value,
                Ball.Create(BallLeter.O, 19).Value,
                Ball.Create(BallLeter.O, 20).Value,
            };

            foreach (var ball in ballsToCheck)
                newGameResult.Value.BallsConfigured.Should().Contain(ball);
        }

        [Test]
        public void WhenCreatingANewGame_IfWrongBallsCountIsProvided_ItFails(
            [Values(-5, -4, -1, 0, 1, 4, 6, 22)] short ballsCount)
        {
            var newGameResult = CreateGameWithoutPlayers(withTotalBallsCount: ballsCount);
            newGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenCreatingANewGame_ItDoesNotHaveAnyBallsPlayed()
        {
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.BallsPlayed.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        #endregion

        #region Adding Players

        [Test]
        public void WhenAddingAValidPlayer_ItWorks()
        {
            var newPlayer = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);

            var newGameResult = CreateGameWithoutPlayers();
            var result = newGameResult.Value.AddPlayer(newPlayer);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            newGameResult.Value.Players.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.Contain(newPlayer);
        }

        [Test]
        public void WhenAddingAInvalidPlayer_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var result = newGameResult.Value.AddPlayer(null);

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingAPlayer_WhenGameHasStarted_ItFails()
        {
            var newPlayer = new Player("Player 03", PlayerSecurity.Create("login 03", "passwd 03").Value);
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Start();

            var result = newGame.AddPlayer(newPlayer);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameLogin_ItFails()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 02", PlayerSecurity.Create("login 01", "passwd 02").Value);
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameName_ItFails()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 01", PlayerSecurity.Create("login 02", "passwd 02").Value);
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithDifferentLogins_ItWorks()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 02", PlayerSecurity.Create("login 02", "passwd 02").Value);
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsSuccess.Should().BeTrue();
        }

        #endregion

        #region Adding Boards to Players

        [Test]
        public void WhenAddingBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var newPlayer = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(addBoardResult.Value);
        }

        [Test]
        public void WhenAddingBoardToPlayer_IfGameHasStarted_ItFails()
        {
            (var newGame, var player1, var _) = CreateDefaultGameWithPlayers();
            newGame.Start();
            
            var addBoardResult = newGame.AddBoardToPlayer(player1);
            addBoardResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingBoardToNonExistingPlayer_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 02", PlayerSecurity.Create("login 01", "passwd 01").Value);
            newGameResult.Value.AddPlayer(newPlayer1);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(newPlayer2);
            addBoardResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Starging Game

        [Test]
        public void WhenGameIsInRightState_StartingGame_Works()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();

            var startGameResult = newGame.Start();

            startGameResult.IsSuccess.Should().BeTrue();
        }

        [Test]
        public void WhenGamePlayersDontHaveBoardsAdded_StartingGame_Fails()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers(false);

            var startGameResult = newGame.Start();

            startGameResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Playing Balls

        [Test]
        public void WhenPlayingBalls_IfInvalidBallProvided_ItFails()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Start();

            var ballToPlay = Ball.Create(BallLeter.B, 120).Value;
            var playBallResult = newGame.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfBallHasBeenPlayedAlready_ItFails()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Start();

            var ballToPlay = Ball.Create(BallLeter.B, 2).Value;
            newGame.PlayBall(ballToPlay);
            var playBallResult = newGame.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfGameHasNotStartedYet_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var ballToPlay = Ball.Create(BallLeter.B, 5).Value;

            var playBallResult = newGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingInvalidBall_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var ballToPlay = Ball.Create(BallLeter.I, 5).Value;

            newGameResult.Value.Start();
            var playBallResult = newGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingBallAlreadyPlayed_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var ballToPlay = Ball.Create(BallLeter.B, 5).Value;

            newGameResult.Value.Start();
            var playBallResult = newGameResult.Value.PlayBall(ballToPlay);
            playBallResult = newGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingValidBall_ItWorks()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            var ballToPlay = Ball.Create(BallLeter.B, 5).Value;
            
            newGame.Start();
            var playBallResult = newGame.PlayBall(ballToPlay);

            playBallResult.IsSuccess.Should().BeTrue();
            newGame.BallsPlayed.Should().NotBeEmpty()
                .And.Contain(ballToPlay);
            
            foreach(var player in newGame.Players)
            {
                foreach(var board in player.Boards)
                {
                    if (board.BallsConfigured.Contains(ballToPlay))
                        board.BallsPlayed.Should().Contain(ballToPlay);
                }
            }
        }

        [Test]
        public void WhenPlayingEachBallForAPlayersBoard_ThatBoardShouldBeSetAsWinner()
        {
            (var newGame, var chosenPlayerSetToWin, var otherPlayer) = CreateDefaultGameWithPlayers();
            newGame.Start();
            var chosenBoardSetToWin = chosenPlayerSetToWin.Boards.First();

            foreach(var ball in chosenBoardSetToWin.BallsConfigured)
                newGame.PlayBall(ball);

            chosenBoardSetToWin.State.Should().Be(BoardState.Winner);

            foreach(var player in new Player[] { chosenPlayerSetToWin, otherPlayer })
            {
                foreach(var board in player.Boards)
                {
                    if(board == chosenBoardSetToWin)
                        board.State.Should().Be(BoardState.Winner);
                    else
                        board.State.Should().Be(BoardState.Playing);
                }
            }
        }

        #endregion

        #region Helpers

        private Result<Game> CreateGameWithoutPlayers(string withName = "Name 01", short withTotalBallsCount = 75, short withMaxNBallsPerBucket = 5) =>
            Game.Create(withName, withTotalBallsCount, withMaxNBallsPerBucket);

        private (Game game, Player player1, Player player2) CreateDefaultGameWithPlayers(bool shouldBoardsBeAdded = true)
        {
            var newGame = CreateGameWithoutPlayers().Value;
            var player1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var player2 = new Player("Player 02", PlayerSecurity.Create("login 02", "passwd 02").Value);

            newGame.AddPlayer(player1);
            newGame.AddPlayer(player2);

            if(shouldBoardsBeAdded)
            {
                newGame.AddBoardToPlayer(player1);
                newGame.AddBoardToPlayer(player1);
                newGame.AddBoardToPlayer(player2);
            }

            return (newGame, player1, player2);
        }

        #endregion
    }
}