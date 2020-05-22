namespace WebUI.Models.GamePlayer
{
    public class BallModel
    {
        public string Leter { get; set; }
        public short Number { get; set; }
        public bool IsItPossibleToSelect { get; set; }
        public bool HasPlayerSelectedIt { get; set; }

        public string Name => $"{Leter}{Number}";
    }
}
