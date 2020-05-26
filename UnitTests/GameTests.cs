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
        public void WhenAddingStandardBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5, withGameType: GameType.STANDARD);
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(newAddedBoard);
            newAddedBoard.BallsConfigured.Should().HaveCount(24);
            newAddedBoard.BallsPlayed.Should().BeEmpty();
            
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.B).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.I).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.N).Should().Be(4);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.G).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.O).Should().Be(5);
        }

        [Test]
        public void WhenAddingLBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5, withGameType: GameType.L);
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(newAddedBoard);
            newAddedBoard.BallsConfigured.Should().HaveCount(9);
            newAddedBoard.BallsPlayed.Should().BeEmpty();

            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.B).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.I).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.N).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.G).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.O).Should().Be(1);
        }

        [Test]
        public void WhenAddingOBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5, withGameType: GameType.O);
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(newAddedBoard);
            newAddedBoard.BallsConfigured.Should().HaveCount(16);
            newAddedBoard.BallsPlayed.Should().BeEmpty();

            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.B).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.I).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.N).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.G).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.O).Should().Be(5);
        }

        [Test]
        public void WhenAddingTBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5, withGameType: GameType.T);
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(newAddedBoard);
            newAddedBoard.BallsConfigured.Should().HaveCount(9);
            newAddedBoard.BallsPlayed.Should().BeEmpty();

            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.B).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.I).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.N).Should().Be(5);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.G).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.O).Should().Be(1);
        }

        [Test]
        public void WhenAddingXBoardToPlayer_ItWorks()
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerBucket: 5, withGameType: GameType.X);
            var newPlayer = CreateValidPlayer();
            newGameResult.Value.AddPlayer(newPlayer);

            var addBoardResult = newGameResult.Value.AddBoardToPlayer(this._randomizer, newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().NotBeEmpty()
                .And.Contain(newAddedBoard);
            newAddedBoard.BallsConfigured.Should().HaveCount(9);
            newAddedBoard.BallsPlayed.Should().BeEmpty();

            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.B).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.I).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.N).Should().Be(1);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.G).Should().Be(2);
            newAddedBoard.BallsConfigured.Count(ball => ball.Letter == BallLeter.O).Should().Be(2);
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
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();

            var startGameResult = draftGame.Start();

            startGameResult.IsSuccess.Should().BeTrue();
            startGameResult.Value.Name.Should().Be(draftGame.Name);
            startGameResult.Value.GameType.Should().Be(draftGame.GameType);
            startGameResult.Value.BallsConfigured.Should().HaveCount(draftGame.BallsConfigured.Count);
            startGameResult.Value.BallsPlayed.Should().BeEmpty();
            startGameResult.Value.Players.Should().HaveCount(draftGame.Players.Count);
        }

        [Test]
        public void WhenGamePlayersDontHaveBoardsAdded_StartingGame_Fails()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers(false);

            var startGameResult = draftGame.Start();

            startGameResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Playing Balls

        [Test]
        public void WhenPlayingBalls_IfInvalidBallProvided_ItFails()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var activeGameResult = draftGame.Start();

            var ballToPlay = Ball.Create(BallLeter.B, 120).Value;
            var playBallResult = activeGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfBallHasBeenPlayedAlready_ItFails()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var activeGameResult = draftGame.Start();

            var ballToPlay = Ball.Create(BallLeter.B, 2).Value;
            activeGameResult.Value.PlayBall(ballToPlay);
            var playBallResult = activeGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingInvalidBall_ItFails()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var ballToPlay = Ball.Create(BallLeter.I, 5).Value;

            var activeGame = draftGame.Start().Value;
            var playBallResult = activeGame.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingBallAlreadyPlayed_ItFails()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var ballToPlay = Ball.Create(BallLeter.B, 5).Value;

            var activeGame = draftGame.Start().Value;
            var playBallResult = activeGame.PlayBall(ballToPlay);
            playBallResult = activeGame.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingValidBall_ItWorks()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var ballToPlay = Ball.Create(BallLeter.B, 5).Value;
            
            var activeGameResult = draftGame.Start();
            var playBallResult = activeGameResult.Value.PlayBall(ballToPlay);

            playBallResult.IsSuccess.Should().BeTrue();
            activeGameResult.Value.BallsPlayed.Should().NotBeEmpty()
                .And.Contain(ballToPlay);
            
            foreach(var player in activeGameResult.Value.Players)
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
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();
            var activeGameResult = draftGame.Start();
            var randomizer = new Random();

            do
            {
                var randomPlayBallResult = activeGameResult.Value.RadmonlyPlayBall(randomizer);
                randomPlayBallResult.IsSuccess.Should().BeTrue();

                activeGameResult.Value.BallsConfigured.Should().Contain(randomPlayBallResult.Value);
                activeGameResult.Value.BallsPlayed.Should().Contain(randomPlayBallResult.Value);

                var winnersResult = activeGameResult.Value.GetPotentialWinners();
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
            (var activeGame, var chosenPlayerSetToWin, var _, var __) = SetWinnerPlayerForGame();

            var finishedGameResult = activeGame.SetWinner(chosenPlayerSetToWin);

            finishedGameResult.IsSuccess.Should().BeTrue();
            finishedGameResult.Value.Winner.Should().Be(chosenPlayerSetToWin);
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingWrongWinner_ItFails()
        {
            (var activeGame, var _, var __, var otherPlayer) = SetWinnerPlayerForGame();

            var finishedGameResult = activeGame.SetWinner(otherPlayer);

            finishedGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingNullWinner_ItFails()
        {
            (var activeGame, var _, var __, var ___) = SetWinnerPlayerForGame();

            var finishedGameResult = activeGame.SetWinner(null);

            finishedGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenSettingTheWinner_IfProvindingNonExistingPlayer_ItFails()
        {
            (var activeGame, var _, var __, var ___) = SetWinnerPlayerForGame();

            var finishedGameResult = activeGame.SetWinner(CreateValidPlayer(withName: "Another name", withLogin: "login 4"));

            finishedGameResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Helpers

        private Result<DraftGame> CreateGameWithoutPlayers(string withName = "Name 01", short withTotalBallsCount = 75, 
            short withMaxNBallsPerBucket = 5, GameType withGameType = GameType.STANDARD) =>
            DraftGame.Create(withName, withGameType, withTotalBallsCount, withMaxNBallsPerBucket);

        private (DraftGame game, Player player1, Player player2) CreateDefaultGameWithPlayers(bool shouldBoardsBeAdded = true)
        {
            var draftGame = CreateGameWithoutPlayers().Value;
            var player1 = CreateValidPlayer(withName: "Player 01", withLogin: "login 01");
            var player2 = CreateValidPlayer(withName: "Player 02", withLogin: "login 02");

            draftGame.AddPlayer(player1);
            draftGame.AddPlayer(player2);

            if(shouldBoardsBeAdded)
            {
                draftGame.AddBoardToPlayer(this._randomizer, player1);
                draftGame.AddBoardToPlayer(this._randomizer, player1);
                draftGame.AddBoardToPlayer(this._randomizer, player2);
            }

            return (draftGame, player1, player2);
        }

        private (ActiveGame game, Player winner, Board winningBoard, Player looser) SetWinnerPlayerForGame()
        {
            (var draftGame, var chosenPlayerSetToWin, var otherPlayer) = CreateDefaultGameWithPlayers();
            var activeGameResult = draftGame.Start();
            var chosenBoardSetToWin = chosenPlayerSetToWin.Boards.First();

            foreach (var ball in chosenBoardSetToWin.BallsConfigured)
                activeGameResult.Value.PlayBall(ball);

            return (activeGameResult.Value, chosenPlayerSetToWin, chosenBoardSetToWin, otherPlayer);
        }

        private Player CreateValidPlayer(string withName = "Player 01", 
            string withLogin = "Login 01", string  withPassword = "Password 01") =>
            Player.Create(withName, PlayerSecurity.Create(withLogin, withPassword).Value).Value;

        #endregion
    }
}