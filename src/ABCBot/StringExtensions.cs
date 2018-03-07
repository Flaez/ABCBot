using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ABCBot
{
    public static class StringExtensions
    {
        public static string Sanitize(this string input) {
            return new string(input.ToLower().Where(x => char.IsLetterOrDigit(x)).ToArray());
        }

        public static string NormalizeLineEndings(this string input) {
            return Regex.Replace(input, "\r\n|\r", "\n");
        }

        public static bool ToBoolean(this string input) {
            switch (input.ToLower()) {
                case "yes": {
                        return true;
                    }
                case "no": {
                        return false;
                    }
            }

            return false;
        }
    }
}
