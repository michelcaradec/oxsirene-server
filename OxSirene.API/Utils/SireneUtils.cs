using System;
using System.Linq;

namespace OxSirene.API
{
    internal static class SireneUtils
    {
        public const int SirenLength = 9;
        public const int NICLength = 5;
        public const int SiretLength = SirenLength + NICLength;

        // https://fr.wikipedia.org/wiki/Formule_de_Luhn
        public static bool IsLuhn(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return
                text.All(char.IsDigit)
                && text.Reverse()
                    .Select(c => c - 48)
                    .Select((thisNum, i) =>
                        i % 2 == 0
                        ? thisNum
                        : ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
                    ).Sum() % 10 == 0;
        }

        private static bool IsDigit(string text) => text?.All(char.IsDigit) ?? false;

        public static bool IsSiren(string text) => text?.Length == SirenLength && IsLuhn(text);

        public static bool IsSiret(string text) => text?.Length == SiretLength && IsLuhn(text);
    }
}