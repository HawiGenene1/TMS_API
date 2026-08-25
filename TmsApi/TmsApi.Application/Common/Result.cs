namespace TmsApi.Application.Common;

// A Result is either Success (with a value) or Failure (with an error)
public readonly record struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;
    
    public bool IsSuccess { get; }

    private Result(TValue value)
    {
        _value = value;
        _error = default;
        IsSuccess = true;
    }

    private Result(TError error)
    {
        _value = default;
        _error = error;
        IsSuccess = false;
    }

    // Factory methods for creating Results
    public static Result<TValue, TError> Success(TValue value) => new(value);
    public static Result<TValue, TError> Failure(TError error) => new(error);

    // Accessors (throw if you access the wrong one)
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Result is failure; call Match instead of Value.");
    
    public TError Error => !IsSuccess 
        ? _error! 
        : throw new InvalidOperationException("Result is success; call Match instead of Error.");

    // Safely handle both cases
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<TError, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}
