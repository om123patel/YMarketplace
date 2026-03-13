namespace Identity.Domain.Exceptions
{
    public class UserAlreadyExistsException : IdentityException
    {
        public UserAlreadyExistsException(string email)
            : base("USER_ALREADY_EXISTS", $"A user with email '{email}' already exists.") { }
    }
}
