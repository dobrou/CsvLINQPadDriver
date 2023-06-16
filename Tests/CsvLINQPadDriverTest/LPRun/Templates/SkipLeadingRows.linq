const string header = "Header";

SkipLeadingRows.First()[header].Should().Be("Value1", Reason());
SkipLeadingRows. Last()[header].Should().Be("Value2", Reason());
