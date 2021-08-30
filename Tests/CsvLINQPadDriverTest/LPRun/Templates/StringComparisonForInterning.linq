new [] { Comments, Same }.SelectMany(_ => _)
	.Distinct()
	.Should()
	.HaveCount(
		context.StringComparison.ToString().EndsWith("IgnoreCase")
			? 2
			: 5,
		Reason());
