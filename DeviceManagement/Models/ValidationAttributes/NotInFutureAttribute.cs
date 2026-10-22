using System;
using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Models.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to enforce that an asset's entry date cannot be in the future.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotInFutureAttribute : ValidationAttribute
    {
        public NotInFutureAttribute() : base("Date of entry cannot be in the future.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // Let [Required] handle null values
                return ValidationResult.Success;
            }

            var memberNames = validationContext.MemberName != null
                ? new[] { validationContext.MemberName }
                : null;

            if (value is DateTime dateTime)
            {
                if (dateTime.Date > DateTime.Today)
                {
                    return new ValidationResult(ErrorMessageString, memberNames);
                }
                return ValidationResult.Success;
            }

            if (value is DateOnly dateOnly)
            {
                if (dateOnly > DateOnly.FromDateTime(DateTime.Today))
                {
                    return new ValidationResult(ErrorMessageString, memberNames);
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date value.", memberNames);
        }
    }
}
