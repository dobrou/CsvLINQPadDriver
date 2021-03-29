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

Array.ForEach(Authors.ToArray(), author => author.Books.Should().BeEquivalentTo(Books.Where(book => book.AuthorId == author.Id), Reason()));
Array.ForEach(Books.ToArray(), book => book.Authors.Should().BeEquivalentTo(Authors.Where(author => author.Id == book.AuthorId), Reason()));
