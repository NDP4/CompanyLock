@echo off
echo ================================================
echo CompanyLock Complete Uninstaller
echo ================================================
echo.

REM Stop all CompanyLock processes
echo [STEP 1] Stopping all CompanyLock processes...
taskkill /F /IM "CompanyLock.Agent.exe" 2>nul
taskkill /F /IM "CompanyLock.UI.exe" 2>nul
taskkill /F /IM "CompanyLock.AdminTool.exe" 2>nul
echo [OK] Processes stopped

REM Remove registry entries
echo.
echo [STEP 2] Removing registry entries...
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "CompanyLockAgent" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "CompanyLockAgent" /f >nul 2>&1
echo [OK] Registry entries removed

REM Remove startup files
echo.
echo [STEP 3] Removing startup files...
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\CompanyLockAgent.vbs" 2>nul
del "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\CompanyLockAgent.vbs" 2>nul
echo [OK] Startup files removed

REM Remove services (if any)
echo.
echo [STEP 4] Removing services...
sc delete "CompanyLockAgent" >nul 2>&1
sc delete "CompanyLock Agent Service" >nul 2>&1
sc delete "CompanyLockService" >nul 2>&1
echo [OK] Services removed

REM Remove program files
echo.
echo [STEP 5] Removing program files...
if exist "C:\Program Files\CompanyLock\" (
    rmdir /S /Q "C:\Program Files\CompanyLock\"
    echo [OK] Program files removed
) else (
    echo [INFO] Program files already removed
)

REM Remove desktop shortcuts
echo.
echo [STEP 6] Removing desktop shortcuts...
del "%USERPROFILE%\Desktop\CompanyLock AdminTool.lnk" 2>nul
del "%USERPROFILE%\Desktop\CompanyLock Lock Screen.lnk" 2>nul
del "%PUBLIC%\Desktop\CompanyLock AdminTool.lnk" 2>nul
del "%PUBLIC%\Desktop\CompanyLock Lock Screen.lnk" 2>nul
echo [OK] Desktop shortcuts removed

REM Remove database
echo.
echo [STEP 7] Removing database...
if exist "%LOCALAPPDATA%\CompanyLock\" (
    rmdir /S /Q "%LOCALAPPDATA%\CompanyLock\"
    echo [OK] Database removed
) else (
    echo [INFO] Database already removed
)

echo.
echo ================================================
echo Uninstallation completed successfully!
echo System is now completely clean.
echo ================================================
echo.
pause