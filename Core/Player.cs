using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public class Player
    {
        #region Properties

        public string Name { get; private set; }
        
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
            return false;
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

        private Player(string name, Board defaultBoard)
        {
            this.Name = name;
            this._boards = new HashSet<Board>() { defaultBoard };
        }

        #endregion

        #region Builders

        internal static Result<Player> Create(string name, HashSet<Ball> withBallSet,
            int withNBallsPerColumn, GameType gameType)
        {
            var result = ValidateName(name);
            if (result.IsFailure)
                return Result.Failure<Player>(result.Error);

            var defaultBoardResult = Board.Create(withBallSet, withNBallsPerColumn, gameType);
            if (defaultBoardResult.IsFailure)
                return Result.Failure<Player>(defaultBoardResult.Error);

            return new Player(name, defaultBoardResult.Value);
        }

        #endregion

        #region Internal methods

        internal void AddBoard(Board board)
        {
            this._boards.Add(board);
        }

        internal Result RemoveBoard(Board board)
        {
            if (!this._boards.Contains(board))
                return Result.Failure("Board does not exist in Player's boards");
            
            if(this._boards.Count == 1)
                return Result.Failure("You cannot remove this last board");

            this._boards.Remove(board);

            return Result.Success();
        }

        internal Result SetInfo(string name)
        {
            var result = ValidateName(name);
            if (result.IsFailure)
                return Result.Failure<Player>(result.Error);

            this.Name = name;

            return Result.Success();
        }

        internal void PlayBall(Ball ballToPlay)
        {
            foreach(var board in this._boards)
                board.PlayBall(ballToPlay);
        }

        #endregion

        #region Validators

        private static Result ValidateName(string name) =>
            string.IsNullOrEmpty(name)
            ? Result.Failure("Please provide a valid Name")
            : Result.Success();

        #endregion
    }
}