@set version=6.0.0
@set zip="%ProgramFiles%\7-Zip\7z.exe"
@set output="CsvLINQPadDriver.%version%.lpx6"

del %output%

set releaseDir=..\bin\Release\netcoreapp3.1

%zip% a -tzip -mx=9 "%output%" header.xml %releaseDir%\CsvLINQPadDriver.dll %releaseDir%\CsvHelper.dll %releaseDir%\Connection.png %releaseDir%\FailedConnection.png ..\README.md

@echo Package %output% created.
@pause
