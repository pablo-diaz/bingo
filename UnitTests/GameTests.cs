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
        #region New Game tests

        [Test]
        public void WhenCreatingANewGame_AllPlayersShouldHaveAtLeastOneBoard()
        {
            (var _, var player1, var player2) = CreateDefaultGameWithPlayers();
            player1.Boards.Should().HaveCount(1);
            player2.Boards.Should().HaveCount(1);
        }

        [Test]
        public void WhenCreatingANewGame_ItsMaxBallsPerColumn_ShouldBeSet()
        {
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5).Value;
            newGame.MaxNBallsPerColumn.Should().Be(5);
        }

        [Test]
        public void WhenCreatingANewGame_IfProvidingWrongMaxBallsPerColumn_ItShouldFail(
            [Values(-2, 0)] short wrongValues)
        {
            var newGameResult = CreateGameWithoutPlayers(withMaxNBallsPerColumn: wrongValues);
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
            var newGame = CreateGameWithoutPlayers().Value;
            newGame.Name.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItDoesNotHaveAnyPlayers()
        {
            var newGame = CreateGameWithoutPlayers().Value;
            newGame.Players.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItHasBallsConfigured()
        {
            var newGame = CreateGameWithoutPlayers(withTotalBallsCount: 20, withMaxNBallsPerColumn: 2).Value;
            newGame.BallsConfigured.Should()
                .NotBeNull()
                .And.HaveCount(20);

            var ballsToCheck = new List<string>() {
                "B1", "B2", "B3", "B4",
                "I5", "I6", "I7", "I8",
                "N9", "N10", "N11", "N12",
                "G13", "G14", "G15", "G16",
                "O17", "O18", "O19", "O20",
            };

            foreach (var ball in ballsToCheck)
                newGame.BallsConfigured.Any(b => b.Name == ball).Should().BeTrue();
        }

        [Test]
        public void WhenCreatingANewGame_IfWrongBallsCountIsProvided_ItFails(
            [Values(-5, -4, -1, 0, 1, 4, 6, 22)] short ballsCount)
        {
            var newGameResult = CreateGameWithoutPlayers(withTotalBallsCount: ballsCount, withMaxNBallsPerColumn: 5);
            newGameResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Adding Players

        [Test]
        public void WhenAddingAValidPlayer_ItWorks()
        {
            var newGame = CreateGameWithoutPlayers().Value;
            var newPlayer = AddValidPlayer(toGame: newGame, withName: "Player 01");

            newGame.Players.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.Contain(newPlayer);

            newPlayer.Boards.Should().HaveCount(1);
        }

        [Test]
        public void WhenAddingAPlayer_IfWrongNameIsProvided_ItFails(
            [Values(null, "")] string withName)
        {
            var newGame = CreateGameWithoutPlayers().Value;
            var result = newGame.AddPlayer(withName);

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameName_ItFails()
        {
            var newGame = CreateGameWithoutPlayers().Value;
            newGame.AddPlayer(withName: "Player 01");
            var result = newGame.AddPlayer(withName: "Player 01");

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithDifferentNames_ItWorks()
        {
            var newGame = CreateGameWithoutPlayers().Value;
            newGame.AddPlayer(withName: "Player 01");
            var result = newGame.AddPlayer(withName: "Player 02");

            result.IsSuccess.Should().BeTrue();

            newGame.Players.Should().HaveCount(2);
            newGame.Players.Any(p => p.Name == "Player 01").Should().BeTrue();
            newGame.Players.Any(p => p.Name == "Player 02").Should().BeTrue();
        }

        #endregion

        #region Editing players

        [Test, Sequential]
        public void WhenEditingPlayer_ItWorks()
        {
            (var game, var playerToUpdate, var _) = CreateDefaultGameWithPlayers();
            
            var updatePlayerResult = game.UpdatePlayerInfo(playerToUpdate, newName: "NewName 01");

            updatePlayerResult.IsSuccess.Should().BeTrue();

            game.Players.Should().HaveCount(2);
            game.Players.Any(p => p.Name == "NewName 01").Should().BeTrue();
        }

        [Test]
        public void WhenEditingPlayer_IfPlayerToUpdateDoesNotExistInCurrentGame_ItFails()
        {
            (var game1, var _, var __) = CreateDefaultGameWithPlayers();
            (var _, var playerFromAnotherGame, var ___) = CreateDefaultGameWithPlayers();

            var updatePlayerResult = game1.UpdatePlayerInfo(playerFromAnotherGame, newName: "New Name");

            updatePlayerResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenEditingPlayer_IfAnotherPlayerWithSameNameExists_ItFails()
        {
            (var game, var player1, var player2) = CreateDefaultGameWithPlayers(
                nameForFirstPlayer: "Name01", nameForSecondPlayer: "Name02");

            var updatePlayerResult = game.UpdatePlayerInfo(player1, newName: "Name02");

            updatePlayerResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Removing player

        [Test]
        public void WhenRemovingValidPlayer_ItWorks()
        {
            (var game, var player1, var player2) = CreateDefaultGameWithPlayers();
            var player3 = AddValidPlayer(toGame: game, withName: "Name03");

            var result = game.RemovePlayer(player1);

            result.IsSuccess.Should().BeTrue();

            game.Players.Should().HaveCount(2)
                .And.NotContain(player1)
                .And.Contain(player2)
                .And.Contain(player3);
        }

        [Test]
        public void WhenRemovingNonExistingPlayer_ItFails()
        {
            (var game1, var _, var __) = CreateDefaultGameWithPlayers();
            (var _, var playerFromAnotherGame, var ___) = CreateDefaultGameWithPlayers();
            
            var result = game1.RemovePlayer(playerFromAnotherGame);

            result.IsFailure.Should().BeTrue();

            game1.Players.Should().HaveCount(2);
        }

        #endregion

        #region Adding Boards to Players

        [Test]
        public void WhenAddingStandardBoardToPlayer_ItWorks()
        {
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5, withGameType: GameType.STANDARD).Value;
            var newPlayer = AddValidPlayer(toGame: newGame);
            var addBoardResult = newGame.AddBoardToPlayer(newPlayer);

            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;

            newPlayer.Boards.Should().HaveCount(1 + 1)
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
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5, withGameType: GameType.L).Value;
            var newPlayer = AddValidPlayer(toGame: newGame);

            var addBoardResult = newGame.AddBoardToPlayer(newPlayer);

            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().HaveCount(1 + 1)
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
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5, withGameType: GameType.O).Value;
            var newPlayer = AddValidPlayer(toGame: newGame);

            var addBoardResult = newGame.AddBoardToPlayer(newPlayer);

            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().HaveCount(1 + 1)
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
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5, withGameType: GameType.T).Value;
            var newPlayer = AddValidPlayer(toGame: newGame);

            var addBoardResult = newGame.AddBoardToPlayer(newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().HaveCount(1 + 1)
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
            var newGame = CreateGameWithoutPlayers(withMaxNBallsPerColumn: 5, withGameType: GameType.X).Value;
            var newPlayer = AddValidPlayer(toGame: newGame);

            var addBoardResult = newGame.AddBoardToPlayer(newPlayer);
            addBoardResult.IsSuccess.Should().BeTrue();

            var newAddedBoard = addBoardResult.Value;
            newPlayer.Boards.Should().HaveCount(1 + 1)
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
            (var game1, var _, var __) = CreateDefaultGameWithPlayers();
            (var _, var playerFromAnotherGame, var ___) = CreateDefaultGameWithPlayers();

            var addBoardResult = game1.AddBoardToPlayer(playerFromAnotherGame);

            addBoardResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Removing board from Player

        [Test]
        public void WhenRemovingBoardFromPlayer_ItWorks()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();
            var remainingBoard = game.AddBoardToPlayer(player1).Value;

            var result = game.RemoveBoardFromPlayer(player1, boardToRemove);

            result.IsSuccess.Should().BeTrue();

            player1.Boards.Should().HaveCount(2 - 1);
            player1.Boards.Should().NotContain(boardToRemove);
            player1.Boards.Should().Contain(remainingBoard);
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfItIsHisLastOne_ItFails()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();

            var result = game.RemoveBoardFromPlayer(player1, boardToRemove);

            result.IsFailure.Should().BeTrue();
            player1.Boards.Should().HaveCount(1);
            player1.Boards.Should().Contain(boardToRemove);
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfProvidingNonExistingPlayer_ItFails()
        {
            (var game, var player1, var _) = CreateDefaultGameWithPlayers();
            (var _, var playerFromAnotherGame, var _) = CreateDefaultGameWithPlayers();
            var boardToRemove = player1.Boards.First();
            game.AddBoardToPlayer(player1);

            var result = game.RemoveBoardFromPlayer(playerFromAnotherGame, boardToRemove);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenRemovingBoardFromPlayer_IfProvidingNonExistingBoardFromPlayer_ItFails()
        {
            (var game, var player1, var player2) = CreateDefaultGameWithPlayers();
            var boardFromPlayer2ToRemove = player2.Boards.First();
            
            game.AddBoardToPlayer(player1);

            game.AddBoardToPlayer(player2);
            game.AddBoardToPlayer(player2);

            var result = game.RemoveBoardFromPlayer(player1, boardFromPlayer2ToRemove);

            result.IsFailure.Should().BeTrue();

            player1.Boards.Should().HaveCount(1 + 1);
            player2.Boards.Should().HaveCount(1 + 1 + 1);
        }

        #endregion

        #region Starging Game

        [Test]
        public void WhenGameIsInRightState_StartingGame_Works()
        {
            (var draftGame, var _, var __) = CreateDefaultGameWithPlayers();

            var startGameResult = draftGame.Start();

            startGameResult.IsSuccess.Should().BeTrue();
            
            startGameResult.Value.BallsPlayed.Should().BeEmpty();
            startGameResult.Value.Should().BeOfType(typeof(ActiveGame));
            startGameResult.Value.Winner.HasNoValue.Should().BeTrue();
        }

        #endregion

        #region Playing Balls

        [Test]
        public void WhenPlayingBalls_IfBallHasBeenPlayedAlready_ItFails()
        {
            (var game, var _, var __) = CreateDefaultGameWithPlayers();
            game = game.Start().Value;
            var ballToPlay = game.BallsConfigured.First();
            game.PlayBall(ballToPlay);

            var playBallResult = game.PlayBall(ballToPlay);

            playBallResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenPlayingBalls_IfProvidingValidBall_ItWorks()
        {
            (var game, var _, var __) = CreateDefaultGameWithPlayers();
            game = game.Start().Value;
            var ballToPlay = game.BallsConfigured.First();

            var playBallResult = game.PlayBall(ballToPlay);

            playBallResult.IsSuccess.Should().BeTrue();

            game.BallsPlayed.Should().HaveCount(1)
                .And.Contain(ballToPlay);
            
            foreach(var player in game.Players)
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
            (var game, var __, var chosenBoardSetToWin, var ___) = SetWinnerPlayerForGame();

            chosenBoardSetToWin.State.Should().Be(BoardState.Winner);
            game.Winner.HasNoValue.Should().BeTrue();
        }

        [Test]
        public void WhenRandomlyPlayingBall_ItWorks()
        {
            (var game, var _, var __) = CreateDefaultGameWithPlayers();
            game = game.Start().Value;

            do
            {
                var randomPlayBallResult = game.RadmonlyPlayBall();

                randomPlayBallResult.IsSuccess.Should().BeTrue();

                game.BallsConfigured.Should().Contain(randomPlayBallResult.Value);
                game.BallsPlayed.Should().Contain(randomPlayBallResult.Value);

                if (IsThereAnyPotentialWinner(inGame: game))
                    break;
            } while (true);

            game.Winner.HasNoValue.Should().BeTrue();
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
            (var _, var playerFromAnotherGame, var _) = CreateDefaultGameWithPlayers();

            var finishedGameResult = activeGame.SetWinner(playerFromAnotherGame);

            finishedGameResult.IsFailure.Should().BeTrue();
        }

        #endregion

        #region Helpers

        private static bool IsThereAnyPotentialWinner(Game inGame) =>
            inGame.Players.Any(p => p.Boards.Any(b => b.State == BoardState.Winner));

        private static Result<Game> CreateGameWithoutPlayers(string withName = "Name 01", short withTotalBallsCount = 75, 
            short withMaxNBallsPerColumn = 5, GameType withGameType = GameType.STANDARD) =>
            GameServices.CreateGame(withName: withName, gameType: withGameType,
                withNBallsTotal: withTotalBallsCount, withNBallsMaxPerColumn: withMaxNBallsPerColumn);

        private static (Game game, Player player1, Player player2) CreateDefaultGameWithPlayers(
            string nameForFirstPlayer = "Player 01", string nameForSecondPlayer = "Player 02")
        {
            var draftGame = CreateGameWithoutPlayers().Value;
            var player1 = AddValidPlayer(toGame: draftGame, withName: nameForFirstPlayer);
            var player2 = AddValidPlayer(toGame: draftGame, withName: nameForSecondPlayer);

            return (draftGame, player1, player2);
        }

        private static (Game game, Player winner, Board winningBoard, Player looser) SetWinnerPlayerForGame()
        {
            (var game, var chosenPlayerSetToWin, var otherPlayer) = CreateDefaultGameWithPlayers();
            game = game.Start().Value;
            var chosenBoardSetToWin = chosenPlayerSetToWin.Boards.First();

            foreach (var ball in chosenBoardSetToWin.BallsConfigured)
                game.PlayBall(ball);

            return (game, chosenPlayerSetToWin, chosenBoardSetToWin, otherPlayer);
        }

        private static Player AddValidPlayer(Game toGame, string withName = "Player 01")
        {
            toGame.AddPlayer(withName);
            return toGame.Players.First(p => p.Name == withName);
        }

        #endregion
    }
}