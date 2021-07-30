var actual =
	from book in Books
	join author in Authors on book.AuthorId equals author.Id
	select (author.Name, book.Title);

var expected = new[]
{
	("Author 1", "Author 1 Book 1"),
	("Author 1", "Author 1 Book 2"),
	("Author 2", "Author 2 Book 1")
};

actual.Should().BeEquivalentTo(expected, Reason());

Authors.ToList().ForEach(
	author => author.Books.Should().Equal(Books.Where(book => book.AuthorId == author.Id), Reason()));

Books.ToList().ForEach(
	book => book.Authors.Should().Equal(Authors.Where(author => author.Id == book.AuthorId), Reason()));
