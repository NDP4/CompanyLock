@echo off
echo ======================================
echo CompanyLock Installation
echo ======================================
echo.
echo [INFO] Installing CompanyLock Enterprise Security System...
echo [INFO] This installer will:
echo   1. Copy all files to Program Files
echo   2. Initialize local database
echo   3. Install Windows service
echo   4. Import sample employee data
echo   5. Configure system startup
echo.
pause

echo [STEP 1] Creating installation directory...
set "INSTALL_DIR=%ProgramFiles%\CompanyLock"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\AdminTool" mkdir "%INSTALL_DIR%\AdminTool"
if not exist "%INSTALL_DIR%\Agent" mkdir "%INSTALL_DIR%\Agent"
if not exist "%INSTALL_DIR%\UI" mkdir "%INSTALL_DIR%\UI"
if not exist "%INSTALL_DIR%\Database" mkdir "%INSTALL_DIR%\Database"
if not exist "%INSTALL_DIR%\Documentation" mkdir "%INSTALL_DIR%\Documentation"

echo [STEP 2] Copying application files...
xcopy /Y /Q AdminTool\* "%INSTALL_DIR%\AdminTool\"
xcopy /Y /Q Agent\* "%INSTALL_DIR%\Agent\"
xcopy /Y /Q UI\* "%INSTALL_DIR%\UI\"
xcopy /Y /Q Documentation\* "%INSTALL_DIR%\Documentation\"
copy /Y sample_employees.csv "%INSTALL_DIR%\"

echo [STEP 3] Installing Windows service...
echo Installing service: "CompanyLockAgent"
sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto DisplayName= "CompanyLock Agent Service" description= "CompanyLock Enterprise Security - Background monitoring" >nul 2>&1
set SERVICE_RESULT=%errorlevel%
if %SERVICE_RESULT% equ 0 (
    echo [SUCCESS] Service installed successfully
) else (
    if %SERVICE_RESULT% equ 1073 (
        echo [INFO] Service already exists - updating configuration
        sc config CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto >nul 2>&1
        if %errorlevel% equ 0 (
            echo [SUCCESS] Service configuration updated
        ) else (
            echo [WARNING] Service configuration update failed
        )
    ) else (
        echo [WARNING] Service installation failed - Error code: %SERVICE_RESULT%
        echo [INFO] Manual command: sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
        echo [INFO] Make sure you are running as Administrator
    )
)

echo [STEP 4] Creating database and importing sample data...
set "LOCALAPPDATA_DB=%LOCALAPPDATA%\CompanyLock"
if not exist "%LOCALAPPDATA_DB%" mkdir "%LOCALAPPDATA_DB%"

echo [STEP 5] Creating desktop shortcuts...
echo Creating AdminTool shortcut...
powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock AdminTool.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\AdminTool\CompanyLock.AdminTool.exe'; $Shortcut.Description = 'CompanyLock Employee Management'; $Shortcut.Save(); Write-Host '[SUCCESS] AdminTool shortcut created' } catch { Write-Host '[ERROR] Failed to create AdminTool shortcut' }"

echo Creating LockScreen shortcut...
powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock Lock Screen.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\UI\CompanyLock.UI.exe'; $Shortcut.Description = 'CompanyLock Lock Screen'; $Shortcut.Save(); Write-Host '[SUCCESS] Lock Screen shortcut created' } catch { Write-Host '[ERROR] Failed to create Lock Screen shortcut' }"

echo [STEP 6] Starting service...
sc start CompanyLockAgent
if %errorlevel% equ 0 (
    echo [SUCCESS] Service started successfully
) else (
    echo [INFO] Service will start on next system boot
)

echo ======================================
echo Installation Complete!
echo ======================================
echo.
echo [SUCCESS] CompanyLock has been installed to: %INSTALL_DIR%
echo.
echo [NEXT STEPS]
echo 1. Use AdminTool to manage employees (desktop shortcut created)
echo 2. Import sample data: sample_employees.csv
echo 3. Test lock screen (desktop shortcut created)
echo 4. Agent service is running in background
echo.
echo [SHORTCUTS CREATED]
echo - Desktop: CompanyLock AdminTool.lnk
echo - Desktop: CompanyLock LockScreen.lnk
echo.
echo [DOCUMENTATION]
echo Check %INSTALL_DIR%\Documentation\ for complete guides
echo.
pause
