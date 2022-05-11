var invalidString = "This is invalid string";
string nullString = null;

// Bool.
((ReadOnlySpan<char>)"0").ToBool().Should().BeFalse(Reason());
"0".ToBool().Should().BeFalse(Reason());
"false".ToBool().Should().BeFalse(Reason());

"-1".ToBool().Should().BeTrue(Reason());
"1".ToBool().Should().BeTrue(Reason());
"true".ToBool().Should().BeTrue(Reason());

invalidString.ToBool().Should().BeNull(Reason());
nullString.ToBool().Should().BeNull(Reason());

// Int.
((ReadOnlySpan<char>)"1").ToInt().Should().Be(1, Reason());
"1".ToInt().Should().Be(1, Reason());
invalidString.ToInt().Should().BeNull(Reason());
nullString.ToInt().Should().BeNull(Reason());

// Long.
((ReadOnlySpan<char>)"1").ToLong().Should().Be(1L, Reason());
"1".ToLong().Should().Be(1L, Reason());
invalidString.ToLong().Should().BeNull(Reason());
nullString.ToLong().Should().BeNull(Reason());

// Float.
((ReadOnlySpan<char>)"1.23").ToFloat().Should().Be(1.23f, Reason());
"1.23".ToFloat().Should().Be(1.23f, Reason());
invalidString.ToFloat().Should().BeNull(Reason());
nullString.ToFloat().Should().BeNull(Reason());

// Double.
((ReadOnlySpan<char>)"1.23").ToDouble().Should().Be(1.23, Reason());
"1.23".ToDouble().Should().Be(1.23, Reason());
invalidString.ToDouble().Should().BeNull(Reason());
nullString.ToDouble().Should().BeNull(Reason());

// Decimal.
((ReadOnlySpan<char>)"1").ToDecimal().Should().Be(1m, Reason());
"1".ToDecimal().Should().Be(1m, Reason());
invalidString.ToDecimal().Should().BeNull(Reason());
nullString.ToDecimal().Should().BeNull(Reason());

// Guid.
var expectedGuid = Guid.NewGuid();
var guidFormat = "D";
var guidFormats = new [] { "X", guidFormat };
var guidString = expectedGuid.ToString(guidFormat);

((ReadOnlySpan<char>)expectedGuid.ToString()).ToGuid().Should().Be(expectedGuid, Reason());
expectedGuid.ToString().ToGuid().Should().Be(expectedGuid, Reason());

((ReadOnlySpan<char>)guidString).ToGuid((ReadOnlySpan<char>)guidFormat).Should().Be(expectedGuid, Reason());
guidString.ToGuid(guidFormat).Should().Be(expectedGuid, Reason());
((ReadOnlySpan<char>)guidString).ToGuid(guidFormats).Should().Be(expectedGuid, Reason());
guidString.ToGuid(guidFormats).Should().Be(expectedGuid, Reason());

invalidString.ToGuid().Should().BeNull(Reason());
invalidString.ToGuid(guidFormat).Should().BeNull(Reason());
invalidString.ToGuid(guidFormats).Should().BeNull(Reason());

nullString.ToGuid().Should().BeNull(Reason());
nullString.ToGuid(guidFormat).Should().BeNull(Reason());
nullString.ToGuid(guidFormats).Should().BeNull(Reason());

// DateTime.
var expectedDateTime = new DateTime(1978, 9, 30, 3, 11, 51);
var dateTimeFormat = @"yyyy-MM-dd hh\:mm\:ss";
var dateTimeFormats = new[] { dateTimeFormat.Replace(@"\:ss", ""), dateTimeFormat };
var dateTimeString = expectedDateTime.ToString(dateTimeFormat);

((ReadOnlySpan<char>)expectedDateTime.ToString()).ToDateTime().Should().Be(expectedDateTime, Reason());
expectedDateTime.ToString().ToDateTime().Should().Be(expectedDateTime, Reason());

