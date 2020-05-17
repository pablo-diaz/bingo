using System.Linq;
using System.Collections.Generic;

namespace Core
{
    public class Player
    {
        #region Properties

        public string Name { get; }
        public PlayerSecurity Security { get; }
        
        private HashSet<Board> _boards { get; }
        public IReadOnlyCollection<Board> Boards { get => this._boards.ToList(); }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            var other = obj as Player;
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Name == other.Name;
        }

        public static bool operator ==(Player a, Player b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Player a, Player b) => !(a == b);

        public override int GetHashCode() => Name.GetHashCode();

        #endregion

        #region Constructor

        public Player(string name, PlayerSecurity security)
        {
            this.Name = name;
            this.Security = security;

            this._boards = new HashSet<Board>();
        }

        #endregion

        #region Internal methods

        internal void AddBoard(Board board)
        {
            this._boards.Add(board);
        }

        internal void PlayBall(Ball ballToPlay)
        {
            foreach(var board in this._boards)
                board.PlayBall(ballToPlay);
        }

        #endregion
    }
}