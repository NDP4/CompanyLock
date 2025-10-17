@echo off
REM Uninstall CompanyLock Service
echo Uninstalling CompanyLock Service...

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Error: This script must be run as Administrator
    pause
    exit /b 1
)

REM Stop service
sc stop CompanyLockService

REM Delete service
sc delete CompanyLockService

if %errorLevel% equ 0 (
    echo Service uninstalled successfully
) else (
    echo Error: Failed to uninstall service
)

pause