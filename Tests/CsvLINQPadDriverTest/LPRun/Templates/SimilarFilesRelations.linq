var actual =
	from book in new []
	{
		Books,
		Books2,
	}.SelectMany(_ => _)
	join author in new []
	{
		Authors,
		Authors2,
	}.SelectMany(_ => _) on book.AuthorId equals author.Id
	select (author.Name, book.Title);

var expected = new[]
{
	("Author 1", "Author 1 Book 1"),
	("Author 1", "Author 1 Book 2"),
	("Author 2", "Author 2 Book 1"),
	("Author 4", "Author 4 Book 1"),
	("Author 5", "Author 5 Book 1"),
	("Author 5", "Author 5 Book 2"),
	("Author 6", "Author 6 Book 1")
};

actual.Should().BeEquivalentTo(expected, Reason());
