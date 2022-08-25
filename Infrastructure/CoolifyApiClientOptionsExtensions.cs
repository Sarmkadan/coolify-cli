using System;
using System.Diagnostics.CodeAnalysis;

namespace CoolifyCli.Infrastructure
{
    /// <summary>
    /// Provides extension methods for <see cref="CoolifyApiClientOptions"/> configuration.
    /// </summary>
    [SuppressMessage("Design", "CA1053:Static holder types should not have constructors", Justification = "Extension class pattern")]
    public static class CoolifyApiClientOptionsExtensions
    {
        /// <summary>
        /// Validates that all timeout values are positive integers.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when any timeout is less than or equal to zero.</exception>
        public static void ValidateTimeouts(this CoolifyApiClientOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.GetTimeoutSeconds <= 0)
            {
                throw new ArgumentException("Get timeout must be positive", nameof(options.GetTimeoutSeconds));
            }

            if (options.PostTimeoutSeconds <= 0)
            {
                throw new ArgumentException("Post timeout must be positive", nameof(options.PostTimeoutSeconds));
            }

            if (options.PutTimeoutSeconds <= 0)
            {
                throw new ArgumentException("Put timeout must be positive", nameof(options.PutTimeoutSeconds));
            }

            if (options.DeleteTimeoutSeconds <= 0)
            {
                throw new ArgumentException("Delete timeout must be positive", nameof(options.DeleteTimeoutSeconds));
            }
        }

        /// <summary>
        /// Creates a deep clone of the options with the same timeout values.
        /// </summary>
        /// <param name="options">The options to clone.</param>
        /// <returns>New instance with identical timeout values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        public static CoolifyApiClientOptions Clone(this CoolifyApiClientOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return new CoolifyApiClientOptions
            {
                GetTimeoutSeconds = options.GetTimeoutSeconds,
                PostTimeoutSeconds = options.PostTimeoutSeconds,
                PutTimeoutSeconds = options.PutTimeoutSeconds,
                DeleteTimeoutSeconds = options.DeleteTimeoutSeconds
            };
        }

        /// <summary>
        /// Sets all timeout values to the same specified value.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <param name="timeoutSeconds">The timeout value to apply to all operations.</param>
        /// <returns>The modified options instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        public static CoolifyApiClientOptions SetAllTimeouts(this CoolifyApiClientOptions options, int timeoutSeconds)
        {
            ArgumentNullException.ThrowIfNull(options);

            options.GetTimeoutSeconds = timeoutSeconds;
            options.PostTimeoutSeconds = timeoutSeconds;
            options.PutTimeoutSeconds = timeoutSeconds;
            options.DeleteTimeoutSeconds = timeoutSeconds;

            return options;
        }
    }
}