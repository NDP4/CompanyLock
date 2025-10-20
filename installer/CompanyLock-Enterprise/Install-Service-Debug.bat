@echo off
title CompanyLock Installation - Debug Service Version
echo ========================================
echo CompanyLock Installation - SERVICE DEBUG
echo ========================================
echo.
echo [INFO] This debug version will show detailed service installation process
echo.

:: Get script directory (where Install.bat is located)
set "SCRIPT_DIR=%~dp0"
set "INSTALL_DIR=%ProgramFiles%\CompanyLock"

echo [DEBUG] Script directory: %SCRIPT_DIR%
echo [DEBUG] Target directory: %INSTALL_DIR%
echo [DEBUG] Running as: %USERNAME%
echo.

:: Check administrator privileges
net session >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Running with Administrator privileges
) else (
    echo [ERROR] NOT running as Administrator!
    echo [INFO] Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo.
echo [STEP 1] Creating directories...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\AdminTool" mkdir "%INSTALL_DIR%\AdminTool"
if not exist "%INSTALL_DIR%\Agent" mkdir "%INSTALL_DIR%\Agent"
if not exist "%INSTALL_DIR%\UI" mkdir "%INSTALL_DIR%\UI"
if not exist "%INSTALL_DIR%\Documentation" mkdir "%INSTALL_DIR%\Documentation"

echo [STEP 2] Copying files...
set "SCRIPT_DIR=%~dp0"
if exist "%SCRIPT_DIR%AdminTool\CompanyLock.AdminTool.exe" (
    xcopy /Y /Q "%SCRIPT_DIR%AdminTool\*.*" "%INSTALL_DIR%\AdminTool\"
    echo [OK] AdminTool files copied
) else (
    echo [ERROR] AdminTool files not found
)

if exist "%SCRIPT_DIR%Agent\CompanyLock.Agent.exe" (
    xcopy /Y /Q "%SCRIPT_DIR%Agent\*.*" "%INSTALL_DIR%\Agent\"
    echo [OK] Agent files copied
) else (
    echo [ERROR] Agent files not found
)

if exist "%SCRIPT_DIR%UI\CompanyLock.UI.exe" (
    xcopy /Y /Q "%SCRIPT_DIR%UI\*.*" "%INSTALL_DIR%\UI\"
    echo [OK] UI files copied
) else (
    echo [ERROR] UI files not found
)

echo [STEP 3] SERVICE INSTALLATION DEBUG...
echo.
echo [DEBUG] Target service executable: %INSTALL_DIR%\Agent\CompanyLock.Agent.exe
if exist "%INSTALL_DIR%\Agent\CompanyLock.Agent.exe" (
    echo [OK] Service executable exists
    dir "%INSTALL_DIR%\Agent\CompanyLock.Agent.exe"
) else (
    echo [ERROR] Service executable NOT found
    echo [DEBUG] Directory contents:
    dir "%INSTALL_DIR%\Agent\"
    pause
    exit /b 1
)

echo.
echo [DEBUG] Checking if service already exists...
sc query "CompanyLock Agent" >nul 2>&1
if %errorlevel% equ 0 (
    echo [INFO] Service already exists - will delete and recreate
    echo [DEBUG] Stopping existing service...
    sc stop "CompanyLock Agent" >nul 2>&1
    timeout /t 3 /nobreak >nul
    echo [DEBUG] Deleting existing service...
    sc delete "CompanyLock Agent" >nul 2>&1
    timeout /t 2 /nobreak >nul
) else (
    echo [OK] Service does not exist - creating new
)

echo.
echo [DEBUG] Creating service with command:
echo sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto DisplayName= "CompanyLock Agent Service"

sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto DisplayName= "CompanyLock Agent Service" description= "CompanyLock Enterprise Security - Background monitoring"
set CREATE_RESULT=%errorlevel%

echo [DEBUG] Service creation result: %CREATE_RESULT%
if %CREATE_RESULT% equ 0 (
    echo [SUCCESS] Service created successfully
) else (
    echo [ERROR] Service creation failed
    echo [DEBUG] Possible causes:
    echo - Not running as Administrator
    echo - Executable path has issues
    echo - Service name conflicts
    echo - Windows permissions problem
    pause
    exit /b 1
)

echo.
echo [DEBUG] Starting service...
sc start "CompanyLock Agent"
set START_RESULT=%errorlevel%
echo [DEBUG] Service start result: %START_RESULT%

if %START_RESULT% equ 0 (
    echo [SUCCESS] Service started successfully
) else (
    echo [WARNING] Service start failed - checking status
    sc query "CompanyLock Agent"
)

echo.
echo [STEP 4] Creating shortcuts...
powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock AdminTool.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\AdminTool\CompanyLock.AdminTool.exe'; $Shortcut.Save(); Write-Host '[OK] AdminTool shortcut created' } catch { Write-Host '[ERROR] AdminTool shortcut failed' }"

powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock Lock Screen.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\UI\CompanyLock.UI.exe'; $Shortcut.Save(); Write-Host '[OK] Lock Screen shortcut created' } catch { Write-Host '[ERROR] Lock Screen shortcut failed' }"

echo.
echo ========================================
echo DEBUG INSTALLATION COMPLETE
echo ========================================
echo.
echo [FINAL STATUS CHECK]
echo Service Status:
sc query "CompanyLock Agent"

echo.
echo Files Status:
if exist "%INSTALL_DIR%\AdminTool\CompanyLock.AdminTool.exe" (
    echo [OK] AdminTool installed
) else (
    echo [ERROR] AdminTool missing
)

if exist "%INSTALL_DIR%\Agent\CompanyLock.Agent.exe" (
    echo [OK] Agent installed
) else (
    echo [ERROR] Agent missing
)

if exist "%INSTALL_DIR%\UI\CompanyLock.UI.exe" (
    echo [OK] UI installed
) else (
    echo [ERROR] UI missing
)

echo.
echo [NEXT STEPS]
echo 1. If service is running, test Ctrl+Alt+L
echo 2. Use AdminTool shortcut to manage employees
echo 3. Check Event Viewer if service has issues
echo.
pause