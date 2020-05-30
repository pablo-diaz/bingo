using Core;

using NUnit.Framework;

using FluentAssertions;

namespace UnitTests
{
    public class PlayerTests
    {
        [Test]
        public void CreatingAValidPlayer_Works()
        {
            var newPlayerResult = Player.Create("Player 01");
            newPlayerResult.IsSuccess.Should().BeTrue();
        }

        [Test]
        public void CreatingAPlayer_WhenProvidingWrongName_ItFails(
            [Values(null, "")] string withName)
        {
            var newPlayerResult = Player.Create(withName);
            newPlayerResult.IsFailure.Should().BeTrue();
        }
    }
}
