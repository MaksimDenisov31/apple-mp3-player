@echo off
setlocal
cd /d "%~dp0"
set "FX=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319"
set "WPF=%FX%\WPF"
if not exist bin mkdir bin
"%FX%\csc.exe" /nologo /target:winexe /optimize+ /platform:anycpu /win32manifest:app.manifest /win32icon:assets\app.ico /out:bin\ApplePlayer.exe /resource:MainWindow.xaml,ApplePlayer.MainWindow.xaml /resource:ControlStyles.xaml,ApplePlayer.ControlStyles.xaml /resource:assets/app.ico,ApplePlayer.Icon /r:System.dll /r:System.Core.dll /r:System.Xaml.dll /r:System.Runtime.Serialization.dll /r:System.Windows.Forms.dll /r:"%WPF%\WindowsBase.dll" /r:"%WPF%\PresentationCore.dll" /r:"%WPF%\PresentationFramework.dll" src\*.cs
if errorlevel 1 exit /b 1
copy /y bin\ApplePlayer.exe ApplePlayer.exe >nul 2>&1
if errorlevel 1 echo Root EXE is in use; new build is available in bin\ApplePlayer.exe
exit /b 0
