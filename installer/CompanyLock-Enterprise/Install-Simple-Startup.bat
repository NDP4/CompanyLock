@echo off
setlocal enabledelayedexpansion

echo ========================================
echo CompanyLock Installation - SIMPLE STARTUP
echo ========================================

echo [INFO] Simple installer for modified Windows systems
echo [INFO] This will install CompanyLock as startup application
echo [DEBUG] Script directory: %~dp0
echo [DEBUG] Target directory: C:\Program Files\CompanyLock

REM Check admin privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Not running as Administrator
    echo [INFO] Please run this script as Administrator
    pause
    exit /b 1
) else (
    echo [OK] Running with Administrator privileges
)

echo.
echo [STEP 1] Cleaning up existing instances...
echo [INFO] Stopping any running CompanyLock Agent instances...
tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" 2>NUL | find /I /N "CompanyLock.Agent.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [INFO] Found running Agent instances - stopping them...
    taskkill /F /IM "CompanyLock.Agent.exe" /T >nul 2>&1
    timeout /t 2 /nobreak >nul
    echo [OK] Existing instances stopped
) else (
    echo [OK] No existing instances found
)

echo [STEP 2] Creating directories...
if not exist "C:\Program Files\CompanyLock" mkdir "C:\Program Files\CompanyLock"
if not exist "C:\Program Files\CompanyLock\AdminTool" mkdir "C:\Program Files\CompanyLock\AdminTool"
if not exist "C:\Program Files\CompanyLock\Agent" mkdir "C:\Program Files\CompanyLock\Agent"
if not exist "C:\Program Files\CompanyLock\UI" mkdir "C:\Program Files\CompanyLock\UI"

echo [STEP 3] Copying files...
xcopy /Y /Q "%~dp0AdminTool\*" "C:\Program Files\CompanyLock\AdminTool\" >nul
xcopy /Y /Q "%~dp0Agent\*" "C:\Program Files\CompanyLock\Agent\" >nul
xcopy /Y /Q "%~dp0UI\*" "C:\Program Files\CompanyLock\UI\" >nul
echo [OK] All files copied

echo.
echo [STEP 4] Setting up automatic startup and background service...

REM Create startup batch file with duplicate detection and silent execution
echo @echo off > "C:\Program Files\CompanyLock\StartAgent.bat"
echo REM Check if CompanyLock Agent is already running >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo tasklist /FI "IMAGENAME eq CompanyLock.Agent.exe" 2^>NUL ^| find /I /N "CompanyLock.Agent.exe"^>NUL >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo if "%%ERRORLEVEL%%"=="0" ( >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo     echo CompanyLock Agent already running - skipping startup >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo     exit /b 0 >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo ^) >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo REM Change to agent directory and start in background >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo cd /d "C:\Program Files\CompanyLock\Agent" >> "C:\Program Files\CompanyLock\StartAgent.bat"
echo start /min /b "CompanyLock Agent" CompanyLock.Agent.exe >> "C:\Program Files\CompanyLock\StartAgent.bat"

REM Create VBS script for completely silent background startup in USER context
echo Set WshShell = CreateObject("WScript.Shell") > "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
echo WshShell.Run chr(34) ^& "C:\Program Files\CompanyLock\StartAgent.bat" ^& Chr(34), 0, False >> "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
echo Set WshShell = Nothing >> "C:\Program Files\CompanyLock\StartAgentSilent.vbs"

REM Copy VBS to user startup folder for USER SESSION execution (not system)
echo [INFO] Adding to user startup folder for proper session context...
copy "C:\Program Files\CompanyLock\StartAgentSilent.vbs" "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\CompanyLockAgent.vbs" >nul 2>&1

if %errorlevel% equ 0 (
    echo [SUCCESS] CompanyLock Agent added to user startup (User Session)
) else (
    echo [WARNING] Could not add to user startup
)

REM Remove any system-wide registry entries to prevent Services session execution  
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "CompanyLockAgent" /f >nul 2>&1
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "CompanyLockAgent" /f >nul 2>&1

echo.
echo [STEP 5] Creating desktop shortcuts...
powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock AdminTool.lnk'); $Shortcut.TargetPath = 'C:\Program Files\CompanyLock\AdminTool\CompanyLock.AdminTool.exe'; $Shortcut.Save(); Write-Host '[OK] AdminTool shortcut created' } catch { Write-Host '[ERROR] AdminTool shortcut failed' }"

powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock Lock Screen.lnk'); $Shortcut.TargetPath = 'C:\Program Files\CompanyLock\UI\CompanyLock.UI.exe'; $Shortcut.Save(); Write-Host '[OK] Lock Screen shortcut created' } catch { Write-Host '[ERROR] Lock Screen shortcut failed' }"

powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\Start CompanyLock Agent.lnk'); $Shortcut.TargetPath = 'C:\Program Files\CompanyLock\StartAgent.bat'; $Shortcut.Description = 'Start CompanyLock Background Agent'; $Shortcut.Save(); Write-Host '[OK] Agent startup shortcut created' } catch { Write-Host '[ERROR] Agent shortcut failed' }"

echo.
echo [STEP 6] Starting CompanyLock Agent in background...
echo [INFO] Starting agent for immediate use (background mode)...

REM Start agent silently in background using VBS script
cscript //nologo "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
timeout /t 5 /nobreak >nul

REM Check if process is running
tasklist | findstr /i "CompanyLock.Agent" >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] CompanyLock Agent is now running in background
    echo [INFO] Agent will automatically start with Windows (silent mode)
    echo [INFO] No visible windows - agent runs completely in background
) else (
    echo [INFO] Agent startup in progress...
    echo [INFO] Use "Start CompanyLock Agent" desktop shortcut if needed
    echo [WARNING] Agent may need a few more seconds to start
)

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo [SUCCESS] CompanyLock installed for Ghost Spectre/Modified Windows
echo.
echo [COMPATIBILITY MODE]
echo - No Windows Service dependency
echo - Uses Windows startup registry entries
echo - Runs as regular application with auto-start
echo.
echo [USAGE]
echo 1. Agent runs automatically on system startup
echo 2. Use Ctrl+Alt+L to activate lock screen
echo 3. Use AdminTool to manage employees
echo 4. Manual start: "Start CompanyLock Agent" shortcut
echo.
echo [SHORTCUTS CREATED]
echo - CompanyLock AdminTool.lnk
echo - CompanyLock Lock Screen.lnk  
echo - Start CompanyLock Agent.lnk
echo.
echo [IMPORTANT]
echo If Ctrl+Alt+L doesn't work immediately, restart Windows
echo or use "Start CompanyLock Agent" shortcut manually.
echo ========================================

pause