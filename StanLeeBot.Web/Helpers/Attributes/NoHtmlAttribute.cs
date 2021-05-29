using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BabouExtensions;

namespace StanLeeBot.Web.Helpers.Attributes
{
    /// <summary>
    /// Checks if there is any HTML in a string field and, if so, returns an invalid response.
    /// </summary>
    public class NoHtmlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is string))
                return ValidationResult.Success;

            var strValue = Convert.ToString(value);
            var htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

            var errorMessage = ErrorMessage.IsNullOrWhiteSpace() ? $"HTML is not allowed for {validationContext.DisplayName}" : ErrorMessageString;

            return htmlRegex.IsMatch(strValue) ? new ValidationResult(errorMessage) : ValidationResult.Success;

        }
    }
}
