const string invalidString = "This is invalid string";
const string? nullString = null;

// Bool.
((ReadOnlySpan<char>)"0").ToBoolSafe().Should().BeFalse(Reason());
"0".ToBoolSafe().Should().BeFalse(Reason());
"false".ToBoolSafe().Should().BeFalse(Reason());

"-1".ToBoolSafe().Should().BeTrue(Reason());
"1".ToBoolSafe().Should().BeTrue(Reason());
"true".ToBoolSafe().Should().BeTrue(Reason());

invalidString.ToBoolSafe().Should().BeNull(Reason());
nullString.ToBoolSafe().Should().BeNull(Reason());

// SByte.
((ReadOnlySpan<char>)"1").ToSByteSafe().Should().Be((sbyte)1, Reason());
"1".ToSByteSafe().Should().Be((sbyte)1, Reason());
invalidString.ToSByteSafe().Should().BeNull(Reason());
nullString.ToSByteSafe().Should().BeNull(Reason());

// Byte.
((ReadOnlySpan<char>)"1").ToByteSafe().Should().Be((byte)1, Reason());
"1".ToByteSafe().Should().Be((byte)1, Reason());
invalidString.ToByteSafe().Should().BeNull(Reason());
nullString.ToByteSafe().Should().BeNull(Reason());

// Short.
((ReadOnlySpan<char>)"1").ToShortSafe().Should().Be((short)1, Reason());
"1".ToShortSafe().Should().Be((short)1, Reason());
invalidString.ToShortSafe().Should().BeNull(Reason());
nullString.ToShortSafe().Should().BeNull(Reason());

// UShort.
((ReadOnlySpan<char>)"1").ToUShortSafe().Should().Be((ushort)1, Reason());
"1".ToUShortSafe().Should().Be((ushort)1, Reason());
invalidString.ToUShortSafe().Should().BeNull(Reason());
nullString.ToUShortSafe().Should().BeNull(Reason());

// Int.
((ReadOnlySpan<char>)"1").ToIntSafe().Should().Be(1, Reason());
"1".ToIntSafe().Should().Be(1, Reason());
invalidString.ToIntSafe().Should().BeNull(Reason());
nullString.ToIntSafe().Should().BeNull(Reason());

// UInt.
((ReadOnlySpan<char>)"1").ToUIntSafe().Should().Be(1u, Reason());
"1".ToUIntSafe().Should().Be(1u, Reason());
invalidString.ToUIntSafe().Should().BeNull(Reason());
nullString.ToUIntSafe().Should().BeNull(Reason());

// Long.
((ReadOnlySpan<char>)"1").ToLongSafe().Should().Be(1L, Reason());
"1".ToLongSafe().Should().Be(1L, Reason());
invalidString.ToLongSafe().Should().BeNull(Reason());
nullString.ToLongSafe().Should().BeNull(Reason());

// ULong.
((ReadOnlySpan<char>)"1").ToULongSafe().Should().Be(1UL, Reason());
"1".ToULongSafe().Should().Be(1UL, Reason());
invalidString.ToULongSafe().Should().BeNull(Reason());
nullString.ToULongSafe().Should().BeNull(Reason());

#if NET5_0_OR_GREATER
// NInt.
#if NET6_0_OR_GREATER
((ReadOnlySpan<char>)"1").ToNIntSafe().Should().Be((nint)1, Reason());
#endif
"1".ToNIntSafe().Should().Be((nint)1, Reason());
invalidString.ToNIntSafe().Should().BeNull(Reason());
nullString.ToNIntSafe().Should().BeNull(Reason());

// NUInt.
#if NET6_0_OR_GREATER
((ReadOnlySpan<char>)"1").ToNUIntSafe().Should().Be((nuint)1, Reason());
#endif
"1".ToNUIntSafe().Should().Be((nuint)1, Reason());
invalidString.ToNUIntSafe().Should().BeNull(Reason());
nullString.ToNUIntSafe().Should().BeNull(Reason());
#endif

#if NET7_0_OR_GREATER
// Int128.
((ReadOnlySpan<char>)"1").ToInt128Safe().Should().Be((Int128)1, Reason());
"1".ToInt128Safe().Should().Be((Int128)1, Reason());
invalidString.ToInt128Safe().Should().BeNull(Reason());
nullString.ToInt128Safe().Should().BeNull(Reason());

