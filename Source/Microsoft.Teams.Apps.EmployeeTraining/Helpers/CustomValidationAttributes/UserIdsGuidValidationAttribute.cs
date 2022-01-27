// <copyright file="UserIdsGuidValidationAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers.CustomValidationAttributes;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

/// <summary>
/// Validate whether participants user id's are valid GUIDs.
/// </summary>
public sealed class UserIdsGuidValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// Validate whether participants user id's are valid GUID.
    /// </summary>
    /// <param name="value">String containing participants user id's separated by comma.</param>
    /// <param name="validationContext">Context for getting object which needs to be validated.</param>
    /// <returns>Validation result (either error message for failed validation or success).</returns>
    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        if ((value != null) && (value.GetType() == typeof(string)))
        {
            var userIds = Convert.ToString(value: value, provider: CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(value: userIds))
            {
                var userIdList = userIds.Split(separator: ';');

                foreach (var userId in userIdList)
                {
                    if (!Guid.TryParse(input: userId, result: out var validIserId))
                    {
                        return new ValidationResult(errorMessage: $"{validationContext?.DisplayName} does not contain valid GUID user Ids");
                    }
                }
            }
        }

        // Return success as property is not mandatory.
        return ValidationResult.Success;
    }
}