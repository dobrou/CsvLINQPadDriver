var properties = new []
{
	"encoding", "кодировка", "الترميز", "编码", "コーディング"
};

var data = new []
{
	Utf16BomCp1200_Encoding,
	Utf16BomCp1201_Encoding,
	Utf8BomCp65001_Encoding,
	Utf8Cp65001_Encoding,
	Utf32Bom_Encoding
}.SelectMany(_ => _).ToList();

data.Should().HaveCount(5, Reason());
data.Distinct().Should().HaveCount(1, Reason());

data.SelectMany(row => properties.Select(property => row[property] == property))
	.Should()
	.OnlyContain(trueValue => trueValue, Reason());