// UInt128.
((ReadOnlySpan<char>)"1").ToUInt128Safe().Should().Be((UInt128)1, Reason());
"1".ToUInt128Safe().Should().Be((UInt128)1, Reason());
invalidString.ToUInt128Safe().Should().BeNull(Reason());
nullString.ToUInt128Safe().Should().BeNull(Reason());
#endif

#if NET5_0_OR_GREATER
// Half.
((ReadOnlySpan<char>)"1.23").ToHalfSafe().Should().Be((Half)1.23, Reason());
"1.23".ToHalfSafe().Should().Be((Half)1.23, Reason());
invalidString.ToHalfSafe().Should().BeNull(Reason());
nullString.ToHalfSafe().Should().BeNull(Reason());
#endif

// Float.
((ReadOnlySpan<char>)"1.23").ToFloatSafe().Should().Be(1.23f, Reason());
"1.23".ToFloatSafe().Should().Be(1.23f, Reason());
invalidString.ToFloatSafe().Should().BeNull(Reason());
nullString.ToFloatSafe().Should().BeNull(Reason());

// Double.
((ReadOnlySpan<char>)"1.23").ToDoubleSafe().Should().Be(1.23, Reason());
"1.23".ToDoubleSafe().Should().Be(1.23, Reason());
invalidString.ToDoubleSafe().Should().BeNull(Reason());
nullString.ToDoubleSafe().Should().BeNull(Reason());

// Decimal.
((ReadOnlySpan<char>)"1").ToDecimalSafe().Should().Be(1m, Reason());
"1".ToDecimalSafe().Should().Be(1m, Reason());
invalidString.ToDecimalSafe().Should().BeNull(Reason());
nullString.ToDecimalSafe().Should().BeNull(Reason());

// Guid.
var expectedGuid = Guid.NewGuid();
var guidFormat = "D";
var guidFormats = new [] { "X", guidFormat };
var guidString = expectedGuid.ToString(guidFormat);

((ReadOnlySpan<char>)expectedGuid.ToString()).ToGuidSafe().Should().Be(expectedGuid, Reason());
expectedGuid.ToString().ToGuidSafe().Should().Be(expectedGuid, Reason());

((ReadOnlySpan<char>)guidString).ToGuidSafe((ReadOnlySpan<char>)guidFormat).Should().Be(expectedGuid, Reason());
guidString.ToGuidSafe(guidFormat).Should().Be(expectedGuid, Reason());
((ReadOnlySpan<char>)guidString).ToGuidSafe(guidFormats).Should().Be(expectedGuid, Reason());
guidString.ToGuidSafe(guidFormats).Should().Be(expectedGuid, Reason());

invalidString.ToGuidSafe().Should().BeNull(Reason());
invalidString.ToGuidSafe(guidFormat).Should().BeNull(Reason());
invalidString.ToGuidSafe(guidFormats).Should().BeNull(Reason());

nullString.ToGuidSafe().Should().BeNull(Reason());
nullString.ToGuidSafe(guidFormat).Should().BeNull(Reason());
nullString.ToGuidSafe(guidFormats).Should().BeNull(Reason());

// DateTime.
var expectedDateTime = new DateTime(1978, 9, 30, 3, 11, 51);
var dateTimeFormat = @"yyyy-MM-dd hh\:mm\:ss";
var dateTimeFormats = new[] { dateTimeFormat.Replace(@"\:ss", ""), dateTimeFormat };
var dateTimeString = expectedDateTime.ToString(dateTimeFormat);

((ReadOnlySpan<char>)expectedDateTime.ToString()).ToDateTimeSafe().Should().Be(expectedDateTime, Reason());
expectedDateTime.ToString().ToDateTimeSafe().Should().Be(expectedDateTime, Reason());

