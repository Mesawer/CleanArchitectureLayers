namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Models
{
    public class IdentityResult
    {
        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        public string[] Errors { get; set; }

        public static IdentityResult Success { get; } = new() { Succeeded = true };

        public static IdentityResult Failed(params string[] errors)
            => new()
            {
                Succeeded = false,
                Errors    = errors
            };
    }
}
