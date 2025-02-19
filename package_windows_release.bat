cd c:\
rmdir /S /Q ladder99
mkdir ladder99\fanuc-driver\logs
mkdir ladder99\fanuc-driver\user
cd ladder99\fanuc-driver

copy "%~dp0fanuc\bin\Release 32 bit\net8.0\win-x86\*.*" .
copy "%~dp0examples\windows\*.*" .\user

cd \
mkdir ladder99-artifacts
cd ladder99-artifacts

@echo off
set /p "archive_version=Archive Version: "

tar.exe -a -cf fanuc-driver-%archive_version%-windows32.zip c:\ladder99

pause