((ReadOnlySpan<char>)dateTimeString).ToDateTime((ReadOnlySpan<char>)dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTime(dateTimeFormat).Should().Be(expectedDateTime, Reason());
((ReadOnlySpan<char>)dateTimeString).ToDateTime(dateTimeFormats).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTime(dateTimeFormats).Should().Be(expectedDateTime, Reason());

invalidString.ToDateTime().Should().BeNull(Reason());
invalidString.ToDateTime(dateTimeFormats).Should().BeNull(Reason());

nullString.ToDateTime().Should().BeNull(Reason());
nullString.ToDateTime(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTime(dateTimeFormats).Should().BeNull(Reason());

var utcNow = DateTime.UtcNow;
var expectedUtcNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second).AddHours(-1).AddMinutes(-30);
utcNow.ToString("yyyy-MM-ddTHH:mm:ss+01:30").ToUtcDateTime().Should().Be(expectedUtcNow, Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());

var isoTimes = new []
{
	"2022-05-11T10:13:19",
	"2022-05-11T10:13:19Z",
	"2022-05-11T10:13:19+01:00"
};

foreach(var isoTime in isoTimes)
{
	isoTime.ToUtcDateTime().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
	((ReadOnlySpan<char>)isoTime).ToUtcDateTime().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
}

nullString.ToUtcDateTime().Should().BeNull(Reason());

// DateTimeOffset.
((ReadOnlySpan<char>)expectedDateTime.ToString()).ToDateTimeOffset().Should().Be(expectedDateTime, Reason());
expectedDateTime.ToString().ToDateTimeOffset().Should().Be(expectedDateTime, Reason());

((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffset((ReadOnlySpan<char>)dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffset(dateTimeFormat).Should().Be(expectedDateTime, Reason());
((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffset(dateTimeFormats).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffset(dateTimeFormats).Should().Be(expectedDateTime, Reason());

invalidString.ToDateTimeOffset().Should().BeNull(Reason());
invalidString.ToDateTimeOffset(dateTimeFormats).Should().BeNull(Reason());

nullString.ToDateTimeOffset().Should().BeNull(Reason());
nullString.ToDateTimeOffset(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTimeOffset(dateTimeFormats).Should().BeNull(Reason());

// TimeSpan.
var expectedTimeSpan = new TimeSpan(9, 12, 34, 56);
var timeSpanFormat = @"d\:hh\:mm\:ss";
var timeSpanFormats = new[] { timeSpanFormat.Replace(@"\:ss", ""), timeSpanFormat };
var timeSpanString = expectedTimeSpan.ToString(timeSpanFormat);

((ReadOnlySpan<char>)expectedTimeSpan.ToString()).ToTimeSpan().Should().Be(expectedTimeSpan, Reason());
expectedTimeSpan.ToString().ToTimeSpan().Should().Be(expectedTimeSpan, Reason());

((ReadOnlySpan<char>)timeSpanString).ToTimeSpan((ReadOnlySpan<char>)timeSpanFormat).Should().Be(expectedTimeSpan, Reason());
timeSpanString.ToTimeSpan(timeSpanFormat).Should().Be(expectedTimeSpan, Reason());
((ReadOnlySpan<char>)timeSpanString).ToTimeSpan(timeSpanFormats).Should().Be(expectedTimeSpan, Reason());
timeSpanString.ToTimeSpan(timeSpanFormats).Should().Be(expectedTimeSpan, Reason());

invalidString.ToTimeSpan().Should().BeNull(Reason());
invalidString.ToTimeSpan(timeSpanFormat).Should().BeNull(Reason());
invalidString.ToTimeSpan(timeSpanFormats).Should().BeNull(Reason());

nullString.ToTimeSpan().Should().BeNull(Reason());
nullString.ToTimeSpan(timeSpanFormat).Should().BeNull(Reason());
nullString.ToTimeSpan(timeSpanFormats).Should().BeNull(Reason());

#if NET6_0_OR_GREATER
// DateOnly.
var expectedDateOnly = new DateOnly(1978, 9, 30);
var dateOnlyFormat = @"yyyy-MM-dd";
var dateOnlyFormats = new[] { dateOnlyFormat.Replace("-dd", ""), dateOnlyFormat };
var dateOnlyString = expectedDateOnly.ToString(dateOnlyFormat);

((ReadOnlySpan<char>)expectedDateOnly.ToString()).ToDateOnly().Should().Be(expectedDateOnly, Reason());
expectedDateOnly.ToString().ToDateOnly().Should().Be(expectedDateOnly, Reason());

((ReadOnlySpan<char>)dateOnlyString).ToDateOnly((ReadOnlySpan<char>)dateOnlyFormat).Should().Be(expectedDateOnly, Reason());
dateOnlyString.ToDateOnly(dateOnlyFormat).Should().Be(expectedDateOnly, Reason());
((ReadOnlySpan<char>)dateOnlyString).ToDateOnly(dateOnlyFormats).Should().Be(expectedDateOnly, Reason());
dateOnlyString.ToDateOnly(dateOnlyFormats).Should().Be(expectedDateOnly, Reason());

invalidString.ToDateOnly().Should().BeNull(Reason());
invalidString.ToDateOnly(dateOnlyFormats).Should().BeNull(Reason());

nullString.ToDateOnly().Should().BeNull(Reason());
nullString.ToDateOnly(dateOnlyFormat).Should().BeNull(Reason());
nullString.ToDateOnly(dateOnlyFormats).Should().BeNull(Reason());

// TimeOnly.
var expectedTimeOnly = new TimeOnly(9, 10, 0);
var timeOnlyFormat = @"hh\:mm";
var timeOnlyFormats = new[] { timeOnlyFormat.Replace(@"\:mm", ""), timeOnlyFormat };
var timeOnlyString = expectedTimeOnly.ToString(timeOnlyFormat);

((ReadOnlySpan<char>)expectedTimeOnly.ToString()).ToTimeOnly().Should().Be(expectedTimeOnly, Reason());
expectedTimeOnly.ToString().ToTimeOnly().Should().Be(expectedTimeOnly, Reason());

((ReadOnlySpan<char>)timeOnlyString).ToTimeOnly((ReadOnlySpan<char>)timeOnlyFormat).Should().Be(expectedTimeOnly, Reason());
timeOnlyString.ToTimeOnly(timeOnlyFormat).Should().Be(expectedTimeOnly, Reason());
((ReadOnlySpan<char>)timeOnlyString).ToTimeOnly(timeOnlyFormats).Should().Be(expectedTimeOnly, Reason());
timeOnlyString.ToTimeOnly(timeOnlyFormats).Should().Be(expectedTimeOnly, Reason());

invalidString.ToTimeOnly().Should().BeNull(Reason());
invalidString.ToTimeOnly(timeOnlyFormat).Should().BeNull(Reason());
invalidString.ToTimeOnly(timeOnlyFormats).Should().BeNull(Reason());

nullString.ToTimeOnly().Should().BeNull(Reason());
nullString.ToTimeOnly(timeOnlyFormat).Should().BeNull(Reason());
nullString.ToTimeOnly(timeOnlyFormats).Should().BeNull(Reason());
#endif
