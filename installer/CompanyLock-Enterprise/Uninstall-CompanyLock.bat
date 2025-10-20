@echo off
echo ======================================
echo CompanyLock Uninstaller
echo ======================================
echo.
echo [WARNING] This will completely remove CompanyLock from your system
echo [WARNING] All local database data will be preserved in %LOCALAPPDATA%\CompanyLock
echo.
set /p "CONFIRM=Are you sure you want to uninstall? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo Uninstall cancelled.
    pause
    exit /b 0
)

echo [STEP 1] Stopping service...
sc stop "CompanyLock Agent"
timeout /t 3 /nobreak > nul

echo [STEP 2] Removing service...
sc delete "CompanyLock Agent"

echo [STEP 3] Removing application files...
set "INSTALL_DIR=%ProgramFiles%\CompanyLock"
if exist "%INSTALL_DIR%" (
    rmdir /s /q "%INSTALL_DIR%"
    echo [SUCCESS] Application files removed
) else (
    echo [INFO] Application files not found
)

echo [STEP 4] Removing desktop shortcuts...
if exist "%USERPROFILE%\Desktop\CompanyLock AdminTool.lnk" del "%USERPROFILE%\Desktop\CompanyLock AdminTool.lnk"
if exist "%USERPROFILE%\Desktop\CompanyLock LockScreen.lnk" del "%USERPROFILE%\Desktop\CompanyLock LockScreen.lnk"

echo ======================================
echo Uninstall Complete!
echo ======================================
echo.
echo [INFO] Database files preserved in: %LOCALAPPDATA%\CompanyLock
echo [INFO] You can manually delete this folder if you want to remove all data
echo.
pause
