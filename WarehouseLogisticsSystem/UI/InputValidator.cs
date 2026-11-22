using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WarehouseLogisticsSystem.UI
{
    // Centralny walidator danych wejściowych z konsoli (chroni przed błędnymi formatami i typami)
    public static class InputValidator
    {
        private static readonly CultureInfo PolishCulture = new("pl-PL");

        // Format SKU: 2-5 liter, myślnik, 2-6 znaków, myślnik, 2-6 znaków (np. ALU-PRF-200, PLC-MOD-S7, POW-RAL-7016)
        private static readonly Regex SkuRegex = new(@"^[A-Za-z]{2,5}-[A-Za-z0-9]{2,6}-[A-Za-z0-9]{2,6}$", RegexOptions.Compiled);

        // Format lokalizacji regałowej: A-00-00
        private static readonly Regex RackLocationRegex = new(@"^[A-Za-z]-\d{2}-\d{2}$", RegexOptions.Compiled);

        // Format ceny: liczba dodatnia z maks. 2 cyframi po przecinku lub kropce
        private static readonly Regex PriceRegex = new(@"^\d+([.,]\d{1,2})?$", RegexOptions.Compiled);

        // Format imienia i nazwiska (dopuszcza polskie litery, spacje, kropki przy tytułach typu 'mgr inż.' i myślniki)
        private static readonly Regex PersonNameRegex = new(@"^[\p{L}\.\-\s]{3,80}$", RegexOptions.Compiled);

        // Wczytuje liczbę całkowitą z określonego zakresu
        public static int ReadIntInRange(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (int.TryParse(input, out int value) && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine($"  [Błąd] Podaj poprawną liczbę całkowitą z zakresu od {min} do {max}.");
            }
        }

        // Wczytuje dodatnią kwotę pieniężną (max 2 miejsca po przecinku)
        public static decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (TryParsePositivePrice(input, out decimal price))
                {
                    return price;
                }

                Console.WriteLine("  [Błąd] Podaj poprawną cenę większą od 0 (np. 85,50 lub 120).");
            }
        }

        // Wczytuje opcjonalną cenę podczas edycji (Enter zwraca null = brak zmian)
        public static decimal? ReadOptionalPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }

                if (TryParsePositivePrice(input, out decimal price))
                {
                    return price;
                }

                Console.WriteLine("  [Błąd] Nieprawidłowa cena! Podaj kwotę większą od 0 (np. 99,99) lub naciśnij Enter, aby pominąć.");
            }
        }

        // Wczytuje poprawny kod SKU produktu
        public static string ReadSku(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

                if (SkuRegex.IsMatch(input))
                {
                    return input;
                }

                Console.WriteLine("  [Błąd] Niepoprawny format SKU! Wymagany wzór: XXX-XXX-XXX (np. ALU-PRF-500 lub PLC-MOD-S7).");
            }
        }

        // Wczytuje poprawną lokalizację regałową (np. A-02-01)
        public static string ReadRackLocation(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

                if (RackLocationRegex.IsMatch(input))
                {
                    return input;
                }

                Console.WriteLine("  [Błąd] Niepoprawny format lokalizacji! Wymagany wzór: Litera-XX-XX (np. A-02-01).");
            }
        }

        // Wczytuje opcjonalną lokalizację regałową podczas edycji (Enter zwraca null = brak zmian)
        public static string? ReadOptionalRackLocation(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }

                if (RackLocationRegex.IsMatch(input))
                {
                    return input;
                }

                Console.WriteLine("  [Błąd] Niepoprawny format lokalizacji! Wpisz np. A-02-01 lub naciśnij Enter, aby pozostawić bez zmian.");
            }
        }

        // Wczytuje imię i nazwisko (wyłącznie litery, spacje, myślniki i kropki dla tytułów)
        public static string ReadPersonName(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (PersonNameRegex.IsMatch(input) && input.Count(char.IsLetter) >= 3)
                {
                    return input;
                }

                Console.WriteLine("  [Błąd] Podaj poprawne imię i nazwisko (bez cyfr i znaków specjalnych, min. 3 litery).");
            }
        }

        // Wczytuje opisowy tekst (np. nazwę produktu, firmy, notatkę) - odrzuca puste wpisy i same liczby
        public static string ReadTextWithLetters(string prompt, int minLength = 3, int maxLength = 150)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (input.Length >= minLength && input.Length <= maxLength && input.Count(char.IsLetter) >= 2)
                {
                    return input;
                }

                Console.WriteLine($"  [Błąd] Wpis musi mieć od {minLength} do {maxLength} znaków i zawierać litery (nie może być samą liczbą).");
            }
        }

        // Wczytuje hasło bez spacji o minimalnej długości
        public static string ReadPassword(string prompt, int minLength = 4)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (input.Length >= minLength && !input.Any(char.IsWhiteSpace))
                {
                    return input;
                }

                Console.WriteLine($"  [Błąd] Hasło musi mieć co najmniej {minLength} znaków i nie może zawierać spacji.");
            }
        }

        // Wczytuje niepusty tekst
        public static string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }

                Console.WriteLine("  [Błąd] Pole nie może być puste.");
            }
        }

        // Wymusza wybór jednej ze ściśle zdefiniowanych opcji
        public static string ReadExactOption(string prompt, params string[] allowedOptions)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";

                if (allowedOptions.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    return input;
                }

                Console.WriteLine($"  [Błąd] Nieprawidłowy wybór. Dozwolone opcje: {string.Join(", ", allowedOptions)}.");
            }
        }

        // Wczytuje potwierdzenie operacji (t/n)
        public static bool ReadConfirmation(string prompt)
        {
            string answer = ReadExactOption($"{prompt} (t/n): ", "t", "n");
            return answer.Equals("t", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParsePositivePrice(string input, out decimal price)
        {
            price = 0m;
            if (!PriceRegex.IsMatch(input)) return false;

            string normalized = input.Replace('.', ',');
            return decimal.TryParse(normalized, NumberStyles.Number, PolishCulture, out price) && price > 0m;
        }
    }
}