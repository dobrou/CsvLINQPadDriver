var data = context.SelectMany(_ => _).ToList();

var properties = data.First().GetType().GetProperties()
	.Where(property => property.Name != "Item")
	.Select(property => property.Name);

data.Should().HaveCount(5, Reason());
data.Distinct().Should().HaveCount(1, Reason());

data.SelectMany(row => properties.Select(property => row[property] == property))
	.Should()
	.OnlyContain(trueValue => trueValue, Reason());
