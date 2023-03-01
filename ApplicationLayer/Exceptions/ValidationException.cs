using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Resources.Common;

namespace Mesawer.ApplicationLayer.Exceptions;

/// <summary>
/// An exception that should be thrown if any validation errors have happened.
/// </summary>
[PublicAPI]
public class ValidationException : Exception
{
    public ValidationException()
        : base(SharedRes.ValidationError)
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(string propertyName, string errorMessage)
        : this()
        => Errors = new Dictionary<string, string[]> { [propertyName] = new[] { errorMessage } };

    public IDictionary<string, string[]> Errors { get; protected set; }
}
