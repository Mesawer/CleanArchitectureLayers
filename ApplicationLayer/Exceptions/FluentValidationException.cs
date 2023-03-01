using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Exceptions;

[PublicAPI]
public class FluentValidationException : ValidationException
{
    public FluentValidationException(params ValidationFailure[] failures)
        : this(failures.AsEnumerable()) { }

    public FluentValidationException(IEnumerable<ValidationFailure> failures)
        => Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
}
