using System;
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
        private Random _randomizer;

        [OneTimeSetUp]
        public void SetUpForAllTests()
        {
            this._randomizer = new Random();
        }

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
        public void WhenCreatingANewGame_ItsWinner_ShouldNotBeSet()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Winner.HasNoValue.Should().BeTrue();
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
            var newPlayer = CreateValidPlayer();

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
            var newPlayer = CreateValidPlayer(withName: "Player 03", withLogin: "login 03");
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Start();

            var result = newGame.AddPlayer(newPlayer);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameLogin_ItFails()
        {
            var newPlayer1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var newPlayer2 = CreateValidPlayer(withName: "Player 02", withLogin: "login 01");
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameName_ItFails()
        {
            var newPlayer1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var newPlayer2 = CreateValidPlayer(withName: "Player 01", withLogin: "login 02");
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithDifferentLogins_ItWorks()
        {
            var newPlayer1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var newPlayer2 = CreateValidPlayer(withName: "Player 02", withLogin: "login 02");
            var newGameResult = CreateGameWithoutPlayers();
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsSuccess.Should().BeTrue();
        }

        #endregion

        #region Editing players

        [Test, Sequential]
        public void WhenEditingPlayer_ItWorks(
            [Values("NewName",     "NewName", null,       null)] string newName,
            [Values("NewLogin",    null,      "NewLogin", null)] string newLogin,
            [Values("NewPassword", null,      null,       "NewPassword")] string newPasswd)
        {
            (var game, var playerToUpdate, var _) = CreateDefaultGameWithPlayers();
            
            newName = newName ?? playerToUpdate.Name;
            newLogin = newLogin ?? playerToUpdate.Security.Login;
            newPasswd = newPasswd ?? playerToUpdate.Security.Password;

            var newPlayerInfo = CreateValidPlayer(withName: newName, withLogin: newLogin, withPassword: newPasswd);
            var updatePlayerResult = game.UpdatePlayer(playerToUpdate, newPlayerInfo);

            updatePlayerResult.IsSuccess.Should().BeTrue();
            game.Players.Should().NotBeEmpty()
                .And.HaveCount(2)
                .And.Contain(newPlayerInfo);
            playerToUpdate.Name.Should().Be(newName);
            playerToUpdate.Security.Login.Should().Be(newLogin);
            playerToUpdate.Security.Password.Should().Be(newPasswd);
        }

        [Test]
        public void WhenEditingPlayer_IfGameIsNotDraft_ItFails()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            game.Start();

            var newPlayerInfo = CreateValidPlayer(withName: "Updated Player", withLogin: "Updated Login");
            var updatePlayerResult = game.UpdatePlayer(player1, newPlayerInfo);

            updatePlayerResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenEditingPlayer_IfPlayerToUpdateDoesNotExist_ItFails()
        {
            (var game, var _, var __) = CreateDefaultGameWithPlayers();
            var nonExistingPlayer = CreateValidPlayer(withName: "NonExisting player");
            var newPlayerInfo = CreateValidPlayer(withName: "Updated Player", withLogin: "Updated Login");

            var updatePlayerResult = game.UpdatePlayer(nonExistingPlayer, newPlayerInfo);

            updatePlayerResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenEditingPlayer_IfNewPlayerInfoAlreadyExists_ItFails()
        {
            (var game, var playerToBeUpdated, var anotherPlayer) = CreateDefaultGameWithPlayers();
            var newPlayerInfo = CreateValidPlayer(withName: anotherPlayer.Name, 
                withLogin: anotherPlayer.Security.Login);

            var updatePlayerResult = game.UpdatePlayer(playerToBeUpdated, newPlayerInfo);

            updatePlayerResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Removing player

        [Test]
        public void WhenRemovingValidPlayer_ItWorks()
        {
            (var game, var player1, var _) = this.CreateDefaultGameWithPlayers();
            
            var result = game.RemovePlayer(player1);

            result.IsSuccess.Should().BeTrue();
            game.Players.Should().NotContain(player1);
        }

        [Test]
        public void WhenRemovingValidPlayer_IfGameIsNotInDraftMode_ItFails()
        {
            (var game, var player1, var _) = this.CreateDefaultGameWithPlayers();
            var _ = game.Start();

            var result = game.RemovePlayer(player1);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenRemovingNonExistingPlayer_ItFails()
        {
            (var game, var _, var __) = this.CreateDefaultGameWithPlayers();
            var nonExistingPlayer = CreateValidPlayer(withName: "NonExisting Player", withLogin: "NonExisting Login");

            var result = game.RemovePlayer(nonExistingPlayer);

            result.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Adding Boards to Players

        [Test]
        public void WhenAddingBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(addBoardResult.Value);
        }

        [Test]
        public void WhenAddingBoardToPlayer_IfGameHasStarted_ItFails()
        {
            (var newGame, var player1, var _) = CreateDefaultGameWithPlayers();
            newGame.Start();
            
            var addBoardResult = newGame.AddBoardToPlayer(this._randomizer, player1);
            addBoardResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingBoardToNonExistingPlayer_ItFails()
        {
            var newGameResult = CreateGameWithoutPlayers();
            var newPlayer1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var newPlayer2 = CreateValidPlayer(withName: "Player 02", withLogin: "login 01");
            newGameResult.Value.AddPlayer(newPlayer1);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer2);
            addBoardResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Removing board from Player

        [Test]
        public void WhenRemovingBoardFromPlayer_ItWorks()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();
            var playerBoardsCount = player1.Boards.Count;

            var result = game.RemoveBoardFromPlayer(player1, boardToRemove);

            result.IsSuccess.Should().BeTrue();
            player1.Boards.Should().HaveCount(playerBoardsCount - 1);
            player1.Boards.Should().NotContain(boardToRemove);
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfGameIsNotInDraftState_ItFails()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();

            var _ = game.Start();
            var result = game.RemoveBoardFromPlayer(player1, boardToRemove);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfProvidingNonExistingPlayer_ItFails()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();
            var nonExistingPlayer = CreateValidPlayer(withName: "NonExistingPlayer", withLogin: "NonExistingLogin");

            var result = game.RemoveBoardFromPlayer(nonExistingPlayer, boardToRemove);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfProvidingNonExistingBoardFromPlayer_ItFails()
        {
            (var game, var player1, var player2) = CreateDefaultGameWithPlayers();
            var boardFromPlayer2ToRemove = player2.Boards.First();

            var result = game.RemoveBoardFromPlayer(player1, boardFromPlayer2ToRemove);

            result.IsFailure.Should().BeTrue();
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
            (var _, var __, var chosenBoardSetToWin, var ___) = SetWinnerPlayerForGame();

            chosenBoardSetToWin.State.Should().Be(BoardState.Winner);
        }

        [Test]
        public void WhenRandomlyPlayingBall_ItWorks()
        {
            (var newGame, var _, var __) = CreateDefaultGameWithPlayers();
            newGame.Start();
            var randomizer = new Random();

            do
            {
                var randomPlayBallResult = newGame.RadmonlyPlayBall(randomizer);
                randomPlayBallResult.IsSuccess.Should().BeTrue();

                newGame.BallsConfigured.Should().Contain(randomPlayBallResult.Value);
                newGame.BallsPlayed.Should().Contain(randomPlayBallResult.Value);

                var winnersResult = newGame.GetPotentialWinners();
                winnersResult.IsSuccess.Should().BeTrue();
                if (winnersResult.Value.Any())
                    break;
            } while (true);

        }

        #endregion

        #region Setting Winner

        [Test]
        public void SettingTheRightWinner_Works()
        {
            (var newGame, var chosenPlayerSetToWin, var _, var __) = SetWinnerPlayerForGame();

            var result = newGame.SetWinner(chosenPlayerSetToWin);

            result.IsSuccess.Should().BeTrue();
            newGame.Winner.HasValue.Should().BeTrue();
            newGame.Winner.Value.Should().Be(chosenPlayerSetToWin);
            newGame.State.Should().Be(GameState.Finished);
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingWrongWinner_ItFails()
        {
            (var newGame, var _, var __, var otherPlayer) = SetWinnerPlayerForGame();

            var result = newGame.SetWinner(otherPlayer);

            result.IsFailure.Should().BeTrue();
            newGame.Winner.HasNoValue.Should().BeTrue();
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingNullWinner_ItFails()
        {
            (var newGame, var _, var __, var ___) = SetWinnerPlayerForGame();

            var result = newGame.SetWinner(null);

            result.IsFailure.Should().BeTrue();
            newGame.Winner.HasNoValue.Should().BeTrue();
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingNonExistingPlayer_ItFails()
        {
            (var newGame, var _, var __, var ___) = SetWinnerPlayerForGame();

            var result = newGame.SetWinner(CreateValidPlayer(withName: "Another name", withLogin: "login 4"));

            result.IsFailure.Should().BeTrue();
            newGame.Winner.HasNoValue.Should().BeTrue();
        }

        [Test]
        public void WhenSettingTheWinner_IfGameHasNotStartedYet_ItFails()
        {
            (var newGame, var chosenPlayerSetToWin, var otherPlayer) = CreateDefaultGameWithPlayers();
            
            var result = newGame.SetWinner(chosenPlayerSetToWin);

            result.IsFailure.Should().BeTrue();
            newGame.Winner.HasNoValue.Should().BeTrue();
        }

        #endregion

        #region Helpers

        private Result<Game> CreateGameWithoutPlayers(string withName = "Name 01", short withTotalBallsCount = 75, short withMaxNBallsPerBucket = 5) =>
            Game.Create(withName, withTotalBallsCount, withMaxNBallsPerBucket);

        private (Game game, Player player1, Player player2) CreateDefaultGameWithPlayers(bool shouldBoardsBeAdded = true)
        {
            var newGame = CreateGameWithoutPlayers().Value;
            var player1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var player2 = CreateValidPlayer(withName: "Player 02", withLogin: "login 02");

            newGame.AddPlayer(player1);
            newGame.AddPlayer(player2);

            if(shouldBoardsBeAdded)
            {
                newGame.AddBoardToPlayer(this._randomizer, player1);
                newGame.AddBoardToPlayer(this._randomizer, player1);
                newGame.AddBoardToPlayer(this._randomizer, player2);
            }

            return (newGame, player1, player2);
        }

        private (Game game, Player winner, Board winningBoard, Player looser) SetWinnerPlayerForGame()
        {
            (var newGame, var chosenPlayerSetToWin, var otherPlayer) = CreateDefaultGameWithPlayers();
            newGame.Start();
            var chosenBoardSetToWin = chosenPlayerSetToWin.Boards.First();

            foreach (var ball in chosenBoardSetToWin.BallsConfigured)
                newGame.PlayBall(ball);

            return (newGame, chosenPlayerSetToWin, chosenBoardSetToWin, otherPlayer);
        }

        private Player CreateValidPlayer(string withName = "Player 01", 
            string withLogin = "Login 01", string  withPassword = "Password 01") =>
            Player.Create(withName, PlayerSecurity.Create(withLogin, withPassword).Value).Value;

        #endregion
    }
}