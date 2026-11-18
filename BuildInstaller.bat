@echo off
setlocal
cd /d "%~dp0"
call Build.bat
if errorlevel 1 exit /b 1
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\ApplePlayer.iss
exit /b %errorlevel%
