@echo off
echo ========================================
echo CompanyLock - Create Distribution Package
echo ========================================
echo [INFO] Creating professional distribution package
echo.

REM Ensure we have the latest build
echo [STEP 1] Building latest version with icon support...
if exist "assets\icons\CompanyLock.ico" (
    echo [INFO] Custom icon detected - building with branding
) else (
    echo [INFO] No custom icon - using default Windows icon
)

REM Build all components
call build_with_icon.bat

REM Clean up old distribution files
echo.
echo [STEP 2] Cleaning up old distribution files...
del "CompanyLock-Enterprise-*.zip" 2>nul

REM Create distribution directory
echo [STEP 3] Preparing distribution package...
if exist "dist" rmdir /S /Q "dist"
mkdir "dist"
mkdir "dist\CompanyLock-Enterprise"

REM Copy installer files
echo [INFO] Copying installer components...
xcopy /E /Q "installer\CompanyLock-Enterprise\" "dist\CompanyLock-Enterprise\" >nul

REM Copy documentation and sample data
echo [INFO] Copying documentation...
copy "README-ENTERPRISE.md" "dist\CompanyLock-Enterprise\README.md" >nul
copy "sample_employees.csv" "dist\CompanyLock-Enterprise\sample_employees.csv" >nul

REM Create version info
echo [INFO] Creating version information...
echo CompanyLock Enterprise Security System > "dist\CompanyLock-Enterprise\VERSION.txt"
echo Build Date: %date% %time% >> "dist\CompanyLock-Enterprise\VERSION.txt"
echo Platform: Windows 10/11 + Ghost Spectre >> "dist\CompanyLock-Enterprise\VERSION.txt"
echo Components: Agent, AdminTool, UI >> "dist\CompanyLock-Enterprise\VERSION.txt"

REM Create deployment checklist
echo [INFO] Creating deployment checklist...
echo # CompanyLock Deployment Checklist > "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo. >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo ## Pre-Installation >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Administrator privileges available >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Target systems meet requirements (Windows 10/11) >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Employee data ready (CSV format) >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Network/antivirus exceptions configured >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo. >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo ## Installation >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Run Install-Simple-Startup.bat as Administrator >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Wait for installation completion >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Restart system >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo. >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo ## Post-Installation >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Verify Agent running in Task Manager >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Test Ctrl+Alt+L lock screen >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Configure AdminTool with employee data >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"
echo - [ ] Test employee login >> "dist\CompanyLock-Enterprise\DEPLOYMENT-CHECKLIST.md"

REM Get current date for filename
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "datestamp=%YYYY%-%MM%-%DD%"

REM Create ZIP package
echo.
echo [STEP 4] Creating ZIP distribution package...
set "zipname=CompanyLock-Enterprise-%datestamp%.zip"
powershell "Compress-Archive -Path 'dist\CompanyLock-Enterprise\*' -DestinationPath '%zipname%' -Force"

if exist "%zipname%" (
    echo [SUCCESS] Distribution package created: %zipname%
    
    REM Show package info
    echo.
    echo ========================================
    echo Package Information
    echo ========================================
    echo Package: %zipname%
    for %%A in ("%zipname%") do echo Size: %%~zA bytes
    echo Created: %date% %time%
    echo.
    echo [CONTENTS]
    echo - Install-Simple-Startup.bat    [Main installer]
    echo - Uninstall-Complete.bat        [Complete removal]
    echo - README.md                     [Installation guide]
    echo - DEPLOYMENT-CHECKLIST.md      [Deployment steps]
    echo - sample_employees.csv          [Employee data template]
    echo - VERSION.txt                  [Build information]
    echo - Agent/                       [Background service]
    echo - AdminTool/                   [Management interface]
    echo - UI/                          [Lock screen interface]
    
) else (
    echo [ERROR] Failed to create ZIP package
)

REM Cleanup temp directory
echo.
echo [STEP 5] Cleaning up temporary files...
rmdir /S /Q "dist" 2>nul

echo.
echo ========================================
echo Distribution Package Ready!
echo ========================================
echo.
echo [NEXT STEPS]
echo 1. Test the ZIP package on a clean system
echo 2. Distribute to target computers
echo 3. Follow DEPLOYMENT-CHECKLIST.md for setup
echo.
echo [ENTERPRISE DEPLOYMENT]
echo - Package is self-contained (no .NET required)
echo - Compatible with Windows 10/11 and Ghost Spectre
echo - Includes complete uninstaller
echo - Professional documentation included
echo.
pause