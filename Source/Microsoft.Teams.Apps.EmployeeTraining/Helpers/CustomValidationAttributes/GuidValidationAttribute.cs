// <copyright file="GuidValidationAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers.CustomValidationAttributes;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

/// <summary>
/// Validate input id is a valid GUID.
/// </summary>
public sealed class GuidValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// Validate whether input id is a valid GUID.
    /// </summary>
    /// <param name="value">String containing input id like tab id etc.</param>
    /// <param name="validationContext">Context for getting object which needs to be validated.</param>
    /// <returns>Validation result (either error message for failed validation or success).</returns>
    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        if ((value != null) && (value.GetType() == typeof(string)))
        {
            var inputId = Convert.ToString(value: value, provider: CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(value: inputId))
            {
                return new ValidationResult(errorMessage: "Input id cannot be null or empty.");
            }

            if (!Guid.TryParse(input: inputId, result: out var validInputId))
            {
                return new ValidationResult(errorMessage: $"Input id: {inputId} is not a valid GUID.");
            }

            // Input id is a valid GUID.
            return ValidationResult.Success;
        }

        return new ValidationResult(errorMessage: $"{validationContext?.DisplayName} is not a valid string.");
    }
}