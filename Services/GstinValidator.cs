namespace ERPDemo.Services
{
    public static class GstinValidator
    {
        private static readonly char[] Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        /// <summary>
        /// Validates GSTIN format + state code + checksum (offline, no API call).
        /// </summary>
        public static bool IsValid(string? gstin)
        {
            if (string.IsNullOrWhiteSpace(gstin)) return false;
            gstin = gstin.Trim().ToUpper();

            if (gstin.Length != 15) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(gstin, @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$"))
                return false;

            // Verify checksum (15th character)
            return CalculateChecksum(gstin.Substring(0, 14)) == gstin[14];
        }

        /// <summary>
        /// Extracts the 2-digit state code from GSTIN.
        /// </summary>
        public static string? GetStateCode(string? gstin)
        {
            if (string.IsNullOrWhiteSpace(gstin) || gstin.Length < 2) return null;
            return gstin.Substring(0, 2);
        }

        /// <summary>
        /// Extracts the 10-char PAN embedded in GSTIN.
        /// </summary>
        public static string? GetPan(string? gstin)
        {
            if (string.IsNullOrWhiteSpace(gstin) || gstin.Length < 12) return null;
            return gstin.Substring(2, 10);
        }

        // GST checksum algorithm (Luhn mod 36)
        private static char CalculateChecksum(string first14)
        {
            int sum = 0;
            for (int i = 0; i < first14.Length; i++)
            {
                int digit = Array.IndexOf(Base36, first14[i]);
                int factor = (i % 2 == 0) ? 1 : 2;
                int product = digit * factor;
                sum += (product / 36) + (product % 36);
            }
            int remainder = sum % 36;
            int checkDigit = (36 - remainder) % 36;
            return Base36[checkDigit];
        }
    }
}