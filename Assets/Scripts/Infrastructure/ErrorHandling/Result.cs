using System;

namespace MusicOM.Infrastructure.ErrorHandling
{
    public readonly struct Result<T>
    {
        public T Value { get; }
        public string Error { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(T value, string error, bool isSuccess)
        {
            Value = value;
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result<T> Success(T value) => new(value, null, true);
        public static Result<T> Failure(string error) => new(default, error, false);

        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return IsSuccess
                ? Result<TNew>.Success(mapper(Value))
                : Result<TNew>.Failure(Error);
        }

        public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        {
            return IsSuccess ? binder(Value) : Result<TNew>.Failure(Error);
        }

        public T GetValueOrDefault(T defaultValue = default)
        {
            return IsSuccess ? Value : defaultValue;
        }

        public void Match(Action<T> onSuccess, Action<string> onFailure)
        {
            if (IsSuccess)
                onSuccess(Value);
            else
                onFailure(Error);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }
    }

    public readonly struct Result
    {
        public string Error { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(string error, bool isSuccess)
        {
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result Success() => new(null, true);
        public static Result Failure(string error) => new(error, false);

        public static implicit operator Result(string error) => Failure(error);

        public void Match(Action onSuccess, Action<string> onFailure)
        {
            if (IsSuccess)
                onSuccess();
            else
                onFailure(Error);
        }
    }
}
