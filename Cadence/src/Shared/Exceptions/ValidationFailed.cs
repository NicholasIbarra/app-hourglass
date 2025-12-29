namespace SharedKernel.Exceptions;

public class ValidationFailed
{
    public ValidationFailed(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
