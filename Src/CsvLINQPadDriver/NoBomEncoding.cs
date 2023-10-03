using System.ComponentModel;

// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver;

public enum NoBomEncoding
{
    [Description("UTF-8｜65001")]
    UTF8,

    [Description("UTF-16｜1200")]
    Unicode,

    [Description("UTF-16 Big Endian｜1201")]
    BigEndianUnicode,

    [Description("UTF-32｜12000")]
    UTF32,

    [Description("UTF-32 Big Endian｜12001")]
    BigEndianUTF32,

    [Description("UTF-7｜65000")]
    UTF7,

    [Description("ASCII｜20127")]
    ASCII,

    [Description("System сode page")]
    SystemCodePage,

    [Description("User сode page")]
    UserCodePage,

    [Description("Arabic (ASMO 708)｜708")]
    Cp708,

    [Description("Arabic (ASMO-449+)｜709")]
    Cp709,

    [Description("Arabic (DOS)｜720")]
    Cp720,

    [Description("Arabic (Mac)｜10004")]
    Cp10004,

    [Description("Arabic (OEM)｜864")]
    Cp864,

    [Description("Arabic (Windows)｜1256")]
    Cp1256,

    [Description("Arabic - Transparent Arabic｜710")]
    Cp710,

    [Description("Baltic (DOS)｜775")]
    Cp775,

    [Description("Baltic (Windows)｜1257")]
    Cp1257,

    [Description("Central European (DOS)｜852")]
    Cp852,

    [Description("Central European (Mac)｜10029")]
    Cp10029,

    [Description("Central European (Windows)｜1250")]
    Cp1250,

    [Description("Chinese Simplified (EUC)｜51936")]
    Cp51936,

    [Description("Chinese Simplified (GB18030)｜54936")]
    Cp54936,

    [Description("Chinese Simplified (GB2312)｜936")]
    Cp936,

    [Description("Chinese Simplified (GB2312-80)｜20936")]
    Cp20936,

    [Description("Chinese Simplified (HZ)｜52936")]
    Cp52936,

    [Description("Chinese Simplified (ISO 2022)｜50227")]
    Cp50227,

    [Description("Chinese Simplified (Mac)｜10008")]
    Cp10008,

    [Description("Chinese Traditional (Big5)｜950")]
    Cp950,

    [Description("Chinese Traditional (CNS)｜20000")]
    Cp20000,

    [Description("Chinese Traditional (Eten)｜20002")]
    Cp20002,

    [Description("Chinese Traditional (Mac)｜10002")]
    Cp10002,

    [Description("Croatian (Mac)｜10082")]
    Cp10082,

    [Description("Cyrillic (DOS)｜866")]
    Cp866,

    [Description("Cyrillic (KOI8-R)｜20866")]
    Cp20866,

    [Description("Cyrillic (KOI8-U)｜21866")]
    Cp21866,

    [Description("Cyrillic (Mac)｜10007")]
    Cp10007,

    [Description("Cyrillic (OEM)｜855")]
    Cp855,

    [Description("Cyrillic (Windows)｜1251")]
    Cp1251,

    [Description("EBCDIC Japanese (Katakana) Extended｜50930")]
    Cp50930,

    [Description("EBCDIC Japanese (Latin) Extended and Japanese｜50939")]
    Cp50939,

    [Description("EBCDIC Korean Extended and Korean｜50933")]
    Cp50933,

    [Description("EBCDIC Simplified Chinese｜50936")]
    Cp50936,

    [Description("EBCDIC Simplified Chinese Extended｜50935")]
    Cp50935,

    [Description("EBCDIC US-Canada and Japanese｜50931")]
    Cp50931,

    [Description("EBCDIC US-Canada and Traditional Chinese｜50937")]
    Cp50937,

    [Description("Europa 3｜29001")]
    Cp29001,

    [Description("French Canadian (DOS)｜863")]
    Cp863,

    [Description("Greek (DOS)｜737")]
    Cp737,

    [Description("Greek (Mac)｜10006")]
    Cp10006,

    [Description("Greek (Windows)｜1253")]
    Cp1253,

    [Description("Greek Modern (DOS)｜869")]
    Cp869,

