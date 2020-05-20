using Core;

namespace WebUI.Models.GameAdmon
{
    public class BallModel
    {
        public string Letter { get; set; }
        public int Number { get; set; }
        public bool WasItPlayed { get; set; }

        public Ball Entity { get; set; }
    }
}
