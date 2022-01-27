// <copyright file="StringExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

using System;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// This class lists the extension methods for string data type
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// Escaping unsafe, reserved and special characters that requires escaping includes
    /// + - &amp; | ! ( ) { } [ ] ^ " ~ * ? : \ /
    /// </summary>
    /// <param name="value">The string value</param>
    /// <returns>Returns string escaping unsafe, reserved and special characters.</returns>
    public static string EscapeSpecialCharacters(this string value)
    {
        if (!string.IsNullOrEmpty(value: value))
        {
            value = value.Replace(oldValue: "*", newValue: string.Empty, comparisonType: StringComparison.InvariantCulture).Trim();
            var pattern = @"([_|\\@&\?\*\+!-:~'\^/(){}<>#&\[\]])";
            var substitution = "\\$&";
            value = Regex.Replace(input: value, pattern: pattern, replacement: substitution);
            value = value.Any(ch => !char.IsLetterOrDigit(c: ch)) ? value += "\\" + "*" : value += "*";
        }

        return value;
    }
}