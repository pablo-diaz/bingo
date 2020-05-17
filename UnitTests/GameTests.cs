using System.Collections.Generic;

using Core;

using FluentAssertions;

using NUnit.Framework;

namespace UnitTests
{
    public class GameTests
    {
        [Test]
        public void WhenCreatingANewGame_ItsDefaultStateIsDraft()
        {
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Should().NotBeNull();
            newGameResult.IsSuccess.Should().BeTrue();

            var newGame = newGameResult.Value;
            newGame.State.Should().Be(GameState.Draft);
        }

        [Test]
        public void WhenCreatingGame_IfWrongNameProvided_ItFails(
            [Values("", null)] string withName)
        {
            var newGameResult = Game.Create(withName, 75);
            newGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenCreatingANewGame_ItShouldHaveAName()
        {
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.Name.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItDoesNotHaveAnyPlayers()
        {
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.Players.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public void WhenCreatingANewGame_ItHasBallsConfigured()
        {
            var newGameResult = Game.Create("Name 01", 10);
            newGameResult.Value.BallsConfigured.Should()
                .NotBeNull()
                .And.HaveCount(10);

            var ballsToCheck = new List<Ball>() {
                Ball.Create(1, BallLeter.B).Value,
                Ball.Create(2, BallLeter.B).Value,
                Ball.Create(3, BallLeter.I).Value,
                Ball.Create(4, BallLeter.I).Value,
                Ball.Create(5, BallLeter.N).Value,
                Ball.Create(6, BallLeter.N).Value,
                Ball.Create(7, BallLeter.G).Value,
                Ball.Create(8, BallLeter.G).Value,
                Ball.Create(9, BallLeter.O).Value,
                Ball.Create(10, BallLeter.O).Value
            };

            foreach (var ball in ballsToCheck)
                newGameResult.Value.BallsConfigured.Should().Contain(ball);
        }

        [Test]
        public void WhenCreatingANewGame_IfWrongBallsCountIsProvided_ItFails(
            [Values(-5, -4, -1, 0, 1, 4, 6, 22)] short ballsCount)
        {
            var newGameResult = Game.Create("Name 01", ballsCount);
            newGameResult.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenCreatingANewGame_ItDoesNotHaveAnyBallsPlayed()
        {
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.BallsPlayed.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Test]
        public void WhenAddingAValidPlayer_ItWorks()
        {
            var newPlayer = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);

            var newGameResult = Game.Create("Name 01", 75);
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
            var newGameResult = Game.Create("Name 01", 75);
            var result = newGameResult.Value.AddPlayer(null);

            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingAPlayer_WhenGameHasStarted_ItFails()
        {
            var newPlayer = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.Start();

            var result = newGameResult.Value.AddPlayer(newPlayer);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameLogin_ItFails()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 02", PlayerSecurity.Create("login 01", "passwd 02").Value);
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithSameName_ItFails()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 01", PlayerSecurity.Create("login 02", "passwd 02").Value);
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public void WhenAddingPlayersWithDifferentLogins_ItWorks()
        {
            var newPlayer1 = new Player("Player 01", PlayerSecurity.Create("login 01", "passwd 01").Value);
            var newPlayer2 = new Player("Player 02", PlayerSecurity.Create("login 02", "passwd 02").Value);
            var newGameResult = Game.Create("Name 01", 75);
            newGameResult.Value.AddPlayer(newPlayer1);
            var result = newGameResult.Value.AddPlayer(newPlayer2);

            result.IsSuccess.Should().BeTrue();
        }
    }
}