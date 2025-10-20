@echo off
echo ========================================
echo CompanyLock Agent - Cleanup Tool
echo ========================================

echo [INFO] Stopping all CompanyLock Agent instances...

REM Kill all CompanyLock Agent processes
tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" 2>NUL | find /I /N "CompanyLock.Agent.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [INFO] Found running Agent instances - stopping them...
    taskkill /F /IM "CompanyLock.Agent.exe" /T >nul 2>&1
    timeout /t 3 /nobreak >nul
    
    REM Verify all instances are stopped
    tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" 2>NUL | find /I /N "CompanyLock.Agent.exe">NUL
    if "%ERRORLEVEL%"=="0" (
        echo [WARNING] Some instances may still be running
        echo [INFO] Please check Task Manager manually
    ) else (
        echo [SUCCESS] All Agent instances stopped
    )
) else (
    echo [OK] No running instances found
)

echo.
echo [INFO] Starting single Agent instance in background...
cd /d "C:\Program Files\CompanyLock\Agent"

REM Start agent silently
if exist "C:\Program Files\CompanyLock\StartAgentSilent.vbs" (
    cscript //nologo "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
) else (
    start /min /b "CompanyLock Agent" CompanyLock.Agent.exe
)

timeout /t 3 /nobreak >nul

REM Verify single instance is running
tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" /FO CSV 2>NUL | find /V "INFO:" | find /C "CompanyLock.Agent.exe" >nul
set AGENT_COUNT=%errorlevel%

echo [INFO] Checking running instances...
tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" 2>NUL | find /I "CompanyLock.Agent.exe"

if %AGENT_COUNT% equ 1 (
    echo [SUCCESS] Single Agent instance is now running
) else (
    echo [INFO] Agent startup in progress or multiple instances detected
)

echo.
echo ========================================
echo Cleanup Complete!
echo ========================================
pause