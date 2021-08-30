var original = Books.First();
var copy = original with { Title = original.Title.ToUpperInvariant() };

var expectedEquality = original.Title.Equals(copy.Title, context.StringComparison);

original.Equals(copy).Should().Be(expectedEquality, Reason());

original.GetHashCode()
	.Equals(copy.GetHashCode())
	.Should()
	.Be(expectedEquality, Reason());
