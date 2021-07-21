@echo off

set version=6.15.0
set fileName=CsvLINQPadDriver.%version%.lpx

set zip="%ProgramFiles%\7-Zip\7z.exe"

echo on

call :pack %fileName%6 netcoreapp3.1
call :pack %fileName%  net461

@echo off

echo.
pause

exit /b 0

:pack
@echo off

set lpx=%1
set folder=..\bin\Release\%2

set additional=

if exist %lpx% del %lpx%
if exist %folder%\Microsoft.Bcl.*.dll set additional=^
%folder%\Microsoft.Bcl.HashCode.dll

echo on

%zip% a -tzip -mx=9 %lpx% ^
header.xml ^
..\README.md ^
..\LICENSE ^
%folder%\*Connection.png ^
%folder%\CsvHelper.dll ^
%folder%\CsvLINQPadDriver.dll ^
%folder%\Humanizer.dll ^
%folder%\Microsoft.WindowsAPICodePack.dll ^
%folder%\UnicodeCharsetDetector.dll ^
%folder%\UtfUnknown.dll ^
%additional%

@exit /b 0
