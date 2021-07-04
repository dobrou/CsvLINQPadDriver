Same.Distinct()
	.Should()
	.HaveCount(
		context.StringComparison.ToString().EndsWith("IgnoreCase")
			? 1
			: 3,
		Reason());
