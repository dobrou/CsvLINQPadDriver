var invalidString = "This is invalid string";

// Bool.
"0".ToBool().Should().BeFalse(Reason());
"false".ToBool().Should().BeFalse(Reason());

"-1".ToBool().Should().BeTrue(Reason());
"1".ToBool().Should().BeTrue(Reason());
"true".ToBool().Should().BeTrue(Reason());

invalidString.ToBool().Should().BeNull(Reason());

// Int.
"1".ToInt().Should().Be(1, Reason());
invalidString.ToInt().Should().BeNull(Reason());

// Long.
"1".ToLong().Should().Be(1L, Reason());
invalidString.ToLong().Should().BeNull(Reason());

// Double.
"1.23".ToDouble().Should().Be(1.23, Reason());
invalidString.ToDouble().Should().BeNull(Reason());

// Decimal.
"1".ToDecimal().Should().Be(1m, Reason());
invalidString.ToDecimal().Should().BeNull(Reason());

// Guid.
var expectedGuid = Guid.NewGuid();
var guidFormat = "D";
var guidFormats = new [] { "X", guidFormat };
var guidString = expectedGuid.ToString(guidFormat);

expectedGuid.ToString().ToGuid().Should().Be(expectedGuid, Reason());

guidString.ToGuid(guidFormat).Should().Be(expectedGuid, Reason());
guidString.ToGuid(guidFormats).Should().Be(expectedGuid, Reason());

invalidString.ToGuid().Should().BeNull(Reason());
invalidString.ToGuid(guidFormat).Should().BeNull(Reason());
invalidString.ToGuid(guidFormats).Should().BeNull(Reason());

// DateTime.
var expectedDateTime = new DateTime(1978, 9, 30, 3, 11, 51);
var dateTimeFormat = @"yyyy-MM-dd hh\:mm\:ss";
var dateTimeFormats = new[] { dateTimeFormat.Replace(@"\:ss", ""), dateTimeFormat };
var dateTimeString = expectedDateTime.ToString(dateTimeFormat);

expectedDateTime.ToString().ToDateTime().Should().Be(expectedDateTime, Reason());

dateTimeString.ToDateTime(dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTime(dateTimeFormats).Should().Be(expectedDateTime, Reason());

invalidString.ToDateTime().Should().BeNull(Reason());
invalidString.ToDateTime(dateTimeFormat).Should().BeNull(Reason());
invalidString.ToDateTime(dateTimeFormats).Should().BeNull(Reason());

// TimeSpan.
var expectedTimeSpan = new TimeSpan(9, 12, 34, 56);
var timeSpanFormat = @"d\:hh\:mm\:ss";
var timeSpanFormats = new[] { timeSpanFormat.Replace(@"\:ss", ""), timeSpanFormat };
var timeSpanString = expectedTimeSpan.ToString(timeSpanFormat);

expectedTimeSpan.ToString().ToTimeSpan().Should().Be(expectedTimeSpan, Reason());

timeSpanString.ToTimeSpan(timeSpanFormat).Should().Be(expectedTimeSpan, Reason());
timeSpanString.ToTimeSpan(timeSpanFormats).Should().Be(expectedTimeSpan, Reason());

invalidString.ToTimeSpan().Should().BeNull(Reason());
invalidString.ToTimeSpan(timeSpanFormat).Should().BeNull(Reason());
invalidString.ToTimeSpan(timeSpanFormats).Should().BeNull(Reason());
