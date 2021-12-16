namespace Mesawer.ApplicationLayer
{
    public static class Regexes
    {
        /// <summary>
        /// Represent username where
        /// * allowed character is a to z, 0 to 9, _, .
        /// * minimum of 4 characters and maximum of 30
        /// * _ and . can not repeated ex: (..)
        /// * can not be suffix or prefix
        /// </summary>
        public const string UserName = @"^[a-zA-Z0-9]([._](?![._])|[a-zA-Z0-9]){1,28}[a-zA-Z0-9]$";

        public const string Email = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})";

        public const string Mac = "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";

        /// <summary>
        /// Represent URL with optional  protocol
        /// </summary>
        public const string Url =
            @"^((https?|ftp|file):\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$";

        /// <summary>
        /// Represent HTTP URL where https://www. is optional
        /// </summary>
        public const string HttpUrl =
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";

        public const string Date =
            @"^((((0?[1-9]|[12]\d|3[01])[\.\-\/](0?[13578]|1[02])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|((0?[1-9]|[12]\d|30)[\.\-\/](0?[13456789]|1[012])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|((0?[1-9]|1\d|2[0-8])[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|(29[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00)))|(((0[1-9]|[12]\d|3[01])(0[13578]|1[02])((1[6-9]|[2-9]\d)?\d{2}))|((0[1-9]|[12]\d|30)(0[13456789]|1[012])((1[6-9]|[2-9]\d)?\d{2}))|((0[1-9]|1\d|2[0-8])02((1[6-9]|[2-9]\d)?\d{2}))|(2902((1[6-9]|[2-9]\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00)))) ?((20|21|22|23|[01]\d|\d)(([:.][0-5]\d){1,2}))?$";

        public const string Decimal = @"^((-?[1-9]+)|[0-9]+)(\.?|\,?)([0-9]*)$";

        public const string Hex = "^#?([a-f0-9]{6}|[a-f0-9]{3})$";

        public const string Integer = "^((-?[1-9]+)|[0-9]+)$";

        public const string Password = @"^.*(?=.{10,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$";

        public const string Tag = @"^<([a-z1-6]+)([^<]+)*(?:>(.*)<\/\1>| *\/>)$";

        public const string Time = @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$";

        public const string ZipCode = @"^[a-zA-Z0-9][a-zA-Z0-9\- ]{0,10}[a-zA-Z0-9]$";

        public const string PhoneNumber = @"^\+?(\d{1,3}) (\d{10})$";

        public const string ArabicName =
            @"[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufbc1]|[\ufbd3-\ufd3f]|[\ufd50-\ufd8f]|[\ufd92-\ufdc7]|[\ufe70-\ufefc]|[\uFDF0-\uFDFD]";

        public const string EnglishArabicName =
            @"[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z]+[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z-_]*";
    }
}
