@echo off
Taskkill /IM WinGrid.exe
cd . > nul
rmdir /S /Q "C:\Program Files\WinGrid"
IF NOT %errorlevel%==0  (GOTO Err)
reg DELETE HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run /f /v WinGrid
IF %errorlevel%==1  (GOTO Err)

echo Uninstalled Successfully
GOTO End
:Err
echo Uninstall failed. Try running as Admin.
:End
pause