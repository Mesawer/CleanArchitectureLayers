using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mesawer.DomainLayer.Exceptions;
using Mesawer.DomainLayer.Models;
using AmbiguousMatchException = Mesawer.DomainLayer.Exceptions.AmbiguousMatchException;

namespace Mesawer.DomainLayer.ValueObjects
{
    public class LocalizedString : ValueObject
    {
        private LocalizedString() { }

        public LocalizedString(string ar, string en)
        {
            Ar = ar;
            En = en;
        }

        private string _ar;

        public string Ar
        {
            get => _ar;
            set
            {
                var str = value?.Trim() ?? throw new InvalidValueException();

                if (!IsValidAr(str)) throw new InvalidValueException(value);

                _ar = value;
            }
        }

        private string _en;

        public string En
        {
            get => _en;
            set
            {
                var str = value?.Trim() ?? throw new InvalidValueException();

                if (!IsValidEn(str)) throw new InvalidValueException(value);

                _en = value;
            }
        }

        public static implicit operator string(LocalizedString str) => str?.ToString();
        public static explicit operator LocalizedString((string ar, string en) values) => new(values.ar, values.en);

        public override string ToString()
        {
            var culture = CultureInfo.CurrentCulture;
            var lang    = culture.IsNeutralCulture ? culture.Name : culture.Parent.Name;

            var property = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name.ToLower() == lang);

            if (property is null) throw new UnsupportedLanguageException(lang);

            if (!property.CanRead || property.PropertyType != typeof(string))
                throw new AmbiguousMatchException("Language match can't be read");

            return property.GetValue(this)?.ToString() ?? Ar;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Ar;
            yield return En;
        }

        public static bool IsValidAr(string ar)
        {
            var regex = new Regex(
                @"^[\u0600-\u06ff\u0750-\u077f\ufb50-\ufbc1\ufbd3-\ufd3f\ufd50-\ufd8f\ufd92-\ufdc7\ufe70-\ufefc\uFDF0-\uFDFD\s\d\p{P}\p{S}]+$");

            return ar is not null && regex.IsMatch(ar.Trim());
        }

        public static bool IsValidEn(string en)
        {
            var regex = new Regex(@"^[a-zA-Z\s\d\p{P}\p{S}]+$");

            return en is not null && regex.IsMatch(en.Trim());
        }
    }
}
