rem
rem  You can simplify development by updating this batch file and then calling it from the 
rem  project's post-build event.
rem
rem  It copies the output .DLL (and .PDB) to LINQPad's drivers folder, so that LINQPad
rem  picks up the drivers immediately (without needing to click 'Add Driver').
rem
rem  The final part of the directory is the name of the assembly.

set driverDir="%LOCALAPPDATA%\LINQPad\Drivers\DataContext\NetCore\CsvLINQPadDriver"

mkdir %driverDir%

for %%e in (CsvLINQPadDriver.dll CsvLINQPadDriver.deps.json Connection.png FailedConnection.png) do xcopy /i/y %%e %driverDir%
