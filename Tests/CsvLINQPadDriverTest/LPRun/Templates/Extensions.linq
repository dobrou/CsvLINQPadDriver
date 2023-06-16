const string invalidString = "This is invalid string";
const string? nullString = null;

// Bool.
((ReadOnlySpan<char>)"0").ToBool().Should().BeFalse(Reason());
"0".ToBool().Should().BeFalse(Reason());
"false".ToBool().Should().BeFalse(Reason());

"-1".ToBool().Should().BeTrue(Reason());
"1".ToBool().Should().BeTrue(Reason());
"true".ToBool().Should().BeTrue(Reason());

new Action(() => invalidString.ToBool()).Should().Throw<ConvertException>(Reason());
nullString.ToBool().Should().BeNull(Reason());

// SByte.
((ReadOnlySpan<char>)"1").ToSByte().Should().Be((sbyte)1, Reason());
"1".ToSByte().Should().Be((sbyte)1, Reason());
new Action(() => invalidString.ToSByte()).Should().Throw<ConvertException>(Reason());
nullString.ToSByte().Should().BeNull(Reason());

// Byte.
((ReadOnlySpan<char>)"1").ToByte().Should().Be((byte)1, Reason());
"1".ToByte().Should().Be((byte)1, Reason());
new Action(() => invalidString.ToByte()).Should().Throw<ConvertException>(Reason());
nullString.ToByte().Should().BeNull(Reason());

// Short.
((ReadOnlySpan<char>)"1").ToShort().Should().Be((short)1, Reason());
"1".ToShort().Should().Be((short)1, Reason());
new Action(() => invalidString.ToShort()).Should().Throw<ConvertException>(Reason());
nullString.ToShort().Should().BeNull(Reason());

// UShort.
((ReadOnlySpan<char>)"1").ToUShort().Should().Be((ushort)1, Reason());
"1".ToUShort().Should().Be((ushort)1, Reason());
new Action(() => invalidString.ToUShort()).Should().Throw<ConvertException>(Reason());
nullString.ToUShort().Should().BeNull(Reason());

// Int.
((ReadOnlySpan<char>)"1").ToInt().Should().Be(1, Reason());
"1".ToInt().Should().Be(1, Reason());
new Action(() => invalidString.ToInt()).Should().Throw<ConvertException>(Reason());
nullString.ToInt().Should().BeNull(Reason());

// UInt.
((ReadOnlySpan<char>)"1").ToUInt().Should().Be(1u, Reason());
"1".ToUInt().Should().Be(1u, Reason());
new Action(() => invalidString.ToUInt()).Should().Throw<ConvertException>(Reason());
nullString.ToUInt().Should().BeNull(Reason());

// Long.
((ReadOnlySpan<char>)"1").ToLong().Should().Be(1L, Reason());
"1".ToLong().Should().Be(1L, Reason());
new Action(() => invalidString.ToLong()).Should().Throw<ConvertException>(Reason());
nullString.ToLong().Should().BeNull(Reason());

// ULong.
((ReadOnlySpan<char>)"1").ToULong().Should().Be(1UL, Reason());
"1".ToULong().Should().Be(1UL, Reason());
new Action(() => invalidString.ToULong()).Should().Throw<ConvertException>(Reason());
nullString.ToULong().Should().BeNull(Reason());

#if NET5_0_OR_GREATER
// NInt.
#if NET6_0_OR_GREATER
((ReadOnlySpan<char>)"1").ToNInt().Should().Be((nint)1, Reason());
#endif
"1".ToNInt().Should().Be((nint)1, Reason());
new Action(() => invalidString.ToNInt()).Should().Throw<ConvertException>(Reason());
nullString.ToNInt().Should().BeNull(Reason());

// NUInt.
#if NET6_0_OR_GREATER
((ReadOnlySpan<char>)"1").ToNUInt().Should().Be((nuint)1, Reason());
#endif
"1".ToNUInt().Should().Be((nuint)1, Reason());
new Action(() => invalidString.ToNUInt()).Should().Throw<ConvertException>(Reason());
nullString.ToNUInt().Should().BeNull(Reason());
#endif

#if NET7_0_OR_GREATER
// Int128.
((ReadOnlySpan<char>)"1").ToInt128().Should().Be((Int128)1, Reason());
"1".ToInt128().Should().Be((Int128)1, Reason());
new Action(() => invalidString.ToInt128()).Should().Throw<ConvertException>(Reason());
nullString.ToInt128().Should().BeNull(Reason());

// UInt128.
((ReadOnlySpan<char>)"1").ToUInt128().Should().Be((UInt128)1, Reason());
"1".ToUInt128().Should().Be((UInt128)1, Reason());
new Action(() => invalidString.ToUInt128()).Should().Throw<ConvertException>(Reason());
nullString.ToUInt128().Should().BeNull(Reason());
#endif

#if NET5_0_OR_GREATER
// Half.
((ReadOnlySpan<char>)"1.23").ToHalf().Should().Be((Half)1.23, Reason());
"1.23".ToHalf().Should().Be((Half)1.23, Reason());
new Action(() => invalidString.ToHalf()).Should().Throw<ConvertException>(Reason());
nullString.ToHalf().Should().BeNull(Reason());
#endif

// Float.
((ReadOnlySpan<char>)"1.23").ToFloat().Should().Be(1.23f, Reason());
"1.23".ToFloat().Should().Be(1.23f, Reason());
new Action(() => invalidString.ToFloat()).Should().Throw<ConvertException>(Reason());
nullString.ToFloat().Should().BeNull(Reason());

// Double.
((ReadOnlySpan<char>)"1.23").ToDouble().Should().Be(1.23, Reason());
"1.23".ToDouble().Should().Be(1.23, Reason());
new Action(() => invalidString.ToDouble()).Should().Throw<ConvertException>(Reason());
nullString.ToDouble().Should().BeNull(Reason());

