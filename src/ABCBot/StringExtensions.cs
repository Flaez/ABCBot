using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ABCBot
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string input) {
            return Regex.Replace(input, "\r\n|\r", "\n");
        }
    }
}
