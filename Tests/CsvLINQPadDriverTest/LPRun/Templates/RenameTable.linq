GetType()
	.GetProperties()
	.Where(p => p.DeclaringType.IsAssignableFrom(typeof(CsvDataContext)))
	.Should()
	.OnlyContain(p => p.Name.StartsWith("table_"), Reason());

table_1.Should().NotBeEmpty(Reason());
