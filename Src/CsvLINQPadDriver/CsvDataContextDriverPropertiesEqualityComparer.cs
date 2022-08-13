using System.Collections.Generic;
using System.Linq;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver
{
    internal class CsvDataContextDriverPropertiesEqualityComparer : IEqualityComparer<ICsvDataContextDriverProperties>
    {
        public static readonly CsvDataContextDriverPropertiesEqualityComparer Default = new();

        public bool Equals(ICsvDataContextDriverProperties? x, ICsvDataContextDriverProperties? y)
        {
            if (ReferenceEquals(x, y))  return true;
            if (x is null || y is null) return false;

            return PropertiesEqual().All(Pass);

            IEnumerable<bool> PropertiesEqual()
            {
                yield return GetFiles(x.ParsedFiles).SequenceEqual(GetFiles(y.ParsedFiles), FileExtensions.FileNameComparer);

                yield return x.FilesOrderBy       == y.FilesOrderBy;
                yield return x.NoBomEncoding      == y.NoBomEncoding;
                yield return x.AutoDetectEncoding == y.AutoDetectEncoding;
                yield return x.IgnoreInvalidFiles == y.IgnoreInvalidFiles;

                yield return x.UseCsvHelperSeparatorAutoDetection == y.UseCsvHelperSeparatorAutoDetection;
                if (!x.UseCsvHelperSeparatorAutoDetection && !y.UseCsvHelperSeparatorAutoDetection)
                {
                    yield return x.SafeCsvSeparator == y.SafeCsvSeparator;
                }

                yield return x.IgnoreBadData    == y.IgnoreBadData;
                yield return x.IgnoreBlankLines == y.IgnoreBlankLines;

                yield return x.AllowSkipLeadingRows == y.AllowSkipLeadingRows;
                if (x.AllowSkipLeadingRows && y.AllowSkipLeadingRows)
                {
                    yield return x.SkipLeadingRowsCount == y.SkipLeadingRowsCount;
                }

                yield return x.TrimSpaces == y.TrimSpaces;
                if (x.TrimSpaces && y.TrimSpaces)
                {
                    yield return x.WhitespaceTrimOptions == y.WhitespaceTrimOptions;
                }

                yield return x.AllowComments == y.AllowComments;
                if (x.AllowComments && y.AllowComments)
                {
                    yield return x.CommentChar == y.CommentChar;
                }

                yield return x.AddHeader == y.AddHeader;
                if (x.AddHeader && y.AddHeader)
                {
                    yield return x.HeaderDetection == y.HeaderDetection &&
                                 x.HeaderFormat    == y.HeaderFormat;
                }

                yield return x.IsCacheEnabled == y.IsCacheEnabled;

                yield return x.IsStringInternEnabled == y.IsStringInternEnabled;
                if (x.IsStringInternEnabled && y.IsStringInternEnabled)
                {
                    yield return x.UseStringComparerForStringIntern == y.UseStringComparerForStringIntern;
                }

                yield return x.UseRecordType              == y.UseRecordType;
                yield return x.UseSingleClassForSameFiles == y.UseSingleClassForSameFiles;
                yield return x.StringComparison           == y.StringComparison;

                yield return x.DetectRelations == y.DetectRelations;

                IEnumerable<string> GetFiles(IEnumerable<string> files) =>
                    x.FilesOrderBy == FilesOrderBy.None
                        ? files
                        : files.OrderBy(Pass, FileExtensions.FileNameComparer);
            }

            static T Pass<T>(T t) => t;
        }

        public int GetHashCode(ICsvDataContextDriverProperties obj) =>
            obj.GetHashCode();
    }
}