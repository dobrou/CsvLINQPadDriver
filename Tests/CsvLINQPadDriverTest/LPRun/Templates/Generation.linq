// Copy.
var original = Books.First();

#if USE_RECORD_TYPE
var copy = original with {};
var modifiedCopy = copy with { Title = null };
#else
var copy = new RBook
{
	Id = original.Id,
	Title = original.Title,
	AuthorId = original.AuthorId,
	Authors = original.Authors
};

var modifiedCopy = new RBook
{
	Id = copy.Id,
	Title = null,
	AuthorId = copy.AuthorId,
	Authors = copy.Authors
};
#endif

// Reference equality.
copy.Should().NotBeSameAs(original, Reason());
copy.Should().NotBeSameAs(modifiedCopy, Reason());

// Equality.
original.Should().NotBe(Authors.First(), Reason());

copy.Should().Be(original, Reason());
copy.Should().Be((object)original, Reason());

copy.Should().NotBe(modifiedCopy, Reason());

copy.Equals(null).Should().BeFalse(Reason());

copy.Equals(original).Should().BeTrue(Reason());
copy.Equals((object)original).Should().BeTrue(Reason());

(copy == original).Should().BeTrue(Reason());
(copy != original).Should().BeFalse(Reason());

(copy is null).Should().BeFalse(Reason());
(copy is not null).Should().BeTrue(Reason());

// Hash code.
copy.GetHashCode().Should().Be(original.GetHashCode(), Reason());

// Stringify.
copy.ToString().Should().Be(string.Format("Id       : {1}{0}Title    : {2}{0}AuthorId : {3}{0}", Environment.NewLine, copy.Id, copy.Title, copy.AuthorId), Reason());

// Indexers.
copy[nameof(copy.Id)].Should().Be(copy.Id, Reason());
copy[nameof(copy.Title)].Should().Be(copy.Title, Reason());
copy[nameof(copy.AuthorId)].Should().Be(copy.AuthorId, Reason());

new Action(() => Console.WriteLine(copy["Title1"]))
	.Should()
	.Throw<IndexOutOfRangeException>(Reason())
	.WithMessage("There is no property *Title1*", Reason());

copy[0].Should().Be(copy[nameof(copy.Id)], Reason());
copy[1].Should().Be(copy[nameof(copy.Title)], Reason());
copy[2].Should().Be(copy[nameof(copy.AuthorId)], Reason());

new Action(() => Console.WriteLine(copy[3]))
	.Should()
	.Throw<IndexOutOfRangeException>(Reason())
	.WithMessage("There is no property *3*", Reason());

// Mutability.
var oldId = copy.Id;
var newId = oldId + 1;

copy.Id = newId;
copy.Id.Should().Be(newId, Reason());

copy[nameof(copy.Id)] = oldId;
copy.Id.Should().Be(oldId, Reason());

copy[0] = newId;
copy.Id.Should().Be(newId, Reason());

new Action(() => copy["Title1"] = "")
	.Should()
	.Throw<IndexOutOfRangeException>(Reason())
	.WithMessage("There is no property *Title1*", Reason());

new Action(() => copy[3] = "")
	.Should()
	.Throw<IndexOutOfRangeException>(Reason())
	.WithMessage("There is no property *3*", Reason());

// Generate single class for similar files.
Authors.First().GetType()
	.Equals(Authors2.First().GetType())
	.Should()
	.Be(context.UseSingleClassForSameFiles, Reason());
