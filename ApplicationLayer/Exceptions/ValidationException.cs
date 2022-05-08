using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Mesawer.ApplicationLayer.Resources.Common;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if any validation errors have happened.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base(SharedRes.ValidationError)
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(params ValidationFailure[] failures)
        : this(failures.AsEnumerable()) { }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
        => Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

    public ValidationException(string propertyName, string errorMessage)
        : this()
        => Errors = new Dictionary<string, string[]> { [propertyName] = new[] { errorMessage } };

    public IDictionary<string, string[]> Errors { get; }
}
