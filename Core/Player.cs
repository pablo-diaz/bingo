﻿using System.Linq;
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

        private Player(string name)
        {
            this.Name = name;
            this._boards = new HashSet<Board>();
        }

        #endregion

        #region Builders

        public static Result<Player> Create(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Result.Failure<Player>("Wrong name");

            return Result.Ok(new Player(name));
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
            
            this._boards.Remove(board);

            return Result.Ok();
        }

        internal void CopyInfoFromPlayer(Player anotherPlayer)
        {
            this.Name = anotherPlayer.Name;
            foreach(var board in anotherPlayer.Boards)
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