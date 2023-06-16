@echo off

set version=7.3.18
set fileName=CsvLINQPadDriver.%version%
set ext=lpx
set ext6=%ext%6
set rootDir=%~dp0
set rootDir=%rootDir:~0,-1%

set zip="%ProgramFiles%\7-Zip\7z.exe"

echo on

call :pack %fileName%-net7.%ext6%   net7.0-windows
call :pack %fileName%-net6.%ext6%   net6.0-windows
call :pack %fileName%-net5.%ext6%   net5.0-windows
call :pack %fileName%-net3.1.%ext6% netcoreapp3.1
call :pack %fileName%.%ext%         net461

@echo off

@echo.

if not %errorlevel%==0 echo ERROR: Packaging has failed. See log for details.

exit /b %errorlevel%

:pack
@echo off

set lpx=%rootDir%\%1
set folder=%rootDir%\..\bin\Release\%2

set additional=%folder%\CsvLINQPadDriver.deps.json

if exist %lpx% del %lpx%
if exist %folder%\Microsoft.Bcl.*.dll set additional=^
%folder%\Microsoft.Bcl.HashCode.dll ^
%folder%\CsvHelper.dll ^
%folder%\Humanizer.dll ^
%folder%\Microsoft.WindowsAPICodePack.dll ^
%folder%\Microsoft.WindowsAPICodePack.Shell.dll ^
%folder%\UnicodeCharsetDetector.dll ^
%folder%\UtfUnknown.dll

echo on

%zip% a -tzip -mx=9 %lpx% ^
%rootDir%\header.xml ^
%rootDir%\..\README.md ^
%rootDir%\..\LICENSE ^
%folder%\*Connection.png ^
%folder%\CsvLINQPadDriver.dll ^
%additional% || exit /b 1

@exit /b 0
