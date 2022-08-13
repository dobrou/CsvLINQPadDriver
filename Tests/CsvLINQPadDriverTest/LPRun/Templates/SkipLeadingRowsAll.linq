this.GetType()
	.GetProperties()
	.Where(p => p.DeclaringType.ToString() == "LINQPad.User.CsvDataContext")
	.Should()
	.BeEmpty(Reason());
