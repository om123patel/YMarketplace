namespace Identity.Domain.Exceptions
{
    public class InvalidCredentialsException : IdentityException
    {
        public InvalidCredentialsException()
            : base("INVALID_CREDENTIALS", "Email or password is incorrect.") { }
    }
}