((ReadOnlySpan<char>)dateTimeString).ToDateTimeSafe((ReadOnlySpan<char>)dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeSafe(dateTimeFormat).Should().Be(expectedDateTime, Reason());
((ReadOnlySpan<char>)dateTimeString).ToDateTimeSafe(dateTimeFormats).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeSafe(dateTimeFormats).Should().Be(expectedDateTime, Reason());

invalidString.ToDateTimeSafe().Should().BeNull(Reason());
invalidString.ToDateTimeSafe(dateTimeFormats).Should().BeNull(Reason());

nullString.ToDateTimeSafe().Should().BeNull(Reason());
nullString.ToDateTimeSafe(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTimeSafe(dateTimeFormats).Should().BeNull(Reason());

var utcNow = DateTime.UtcNow;
var dateTimeOffsetUtcNow = DateTimeOffset.UtcNow;

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
	isoTime.ToUtcDateTimeSafe().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
	((ReadOnlySpan<char>)isoTime).ToUtcDateTimeSafe().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
}

nullString.ToUtcDateTimeSafe().Should().BeNull(Reason());

var expectedUtcDateTime = new DateTime(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);

dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString()
	.ToUtcDateTimeFromUnixTimeSecondsSafe()
	.Should()
	.Be(expectedUtcDateTime, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString())
	.ToUtcDateTimeFromUnixTimeSecondsSafe()
	.Should()
	.Be(expectedUtcDateTime, Reason());

invalidString.ToUtcDateTimeFromUnixTimeSecondsSafe().Should().BeNull(Reason());
nullString.ToUtcDateTimeFromUnixTimeSecondsSafe().Should().BeNull(Reason());

expectedUtcDateTime = new DateTime(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);

dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString()
	.ToUtcDateTimeFromUnixTimeMillisecondsSafe()
	.Should()
	.Be(expectedUtcDateTime, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString())
	.ToUtcDateTimeFromUnixTimeMillisecondsSafe()
	.Should()
	.Be(expectedUtcDateTime, Reason());

invalidString.ToUtcDateTimeFromUnixTimeMillisecondsSafe().Should().BeNull(Reason());
nullString.ToUtcDateTimeFromUnixTimeMillisecondsSafe().Should().BeNull(Reason());

// DateTimeOffset.
((ReadOnlySpan<char>)expectedDateTime.ToString()).ToDateTimeOffsetSafe().Should().Be(expectedDateTime, Reason());
expectedDateTime.ToString().ToDateTimeOffsetSafe().Should().Be(expectedDateTime, Reason());

((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffsetSafe((ReadOnlySpan<char>)dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffsetSafe(dateTimeFormat).Should().Be(expectedDateTime, Reason());
((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffsetSafe(dateTimeFormats).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffsetSafe(dateTimeFormats).Should().Be(expectedDateTime, Reason());

invalidString.ToDateTimeOffsetSafe().Should().BeNull(Reason());
invalidString.ToDateTimeOffsetSafe(dateTimeFormats).Should().BeNull(Reason());

nullString.ToDateTimeOffsetSafe().Should().BeNull(Reason());
nullString.ToDateTimeOffsetSafe(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTimeOffsetSafe(dateTimeFormats).Should().BeNull(Reason());

var expectedUtcDateTimeOffset = new DateTimeOffset(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerSecond), TimeSpan.Zero);

dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString()
	.ToDateTimeOffsetFromUnixTimeSecondsSafe()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString())
	.ToDateTimeOffsetFromUnixTimeSecondsSafe()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());

invalidString.ToDateTimeOffsetFromUnixTimeSecondsSafe().Should().BeNull(Reason());
nullString.ToDateTimeOffsetFromUnixTimeSecondsSafe().Should().BeNull(Reason());

expectedUtcDateTimeOffset = new DateTimeOffset(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerMillisecond), TimeSpan.Zero);

dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString()
	.ToDateTimeOffsetFromUnixTimeMillisecondsSafe()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString())
	.ToDateTimeOffsetFromUnixTimeMillisecondsSafe()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());

invalidString.ToDateTimeOffsetFromUnixTimeMillisecondsSafe().Should().BeNull(Reason());
nullString.ToDateTimeOffsetFromUnixTimeMillisecondsSafe().Should().BeNull(Reason());

// TimeSpan.
var expectedTimeSpan = new TimeSpan(9, 12, 34, 56);
var timeSpanFormat = @"d\:hh\:mm\:ss";
var timeSpanFormats = new[] { timeSpanFormat.Replace(@"\:ss", ""), timeSpanFormat };
var timeSpanString = expectedTimeSpan.ToString(timeSpanFormat);

((ReadOnlySpan<char>)expectedTimeSpan.ToString()).ToTimeSpanSafe().Should().Be(expectedTimeSpan, Reason());
expectedTimeSpan.ToString().ToTimeSpanSafe().Should().Be(expectedTimeSpan, Reason());

((ReadOnlySpan<char>)timeSpanString).ToTimeSpanSafe((ReadOnlySpan<char>)timeSpanFormat).Should().Be(expectedTimeSpan, Reason());
timeSpanString.ToTimeSpanSafe(timeSpanFormat).Should().Be(expectedTimeSpan, Reason());
((ReadOnlySpan<char>)timeSpanString).ToTimeSpanSafe(timeSpanFormats).Should().Be(expectedTimeSpan, Reason());
timeSpanString.ToTimeSpanSafe(timeSpanFormats).Should().Be(expectedTimeSpan, Reason());

invalidString.ToTimeSpanSafe().Should().BeNull(Reason());
invalidString.ToTimeSpanSafe(timeSpanFormat).Should().BeNull(Reason());
invalidString.ToTimeSpanSafe(timeSpanFormats).Should().BeNull(Reason());

nullString.ToTimeSpanSafe().Should().BeNull(Reason());
nullString.ToTimeSpanSafe(timeSpanFormat).Should().BeNull(Reason());
nullString.ToTimeSpanSafe(timeSpanFormats).Should().BeNull(Reason());

#if NET6_0_OR_GREATER
// DateOnly.
var expectedDateOnly = new DateOnly(1978, 9, 30);
var dateOnlyFormat = @"yyyy-MM-dd";
var dateOnlyFormats = new[] { dateOnlyFormat.Replace("-dd", ""), dateOnlyFormat };
var dateOnlyString = expectedDateOnly.ToString(dateOnlyFormat);

((ReadOnlySpan<char>)expectedDateOnly.ToString()).ToDateOnlySafe().Should().Be(expectedDateOnly, Reason());
expectedDateOnly.ToString().ToDateOnlySafe().Should().Be(expectedDateOnly, Reason());

((ReadOnlySpan<char>)dateOnlyString).ToDateOnlySafe((ReadOnlySpan<char>)dateOnlyFormat).Should().Be(expectedDateOnly, Reason());
dateOnlyString.ToDateOnlySafe(dateOnlyFormat).Should().Be(expectedDateOnly, Reason());
((ReadOnlySpan<char>)dateOnlyString).ToDateOnlySafe(dateOnlyFormats).Should().Be(expectedDateOnly, Reason());
dateOnlyString.ToDateOnlySafe(dateOnlyFormats).Should().Be(expectedDateOnly, Reason());

invalidString.ToDateOnlySafe().Should().BeNull(Reason());
invalidString.ToDateOnlySafe(dateOnlyFormats).Should().BeNull(Reason());

nullString.ToDateOnlySafe().Should().BeNull(Reason());
nullString.ToDateOnlySafe(dateOnlyFormat).Should().BeNull(Reason());
nullString.ToDateOnlySafe(dateOnlyFormats).Should().BeNull(Reason());

// TimeOnly.
var expectedTimeOnly = new TimeOnly(9, 10, 0);
var timeOnlyFormat = @"hh\:mm";
var timeOnlyFormats = new[] { timeOnlyFormat.Replace(@"\:mm", ""), timeOnlyFormat };
var timeOnlyString = expectedTimeOnly.ToString(timeOnlyFormat);

((ReadOnlySpan<char>)expectedTimeOnly.ToString()).ToTimeOnlySafe().Should().Be(expectedTimeOnly, Reason());
expectedTimeOnly.ToString().ToTimeOnlySafe().Should().Be(expectedTimeOnly, Reason());

((ReadOnlySpan<char>)timeOnlyString).ToTimeOnlySafe((ReadOnlySpan<char>)timeOnlyFormat).Should().Be(expectedTimeOnly, Reason());
timeOnlyString.ToTimeOnlySafe(timeOnlyFormat).Should().Be(expectedTimeOnly, Reason());
((ReadOnlySpan<char>)timeOnlyString).ToTimeOnlySafe(timeOnlyFormats).Should().Be(expectedTimeOnly, Reason());
timeOnlyString.ToTimeOnlySafe(timeOnlyFormats).Should().Be(expectedTimeOnly, Reason());

invalidString.ToTimeOnlySafe().Should().BeNull(Reason());
invalidString.ToTimeOnlySafe(timeOnlyFormat).Should().BeNull(Reason());
invalidString.ToTimeOnlySafe(timeOnlyFormats).Should().BeNull(Reason());

nullString.ToTimeOnlySafe().Should().BeNull(Reason());
nullString.ToTimeOnlySafe(timeOnlyFormat).Should().BeNull(Reason());
nullString.ToTimeOnlySafe(timeOnlyFormats).Should().BeNull(Reason());
#endif
