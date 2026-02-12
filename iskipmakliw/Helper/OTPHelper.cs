namespace iskipmakliw.Helper
{
    public class OTPHelper
    {
        private static Random random = new Random();

        public static string GenerateOTP()
        {
            return random.Next(1000, 9999).ToString();
        }

        public static bool IsOTPExpired(DateTime? createdAt, int expirationMinutes = 10)
        {
            if (!createdAt.HasValue)
                return true;

            return DateTime.UtcNow > createdAt.Value.AddMinutes(expirationMinutes);
        }

        public static bool CanResendOTP(DateTime? lastSentAt, int cooldownMinutes = 2)
        {
            if (!lastSentAt.HasValue)
                return true;

            return DateTime.UtcNow >= lastSentAt.Value.AddMinutes(cooldownMinutes);
        }

        public static TimeSpan GetRemainingCooldown(DateTime? lastSentAt, int cooldownMinutes = 2)
        {
            if (!lastSentAt.HasValue)
                return TimeSpan.Zero;

            var nextAllowedTime = lastSentAt.Value.AddMinutes(cooldownMinutes);
            var remaining = nextAllowedTime - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
