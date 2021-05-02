@set version=6.8.2
@set zip="%ProgramFiles%\7-Zip\7z.exe"
@set output="CsvLINQPadDriver.%version%.lpx6"

del %output%

set releaseDir=..\bin\Release\netcoreapp3.1

%zip% a -tzip -mx=9 "%output%" header.xml %releaseDir%\*.dll %releaseDir%\*Connection.png ..\README.md ..\LICENSE

@echo Package %output% created.
@pause
