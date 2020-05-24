namespace WebUI.Models.GamePlayer
{
    public class BallModel
    {
        public string Leter { get; set; }
        public short Number { get; set; }
        public bool IsItPossibleToSelect { get; set; }
        public bool HasPlayerSelectedIt { get; set; }
        public bool IsItSpecialCharacterBall { get; set; } = false;

        public string Name => $"{Leter}{Number}";

        public static BallModel CreateSpecialCharacterBall() =>
            new BallModel { IsItSpecialCharacterBall = true, HasPlayerSelectedIt = true };
    }
}
