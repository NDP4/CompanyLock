@echo off
title CompanyLock Installation
echo ========================================
echo   CompanyLock Enterprise Security
echo        Installation Wizard
echo ========================================
echo.
echo This will install CompanyLock to your computer.
echo.
pause

echo [1/5] Creating directories...
set INSTALL_DIR=%ProgramFiles%\CompanyLock
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
mkdir "%INSTALL_DIR%\AdminTool"
mkdir "%INSTALL_DIR%\Agent"
mkdir "%INSTALL_DIR%\UI"
mkdir "%INSTALL_DIR%\Documentation"

echo [2/5] Copying files...
xcopy /Y AdminTool\* "%INSTALL_DIR%\AdminTool\"
xcopy /Y Agent\* "%INSTALL_DIR%\Agent\"
xcopy /Y UI\* "%INSTALL_DIR%\UI\"
xcopy /Y Documentation\* "%INSTALL_DIR%\Documentation\"
copy sample_employees.csv "%INSTALL_DIR%\"

echo [3/5] Installing service...
sc create "CompanyLock Agent" binPath="%INSTALL_DIR%\Agent\CompanyLock.Agent.exe" start=auto

echo [4/5] Creating shortcuts...
powershell "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\CompanyLock AdminTool.lnk'^); $Shortcut.TargetPath = '%INSTALL_DIR%\AdminTool\CompanyLock.AdminTool.exe'; $Shortcut.Save(^)"
powershell "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\CompanyLock Lock Screen.lnk'^); $Shortcut.TargetPath = '%INSTALL_DIR%\UI\CompanyLock.UI.exe'; $Shortcut.Save(^)"

echo [5/5] Starting service...
sc start "CompanyLock Agent"

echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo Desktop shortcuts created:
echo - CompanyLock AdminTool
echo - CompanyLock Lock Screen  
echo.
echo Default login: admin / admin123
echo.
pause