    [Description("Hebrew (DOS)｜862")]
    Cp862,

    [Description("Hebrew (ISO-Logical)｜38598")]
    Cp38598,

    [Description("Hebrew (Mac)｜10005")]
    Cp10005,

    [Description("Hebrew (Windows)｜1255")]
    Cp1255,

    [Description("IA5 German (7-bit)｜20106")]
    Cp20106,

    [Description("IA5 Norwegian (7-bit)｜20108")]
    Cp20108,

    [Description("IA5 Swedish (7-bit)｜20107")]
    Cp20107,

    [Description("IBM EBCDIC (Denmark-Norway-Euro)｜1142")]
    Cp1142,

    [Description("IBM EBCDIC (Finland-Sweden-Euro)｜1143")]
    Cp1143,

    [Description("IBM EBCDIC (France-Euro)｜1147")]
    Cp1147,

    [Description("IBM EBCDIC (Germany-Euro)｜1141")]
    Cp1141,

    [Description("IBM EBCDIC (Icelandic-Euro)｜1149")]
    Cp1149,

    [Description("IBM EBCDIC (International-Euro)｜1148")]
    Cp1148,

    [Description("IBM EBCDIC (Italy-Euro)｜1144")]
    Cp1144,

    [Description("IBM EBCDIC (Spain-Euro)｜1145")]
    Cp1145,

    [Description("IBM EBCDIC (UK-Euro)｜1146")]
    Cp1146,

    [Description("IBM EBCDIC (US-Canada-Euro)｜1140")]
    Cp1140,

    [Description("IBM EBCDIC Arabic｜20420")]
    Cp20420,

    [Description("IBM EBCDIC Cyrillic Russian｜20880")]
    Cp20880,

    [Description("IBM EBCDIC Cyrillic Serbian-Bulgarian｜21025")]
    Cp21025,

    [Description("IBM EBCDIC Denmark-Norway｜20277")]
    Cp20277,

    [Description("IBM EBCDIC Finland-Sweden｜20278")]
    Cp20278,

    [Description("IBM EBCDIC France｜20297")]
    Cp20297,

    [Description("IBM EBCDIC Germany｜20273")]
    Cp20273,

    [Description("IBM EBCDIC Greek｜20423")]
    Cp20423,

    [Description("IBM EBCDIC Greek Modern｜875")]
    Cp875,

    [Description("IBM EBCDIC Hebrew｜20424")]
    Cp20424,

    [Description("IBM EBCDIC Icelandic｜20871")]
    Cp20871,

    [Description("IBM EBCDIC International｜500")]
    Cp500,

    [Description("IBM EBCDIC Italy｜20280")]
    Cp20280,

    [Description("IBM EBCDIC Japanese Katakana Extended｜20290")]
    Cp20290,

    [Description("IBM EBCDIC Korean Extended｜20833")]
    Cp20833,

    [Description("IBM EBCDIC Latin 1/Open System｜1047")]
    Cp1047,

    [Description("IBM EBCDIC Latin 1/Open System (1047 + Euro)｜20924")]
    Cp20924,

    [Description("IBM EBCDIC Latin America-Spain｜20284")]
    Cp20284,

    [Description("IBM EBCDIC Multilingual Latin 2｜870")]
    Cp870,

    [Description("IBM EBCDIC Thai｜20838")]
    Cp20838,

    [Description("IBM EBCDIC Turkish｜20905")]
    Cp20905,

    [Description("IBM EBCDIC Turkish (Latin 5)｜1026")]
    Cp1026,

    [Description("IBM EBCDIC United Kingdom｜20285")]
    Cp20285,

    [Description("IBM EBCDIC US-Canada｜037")]
    Cp037,

    [Description("IBM5550 Taiwan｜20003")]
    Cp20003,

    [Description("Icelandic (DOS)｜861")]
    Cp861,

    [Description("Icelandic (Mac)｜10079")]
    Cp10079,

    [Description("ISCII Assamese｜57006")]
    Cp57006,

    [Description("ISCII Bangla｜57003")]
    Cp57003,

    [Description("ISCII Devanagari｜57002")]
    Cp57002,

