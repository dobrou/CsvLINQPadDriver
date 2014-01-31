@set version=1.5
@set zip="%ProgramFiles%\7-Zip\7z.exe"
@set output="CsvLINQPadDriver.v%version%.lpx"

del %output%

%zip% a -tzip "%output%" header.xml ..\bin\Debug\CsvLINQPadDriver.dll ..\bin\Debug\CsvHelper.dll ..\bin\Debug\Connection.png ..\bin\Debug\FailedConnection.png ..\README.md ..\bin\Debug\CsvLINQPadFileOpen.exe 
copy /y ..\bin\Debug\CsvLINQPadFileOpen.exe CsvLINQPadFileOpen.v%version%.exe

@echo Package %output% created.
@pause


