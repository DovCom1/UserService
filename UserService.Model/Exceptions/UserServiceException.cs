namespace UserService.Model.Exceptions;

public class UserServiceException(string error, int statusCode = 400) : Exception(error)
{
    public int StatusCode { get; } = statusCode;
    public string Error { get; } = error;
}