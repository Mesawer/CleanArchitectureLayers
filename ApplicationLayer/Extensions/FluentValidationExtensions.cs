using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using JetBrains.Annotations;
using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Models;
using Mesawer.ApplicationLayer.Resources.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class FluentValidationExtensions
{
    public static string GetJsonPropertyName<T>(string name)
    {
        var jsonPropertyName =
            typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name == name)
                ?.GetCustomAttribute<JsonPropertyAttribute>()
                ?.PropertyName;

        if (jsonPropertyName is null)
            throw new ArgumentException($"Type {nameof(T)} does not contain a property named {name}");

        return jsonPropertyName;
    }

    public static IRuleBuilderOptions<T, TProperty> WithJsonPropertyName<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder,
        Func<T, string> propertyName)
        => ruleBuilder.WithName(c => GetJsonPropertyName<T>(propertyName(c)));

    public static IRuleBuilderOptions<T, IList<TElement>> MinElements<T, TElement>(
        this IRuleBuilder<T, IList<TElement>> builder,
        int size)
        => builder
            .Must(
                (_, list, context) =>
                {
                    context.MessageFormatter
                        .AppendArgument("MinElements", size)
                        .AppendArgument("TotalElements", list.Count);

                    return list.Count >= size;
                })
            .WithMessage(
                "{PropertyName} must contain at least {MinElements} items. The {PropertyName} contains {TotalElements} element");

    public static IRuleBuilderOptions<T, IList<TElement>> MaxElements<T, TElement>(
        this IRuleBuilder<T, IList<TElement>> builder,
        int size)
        => builder
            .Must(
                (_, list, context) =>
                {
                    context.MessageFormatter
                        .AppendArgument("MaxElements", size)
                        .AppendArgument("TotalElements", list.Count);

                    return list.Count <= size;
                })
            .WithMessage(
                "{PropertyName} must contain at least {MaxElements} items. The {PropertyName} contains {TotalElements} element");

    public static IRuleBuilderOptions<T, TIFormFile> MinSize<T, TIFormFile>(
        this IRuleBuilder<T, TIFormFile> builder,
        int sizeInBytes) where TIFormFile : IFormFile
        => builder
            .Must(
                (_, formFile, context) =>
                {
                    context.MessageFormatter.AppendArgument("MinSize", sizeInBytes / 1048576);
                    // Check the file length.
                    // This check doesn't catch files that only have a BOM as their content.
                    return formFile.Length >= sizeInBytes;
                })
            .WithMessage("File must be greater than {MinSize:N1} MB");

    public static IRuleBuilderOptions<T, TIFormFile> MaxSize<T, TIFormFile>(
        this IRuleBuilder<T, TIFormFile> builder,
        int sizeInBytes) where TIFormFile : IFormFile
        => builder
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("MaxSize", sizeInBytes / 1048576);
                    return file.Length <= sizeInBytes;
                })
            .WithMessage("File must be less than {MaxSize:N1} MB");

    public static IRuleBuilderOptions<T, TIFormFile> ContentTypes<T, TIFormFile>(
        this IRuleBuilder<T, TIFormFile> builder,
        ContentType contentType) where TIFormFile : IFormFile
    {
        var contentTypes = contentType switch
        {
            ContentType.Picture   => new[] { "jpg", "jpeg", "png" },
            ContentType.Audio     => new[] { "mp3", "wav", "ase", "webm" },
            ContentType.Video     => new[] { "mp4", "webm" },
            ContentType.Pdf       => new[] { "pdf" },
            ContentType.Document  => new[] { "pdf" },
            ContentType.Text      => new[] { "txt" },
            ContentType.Xml       => new[] { "xml" },
            ContentType.Html      => new[] { "html" },
            ContentType.Svg       => new[] { "svg" },
            ContentType.Utilities => new[] { "*/*" },
            _                  => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
        };

        return builder.ContentTypes(contentTypes);
    }

    public static IRuleBuilderOptions<T, TIFormFile> ContentTypes<T, TIFormFile>(
        this IRuleBuilder<T, TIFormFile> builder,
        params string[] contentTypes) where TIFormFile : IFormFile
        => builder
            .Must(
                (_, file, context) =>
                {
                    if (file is null) return true;

                    context.MessageFormatter.AppendArgument(
                        "ContentTypes",
                        contentTypes.Aggregate((p, c) => $"'{p}', '{c}'"));

                    if (contentTypes.Contains("*/*")) return true;

                    return contentTypes
                        .Any(contentType
                            => file.ContentType.Contains(contentType, StringComparison.OrdinalIgnoreCase));
                })
            .WithMessage("File type is not allowed. allowed types is {ContentTypes}.");

    public static IRuleBuilder<T, FullNameDto> FullName<T>(
        this IRuleBuilder<T, FullNameDto> builder)
        => builder
            .NotNull()
            .ChildRules(str =>
            {
                str.RuleFor(s => s!.FirstName).NotEmpty();

                str.RuleFor(s => s!.LastName).NotEmpty();
            })
            .When(s => s is not null);

    public static IRuleBuilder<T, LocalizedStringDto> LocalizedString<T>(
        this IRuleBuilder<T, LocalizedStringDto> builder,
        int maxLength = NameMaxLength)
        => builder
            .NotNull()
            .ChildRules(str =>
            {
                str.RuleFor(s => s.Ar)
                    .NotNull()
                    .Must(DomainLayer.ValueObjects.LocalizedString.IsValidAr)
                    .WithMessage(SharedRes.InvalidAr)
                    .MaximumLength(maxLength);

                str.RuleFor(s => s.En)
                    .NotNull()
                    .Must(DomainLayer.ValueObjects.LocalizedString.IsValidEn)
                    .WithMessage(SharedRes.InvalidEn)
                    .MaximumLength(maxLength);
            });

    public static IRuleBuilder<T, LocalizedStringDto> WeakLocalizedString<T>(
        this IRuleBuilder<T, LocalizedStringDto> builder,
        int maxLength = NameMaxLength)
        => builder
            .NotNull()
            .ChildRules(str =>
            {
                str.RuleFor(s => s.Ar)
                    .NotEmpty()
                    .MaximumLength(maxLength);

                str.RuleFor(s => s.En)
                    .NotEmpty()
                    .MaximumLength(maxLength);
            });

    public static IRuleBuilder<T, LocalizedStringDto> NullableWeakLocalizedString<T>(
        this IRuleBuilder<T, LocalizedStringDto> builder,
        int maxLength = NameMaxLength)
        => builder
            .ChildRules(str =>
            {
                str.RuleFor(s => s.Ar).MaximumLength(maxLength);

                str.RuleFor(s => s.En).MaximumLength(maxLength);
            });

    public static IRuleBuilder<T, LocalizedStringDto> NullableLocalizedString<T>(
        this IRuleBuilder<T, LocalizedStringDto> builder)
        => builder
            .ChildRules(str =>
            {
                str.When(c => c.Ar is not null,
                    () => str.RuleFor(s => s.Ar)
                        .Must(DomainLayer.ValueObjects.LocalizedString.IsValidAr)
                        .WithMessage(SharedRes.InvalidAr));

                str.When(c => c.En is not null,
                    () => str.RuleFor(s => s.En)
                        .Must(DomainLayer.ValueObjects.LocalizedString.IsValidEn)
                        .WithMessage(SharedRes.InvalidAr));
            });
}