// Decimal.
((ReadOnlySpan<char>)"1").ToDecimal().Should().Be(1m, Reason());
"1".ToDecimal().Should().Be(1m, Reason());
new Action(() => invalidString.ToDecimal()).Should().Throw<ConvertException>(Reason());
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

new Action(() => invalidString.ToGuid()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToGuid(guidFormat)).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToGuid(guidFormats)).Should().Throw<ConvertException>(Reason());

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

new Action(() => invalidString.ToDateTime()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToDateTime(dateTimeFormats)).Should().Throw<ConvertException>(Reason());

nullString.ToDateTime().Should().BeNull(Reason());
nullString.ToDateTime(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTime(dateTimeFormats).Should().BeNull(Reason());

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
	isoTime.ToUtcDateTime().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
	((ReadOnlySpan<char>)isoTime).ToUtcDateTime().Should().NotBeNull(Reason()).And.Subject?.Kind.Should().Be(DateTimeKind.Utc, Reason());
}

nullString.ToUtcDateTime().Should().BeNull(Reason());

var expectedUtcDateTime = new DateTime(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);

dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString()
	.ToUtcDateTimeFromUnixTimeSeconds()
	.Should()
	.Be(expectedUtcDateTime, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString())
	.ToUtcDateTimeFromUnixTimeSeconds()
	.Should()
	.Be(expectedUtcDateTime, Reason());

new Action(() => invalidString.ToUtcDateTimeFromUnixTimeSeconds()).Should().Throw<ConvertException>(Reason());
nullString.ToUtcDateTimeFromUnixTimeSeconds().Should().BeNull(Reason());

expectedUtcDateTime = new DateTime(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);

dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString()
	.ToUtcDateTimeFromUnixTimeMilliseconds()
	.Should()
	.Be(expectedUtcDateTime, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString())
	.ToUtcDateTimeFromUnixTimeMilliseconds()
	.Should()
	.Be(expectedUtcDateTime, Reason());

new Action(() => invalidString.ToUtcDateTimeFromUnixTimeMilliseconds()).Should().Throw<ConvertException>(Reason());
nullString.ToUtcDateTimeFromUnixTimeMilliseconds().Should().BeNull(Reason());

// DateTimeOffset.
((ReadOnlySpan<char>)expectedDateTime.ToString()).ToDateTimeOffset().Should().Be(expectedDateTime, Reason());
expectedDateTime.ToString().ToDateTimeOffset().Should().Be(expectedDateTime, Reason());

((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffset((ReadOnlySpan<char>)dateTimeFormat).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffset(dateTimeFormat).Should().Be(expectedDateTime, Reason());
((ReadOnlySpan<char>)dateTimeString).ToDateTimeOffset(dateTimeFormats).Should().Be(expectedDateTime, Reason());
dateTimeString.ToDateTimeOffset(dateTimeFormats).Should().Be(expectedDateTime, Reason());

new Action(() => invalidString.ToDateTimeOffset()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToDateTimeOffset(dateTimeFormats)).Should().Throw<ConvertException>(Reason());

nullString.ToDateTimeOffset().Should().BeNull(Reason());
nullString.ToDateTimeOffset(dateTimeFormat).Should().BeNull(Reason());
nullString.ToDateTimeOffset(dateTimeFormats).Should().BeNull(Reason());

var expectedUtcDateTimeOffset = new DateTimeOffset(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerSecond), TimeSpan.Zero);

dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString()
	.ToDateTimeOffsetFromUnixTimeSeconds()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeSeconds().ToString())
	.ToDateTimeOffsetFromUnixTimeSeconds()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());

new Action(() => invalidString.ToDateTimeOffsetFromUnixTimeSeconds()).Should().Throw<ConvertException>(Reason());
nullString.ToDateTimeOffsetFromUnixTimeSeconds().Should().BeNull(Reason());

expectedUtcDateTimeOffset = new DateTimeOffset(dateTimeOffsetUtcNow.Ticks - (dateTimeOffsetUtcNow.Ticks % TimeSpan.TicksPerMillisecond), TimeSpan.Zero);

dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString()
	.ToDateTimeOffsetFromUnixTimeMilliseconds()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());
((ReadOnlySpan<char>)dateTimeOffsetUtcNow.ToUnixTimeMilliseconds().ToString())
	.ToDateTimeOffsetFromUnixTimeMilliseconds()
	.Should()
	.Be(expectedUtcDateTimeOffset, Reason());

new Action(() => invalidString.ToDateTimeOffsetFromUnixTimeMilliseconds()).Should().Throw<ConvertException>(Reason());
nullString.ToDateTimeOffsetFromUnixTimeMilliseconds().Should().BeNull(Reason());

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

new Action(() => invalidString.ToTimeSpan()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToTimeSpan(timeSpanFormat)).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToTimeSpan(timeSpanFormats)).Should().Throw<ConvertException>(Reason());

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

new Action(() => invalidString.ToDateOnly()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToDateOnly(dateOnlyFormats)).Should().Throw<ConvertException>(Reason());

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

new Action(() => invalidString.ToTimeOnly()).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToTimeOnly(timeOnlyFormat)).Should().Throw<ConvertException>(Reason());
new Action(() => invalidString.ToTimeOnly(timeOnlyFormats)).Should().Throw<ConvertException>(Reason());

nullString.ToTimeOnly().Should().BeNull(Reason());
nullString.ToTimeOnly(timeOnlyFormat).Should().BeNull(Reason());
nullString.ToTimeOnly(timeOnlyFormats).Should().BeNull(Reason());
#endif
