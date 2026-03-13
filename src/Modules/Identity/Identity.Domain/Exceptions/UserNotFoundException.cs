namespace Identity.Domain.Exceptions
{
    public class UserNotFoundException : IdentityException
    {
        public UserNotFoundException(Guid userId)
            : base("USER_NOT_FOUND", $"User with id '{userId}' was not found.") { }

        public UserNotFoundException(string email)
            : base("USER_NOT_FOUND", $"User with email '{email}' was not found.") { }
    }
}
