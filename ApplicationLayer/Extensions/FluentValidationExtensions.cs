using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Mesawer.ApplicationLayer.Enums;
using Mesawer.ApplicationLayer.Models;
using Mesawer.ApplicationLayer.Resources.Common;
using Mesawer.DomainLayer.ValueObjects;
using Microsoft.AspNetCore.Http;
using static Mesawer.ApplicationLayer.Constants;

namespace Mesawer.ApplicationLayer.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> Id<T>(this IRuleBuilder<T, string> builder)
            => builder
                .NotEmpty()
                .MaximumLength(IdMaxLength);

        public static IRuleBuilderOptions<T, int> Id<T>(this IRuleBuilder<T, int> builder)
            => builder.GreaterThan((int) default);

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> builder)
            => builder
                .Length(MinPhoneNumber, MaxPhoneNumber)
                .WithMessage(
                    $"Phone number must be greater than {MaxPhoneNumber} and less than {MinPhoneNumber} digits")
                .Matches(Regexes.PhoneNumber)
                .WithMessage(
                    "Not valid phone number! Please enter a valid phone number with English digits.");

        public static IRuleBuilderOptions<T, string> UserName<T>(this IRuleBuilder<T, string> builder)
            => builder
                .Matches(Regexes.UserName)
                .WithMessage(
                    "Invalid Username. You can only use a combination of English letters, digits, ., or _ And must contains 4 to 30 characters.");

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
            FileType fileType) where TIFormFile : IFormFile
        {
            var contentTypes = fileType switch
            {
                FileType.Picture   => new[] { "jpg", "jpeg", "png" },
                FileType.Audio     => new[] { "mp3", "wav", "ase", "webm" },
                FileType.Video     => new[] { "mp4", "webm" },
                FileType.Pdf       => new[] { "pdf" },
                FileType.Document  => new[] { "pdf" },
                FileType.Text      => new[] { "txt" },
                FileType.Xml       => new[] { "xml" },
                FileType.Html      => new[] { "html" },
                FileType.Svg       => new[] { "svg" },
                FileType.Utilities => new[] { "*/*" },
                _                  => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
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
                    str.RuleFor(s => s.FirstName)
                        .NotEmpty();

                    str.RuleFor(s => s.LastName)
                        .NotEmpty();
                });

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
                .NotNull()
                .ChildRules(str =>
                {
                    str.RuleFor(s => s.Ar).MaximumLength(maxLength);

                    str.RuleFor(s => s.En).MaximumLength(maxLength);
                });

        public static IRuleBuilder<T, LocalizedStringDto> UniqueLocalizedString<T, TEntity>(
            this IRuleBuilder<T, LocalizedStringDto> builder,
            IQueryable<TEntity> queryable,
            string propertyName = null)
            => builder
                .ChildRules(str =>
                {
                    Func<string, bool> validateAr = DomainLayer.ValueObjects.LocalizedString.IsValidAr;
                    Func<string, bool> validateEn = DomainLayer.ValueObjects.LocalizedString.IsValidEn;

                    var property = propertyName is null
                        ? typeof(TEntity)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.PropertyType == typeof(LocalizedString))
                        : typeof(TEntity)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.Name == propertyName);

                    if (property is null) return;

                    str.RuleFor(s => s)
                        .MustAsync((s, ct)
                            => queryable.LocalizedStringAllNotEquals(
                                property,
                                nameof(LocalizedStringDto.Ar),
                                s.Ar,
                                ct))
                        .When(s => s is not null && validateAr(s.Ar))
                        .WithName($"{nameof(LocalizedStringDto.Ar)}")
                        .WithMessage(SharedRes.NameAlreadyExists);

                    str.RuleFor(s => s)
                        .MustAsync((s, ct)
                            => queryable.LocalizedStringAllNotEquals(
                                property,
                                nameof(LocalizedStringDto.En),
                                s.En,
                                ct))
                        .When(s => s is not null && validateEn(s.En))
                        .WithName($"{nameof(LocalizedStringDto.En)}")
                        .WithMessage(SharedRes.NameAlreadyExists);
                });

        public static IRuleBuilder<T, Localizable<TKey>> UniqueLocalizedString<T, TEntity, TKey>(
            this IRuleBuilder<T, Localizable<TKey>> builder,
            IQueryable<TEntity> queryable,
            string keyPropertyName,
            string propertyName = null)
            => builder
                .ChildRules(str =>
                {
                    Func<string, bool> validateAr = DomainLayer.ValueObjects.LocalizedString.IsValidAr;
                    Func<string, bool> validateEn = DomainLayer.ValueObjects.LocalizedString.IsValidEn;

                    var keyProperty = keyPropertyName is not null
                        ? typeof(TEntity)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.Name == keyPropertyName)
                        : null;

                    var property = propertyName is null
                        ? typeof(TEntity)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.PropertyType == typeof(LocalizedString))
                        : typeof(TEntity)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(p => p.Name == propertyName);

                    if (property is null) return;

                    if (keyProperty is null) return;

                    str.RuleFor(s => s)
                        .MustAsync((s, ct)
                            => queryable.LocalizedStringOnlyNotEquals(keyProperty,
                                property,
                                nameof(LocalizedStringDto.Ar),
                                s.Key,
                                s.String.Ar,
                                ct))
                        .When(s => s.String is not null && validateAr(s.String.Ar))
                        .WithName($"{property.Name}.{nameof(LocalizedStringDto.Ar)}")
                        .WithMessage(SharedRes.NameAlreadyExists);

                    str.RuleFor(s => s)
                        .MustAsync((s, ct)
                            => queryable.LocalizedStringOnlyNotEquals(keyProperty,
                                property,
                                nameof(LocalizedStringDto.En),
                                s.Key,
                                s.String.En,
                                ct))
                        .When(s => s.String is not null && validateEn(s.String.En))
                        .WithName($"{property.Name}.{nameof(LocalizedStringDto.En)}")
                        .WithMessage(SharedRes.NameAlreadyExists);
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
}