    [Description("ISCII Gujarati｜57010")]
    Cp57010,

    [Description("ISCII Kannada｜57008")]
    Cp57008,

    [Description("ISCII Malayalam｜57009")]
    Cp57009,

    [Description("ISCII Odia｜57007")]
    Cp57007,

    [Description("ISCII Punjabi｜57011")]
    Cp57011,

    [Description("ISCII Tamil｜57004")]
    Cp57004,

    [Description("ISCII Telugu｜57005")]
    Cp57005,

    [Description("ISO 6937 Non-Spacing Accent｜20269")]
    Cp20269,

    [Description("ISO 8859-1 Latin 1｜28591")]
    Cp28591,

    [Description("ISO 8859-2 Central European｜28592")]
    Cp28592,

    [Description("ISO 8859-3 Latin 3｜28593")]
    Cp28593,

    [Description("ISO 8859-4 Baltic｜28594")]
    Cp28594,

    [Description("ISO 8859-5 Cyrillic｜28595")]
    Cp28595,

    [Description("ISO 8859-6 Arabic｜28596")]
    Cp28596,

    [Description("ISO 8859-7 Greek｜28597")]
    Cp28597,

    [Description("ISO 8859-8 Hebrew｜28598")]
    Cp28598,

    [Description("ISO 8859-9 Turkish｜28599")]
    Cp28599,

    [Description("ISO 8859-13 Estonian｜28603")]
    Cp28603,

    [Description("ISO 8859-15 Latin 9｜28605")]
    Cp28605,

    [Description("Japanese (EUC)｜51932")]
    Cp51932,

    [Description("Japanese (JIS 0208-1990 and 0212-1990)｜20932")]
    Cp20932,

    [Description("Japanese (JIS)｜50220")]
    Cp50220,

    [Description("Japanese (JIS-Allow 1 byte Kana - SO/SI)｜50222")]
    Cp50222,

    [Description("Japanese (JIS-Allow 1 byte Kana)｜50221")]
    Cp50221,

    [Description("Japanese (Mac)｜10001")]
    Cp10001,

    [Description("Japanese (Shift-JIS)｜932")]
    Cp932,

    [Description("Korean｜50225")]
    Cp50225,

    [Description("Korean (EUC)｜51949")]
    Cp51949,

    [Description("Korean (Johab)｜1361")]
    Cp1361,

    [Description("Korean (Mac)｜10003")]
    Cp10003,

    [Description("Korean (Unified Hangul Code)｜949")]
    Cp949,

    [Description("Korean Wansung｜20949")]
    Cp20949,

    [Description("Multilingual Latin 1 (OEM)｜858")]
    Cp858,

    [Description("Nordic (DOS)｜865")]
    Cp865,

    [Description("Portuguese (DOS)｜860")]
    Cp860,

    [Description("Romanian (Mac)｜10010")]
    Cp10010,

    [Description("T.61｜20261")]
    Cp20261,

    [Description("TCA Taiwan｜20001")]
    Cp20001,

    [Description("TeleText Taiwan｜20004")]
    Cp20004,

    [Description("Thai (Mac)｜10021")]
    Cp10021,

    [Description("Thai (Windows)｜874")]
    Cp874,

    [Description("Traditional Chinese｜50229")]
    Cp50229,

    [Description("Traditional Chinese (EUC)｜51950")]
    Cp51950,

    [Description("Turkish (DOS)｜857")]
    Cp857,

    [Description("Turkish (Mac)｜10081")]
    Cp10081,

    [Description("Turkish (Windows)｜1254")]
    Cp1254,

    [Description("Ukrainian (Mac)｜10017")]
    Cp10017,

    [Description("United States (OEM)｜437")]
    Cp437,

    [Description("Vietnamese (Windows)｜1258")]
    Cp1258,

    [Description("Wang Taiwan｜20005")]
    Cp20005,

    [Description("Western European (DOS)｜850")]
    Cp850,

    [Description("Western European (IA5)｜20105")]
    Cp20105,

    [Description("Western European (Mac)｜10000")]
    Cp10000,

    [Description("Western European (Windows)｜1252")]
    Cp1252
}
