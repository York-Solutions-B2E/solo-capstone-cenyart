
namespace Shared.Exceptions;

public class ValidationException(Dictionary<string, string[]> errors) : Exception("One or more validation failures have occurred.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string propertyName, string errorMessage)
        : this(new Dictionary<string, string[]> { { propertyName, new[] { errorMessage } } })
    {
    }
}
