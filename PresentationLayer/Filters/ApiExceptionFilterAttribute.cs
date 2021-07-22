using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mesawer.ApplicationLayer.Exceptions;
using Mesawer.ApplicationLayer.Resources.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValidationException = Mesawer.ApplicationLayer.Exceptions.ValidationException;

namespace Mesawer.PresentationLayer.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IConfiguration                              _configuration;
        private readonly ILogger<ApiExceptionFilterAttribute>        _logger;
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilterAttribute(IConfiguration configuration, ILogger<ApiExceptionFilterAttribute> logger)
        {
            _configuration = configuration;
            _logger        = logger;

            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(BadRequestException), HandleBadRequestException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(UnhandledRequestException), HandleUnhandledRequestException },
            };
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);

            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            var type = context.Exception.GetType();

            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            HandleUnknownException(context);
        }

        private static void HandleValidationException(ExceptionContext context)
        {
            var exception = context.Exception as ValidationException;

            foreach (var (_, values) in exception!.Errors)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var matches = Regex.Matches(values[i], "'.*?'");

                    if (!matches.Any()) continue;

                    values[i] = values[i].Replace(matches[0].Value, SharedRes.Entity).Trim();
                }
            }

            var details = new ValidationProblemDetails(exception!.Errors)
            {
                Type   = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title  = "The request is not valid",
                Status = StatusCodes.Status422UnprocessableEntity,
            };

            context.Result = new UnprocessableEntityObjectResult(details);

            context.ExceptionHandled = true;
        }

        private static void HandleBadRequestException(ExceptionContext context)
        {
            var exception = context.Exception as BadRequestException;

            var details = new ProblemDetails
            {
                Type   = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title  = "The request is not valid",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception!.Error
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private static void HandleForbiddenAccessException(ExceptionContext context)
        {
            context.Result = new ForbidResult();

            context.ExceptionHandled = true;
        }

        private static void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            context.Result = new UnauthorizedResult();

            context.ExceptionHandled = true;
        }

        private static void HandleNotFoundException(ExceptionContext context)
        {
            var exception = context.Exception as NotFoundException;

            var details = new ProblemDetails
            {
                Type   = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title  = "The specified resource was not founded",
                Status = StatusCodes.Status404NotFound,
                Detail = exception?.Message,
            };

            context.Result = new NotFoundObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleUnhandledRequestException(ExceptionContext context)
        {
            var currentVersion = _configuration["Version"];
            var version        = context.HttpContext.Request.Headers["Version"];

            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(currentVersion)
                                              || version.Equals(currentVersion))
            {
                var details = new ProblemDetails
                {
                    Type   = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title  = "An error occurred while processing your request.",
                    Status = StatusCodes.Status500InternalServerError,
                };

                context.Result = new ObjectResult(details)
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }
            else
            {
                var details = new ProblemDetails
                {
                    Title  = "The request is sent from an unsupported version",
                    Type   = "https://tools.ietf.org/html/rfc7231#section-6.6.6",
                    Status = StatusCodes.Status505HttpVersionNotsupported,
                };

                context.Result = new ObjectResult(details)
                {
                    StatusCode = StatusCodes.Status505HttpVersionNotsupported,
                };
            }

            context.ExceptionHandled = true;
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            _logger.LogCritical(
                context.Exception,
                "Unhandled exception filtered by ApiException Filter");

            HandleUnhandledRequestException(context);
        }
    }
}
