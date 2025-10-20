@echo off
title CompanyLock Uninstaller
echo Removing CompanyLock...

sc stop "CompanyLock Agent"
sc delete "CompanyLock Agent"

set INSTALL_DIR=%ProgramFiles%\CompanyLock
if exist "%INSTALL_DIR%" rmdir /s /q "%INSTALL_DIR%"

del "%USERPROFILE%\Desktop\CompanyLock AdminTool.lnk"
del "%USERPROFILE%\Desktop\CompanyLock Lock Screen.lnk"

echo Uninstall complete!
pause
