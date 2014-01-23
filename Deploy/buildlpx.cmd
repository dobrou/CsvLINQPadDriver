@set version=1.4
@set zip="%ProgramFiles%\7-Zip\7z.exe"
@set output="CsvLINQPadDriver.v%version%.lpx"

del %output%

%zip% a -tzip "%output%" header.xml ..\Src\bin\Debug\CsvLINQPadDriver.dll ..\Src\bin\Debug\CsvHelper.dll ..\Src\bin\Debug\Connection.png ..\Src\bin\Debug\FailedConnection.png ..\README.md ..\Tools\CsvLINQPadFileOpen\bin\Debug\CsvLINQPadFileOpen.exe 
copy /y ..\Tools\CsvLINQPadFileOpen\bin\Debug\CsvLINQPadFileOpen.exe CsvLINQPadFileOpen.v%version%.exe

@echo Package %output% created.
@pause


