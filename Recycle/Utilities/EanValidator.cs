namespace Recycle.Api.Utilities
{
    public static class EanValidator
    {
        public static bool IsValidEAN13(string ean)
        {
            if (ean.Length != 13 || !ean.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = ean[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int calculatedCheckDigit = (10 - (sum % 10)) % 10;
            int actualCheckDigit = ean[12] - '0';

            return calculatedCheckDigit == actualCheckDigit;
        }
    }
}
