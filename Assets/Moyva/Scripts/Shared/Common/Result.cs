using System;

namespace Kruty1918.Moyva.Shared.Common
{
    /// <summary>
    /// Result type for expected domain failures without exceptions.
    /// </summary>
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public DomainError Error { get; }

        public bool IsFailure => !IsSuccess;

        private Result(bool isSuccess, DomainError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new Result(true, DomainError.None);
        }

        public static Result Fail(DomainErrorCode code, string message)
        {
            if (code == DomainErrorCode.None)
                throw new ArgumentOutOfRangeException(nameof(code), "Failure code cannot be None.");

            return new Result(false, new DomainError(code, message));
        }
    }

    /// <summary>
    /// Result type carrying a value for successful domain operations.
    /// </summary>
    public readonly struct Result<T>
    {
        private readonly T _value;

        public bool IsSuccess { get; }
        public DomainError Error { get; }
        public bool IsFailure => !IsSuccess;

        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException("Cannot access Value for failed Result.");

                return _value;
            }
        }

        private Result(T value)
        {
            _value = value;
            IsSuccess = true;
            Error = DomainError.None;
        }

        private Result(DomainError error)
        {
            _value = default;
            IsSuccess = false;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Fail(DomainErrorCode code, string message)
        {
            if (code == DomainErrorCode.None)
                throw new ArgumentOutOfRangeException(nameof(code), "Failure code cannot be None.");

            return new Result<T>(new DomainError(code, message));
        }
    }
}
