using System.Globalization;
using TechTalk.SpecFlow.Assist.ValueRetrievers;

namespace MyApi.WebApi.Tests.CustomValueRetrievers;

public class DateOnlyValueRetriever : StructRetriever<DateOnly>
{
    /// <summary>
    /// Gets or sets the DateTimeStyles to use when parsing the string value.
    /// </summary>
    /// <remarks>Defaults to DateTimeStyles.None.</remarks>
    public static DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;

    protected override DateOnly GetNonEmptyValue(string value)
    {
        DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles, out DateTime dateTimeValue);
        return DateOnly.FromDateTime(dateTimeValue);
    }
}
