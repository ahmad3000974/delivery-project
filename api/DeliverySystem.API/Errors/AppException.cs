namespace DeliverySystem.API.Errors;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}

public sealed class NotFoundException(string message) : AppException(message);

public sealed class ConflictException(string message) : AppException(message);

public sealed class BusinessRuleException(string message) : AppException(message);
