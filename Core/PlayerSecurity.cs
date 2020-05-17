using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Core
{
    public class PlayerSecurity: ValueObject
    {
        public string Login { get; }
        public string Password { get; }

        private PlayerSecurity(string login, string password)
        {
            this.Login = login;
            this.Password = password;
        }

        public static Result<PlayerSecurity> Create(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
                return Result.Failure<PlayerSecurity>("Login cannot be null or empty");

            if (string.IsNullOrEmpty(password))
                return Result.Failure<PlayerSecurity>("Password cannot be null or empty");

            return Result.Ok(new PlayerSecurity(login, password));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Login;
        }
    }
}