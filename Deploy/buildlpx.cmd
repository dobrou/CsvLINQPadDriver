@set zip="%ProgramFiles%\7-Zip\7z.exe"
@set output="CsvLINQPadDriver.lpx"

del %output%

%zip% a -tzip "%output%" header.xml ..\Src\bin\Debug\CsvLINQPadDriver.dll ..\Src\bin\Debug\CsvHelper.dll ..\README.md ..\Tools\CsvLINQPadFileOpen\bin\Debug\CsvLINQPadFileOpen.exe

@echo Package %output% created.
@pause


