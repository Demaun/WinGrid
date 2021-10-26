@echo off
Taskkill /IM WinGrid.exe
rmdir /S /Q "C:\Program Files\WinGrid"
cd . > nul
mkdir "C:\Program Files\WinGrid"
IF %errorlevel%==1  (GOTO Err)
xcopy %~dp0 "C:\Program Files\WinGrid\"
IF %errorlevel%==1  (GOTO Err)
reg ADD HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run /f /v WinGrid /t REG_SZ /d "C:\Program Files\WinGrid\WinGrid.exe"
IF %errorlevel%==1  (GOTO Err)

echo Installed Successfully
GOTO End
:Err
echo Install failed. Try running as Admin.
:End
pause