namespace Mesawer.ApplicationLayer
{
    public static class Constants
    {
        #region General

        public const string EnvironmentVariableName = "DOTNET_ENVIRONMENT";
        public const string SpaProxyBaseUrlName = "SPA_BASE_URL";

        public const int MaxUrlLength = 65519;

        #endregion

        #region Validation

        public const int IdMaxLength   = 32;
        public const int MacMaxLength  = 17;
        public const int NameMaxLength = 32;
        public const int UriMaxLength  = 38;

        // User
        public const int MinPassword     = 8;
        public const int MaxPassword     = 100;
        public const int MaxUserName     = 50;
        public const int MaxEmailAddress = 256;
        public const int MinPhoneNumber  = 5;
        public const int MaxPhoneNumber  = 24;

        #endregion
    }
}
