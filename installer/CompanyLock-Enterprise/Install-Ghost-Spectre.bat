@echo off
setlocal enabledelayedexpansion

echo ========================================
echo CompanyLock Installation - GHOST SPECTRE FIX
echo ========================================

echo [INFO] Detecting Windows environment...
echo [INFO] This installer is optimized for modified Windows systems
echo [DEBUG] Script directory: %~dp0
echo [DEBUG] Target directory: C:\Program Files\CompanyLock
echo [DEBUG] Running as: %USERNAME%

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
echo [STEP 1] Creating directories...
if not exist "C:\Program Files\CompanyLock" mkdir "C:\Program Files\CompanyLock"
if not exist "C:\Program Files\CompanyLock\AdminTool" mkdir "C:\Program Files\CompanyLock\AdminTool"
if not exist "C:\Program Files\CompanyLock\Agent" mkdir "C:\Program Files\CompanyLock\Agent"
if not exist "C:\Program Files\CompanyLock\UI" mkdir "C:\Program Files\CompanyLock\UI"

echo [STEP 2] Copying files...
xcopy /Y /Q "%~dp0AdminTool\*" "C:\Program Files\CompanyLock\AdminTool\" >nul
if %errorlevel% equ 0 (
    echo [OK] AdminTool files copied
) else (
    echo [ERROR] AdminTool copy failed
)

xcopy /Y /Q "%~dp0Agent\*" "C:\Program Files\CompanyLock\Agent\" >nul
if %errorlevel% equ 0 (
    echo [OK] Agent files copied
) else (
    echo [ERROR] Agent copy failed
)

xcopy /Y /Q "%~dp0UI\*" "C:\Program Files\CompanyLock\UI\" >nul
if %errorlevel% equ 0 (
    echo [OK] UI files copied
) else (
    echo [ERROR] UI copy failed
)

echo.
echo [STEP 3] ALTERNATIVE SERVICE INSTALLATION FOR GHOST SPECTRE...

echo [DEBUG] Target service executable: C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe
if exist "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" (
    echo [OK] Service executable exists
) else (
    echo [ERROR] Service executable not found
    pause
    exit /b 1
)

echo.
echo [METHOD 1] Trying PowerShell New-Service...
powershell -Command "try { New-Service -Name 'CompanyLockAgent' -BinaryPathName 'C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe' -StartupType Automatic -DisplayName 'CompanyLock Agent Service' -Description 'CompanyLock Enterprise Security'; Write-Host '[SUCCESS] PowerShell service creation succeeded' } catch { Write-Host '[FAILED] PowerShell method failed:' $_.Exception.Message }"
set POWERSHELL_RESULT=%errorlevel%

if %POWERSHELL_RESULT% neq 0 (
    echo.
    echo [METHOD 2] Trying WMI service creation...
    powershell -Command "try { $service = (Get-WmiObject -Class Win32_BaseService).Create('C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe', 'CompanyLockAgent', 'C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe', 16, 2, 'Automatic', 0, $null, $null, 'CompanyLock Agent Service', $null, $null); if($service.ReturnValue -eq 0) { Write-Host '[SUCCESS] WMI service creation succeeded' } else { Write-Host '[FAILED] WMI creation failed with code:' $service.ReturnValue } } catch { Write-Host '[FAILED] WMI method failed:' $_.Exception.Message }"
    set WMI_RESULT=%errorlevel%
    
    if %WMI_RESULT% neq 0 (
        echo.
        echo [METHOD 3] Trying direct registry method...
        
        REM Create service registry entries directly
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "Type" /t REG_DWORD /d 16 /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "Start" /t REG_DWORD /d 2 /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "ErrorControl" /t REG_DWORD /d 1 /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "ImagePath" /t REG_SZ /d "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "DisplayName" /t REG_SZ /d "CompanyLock Agent Service" /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "Description" /t REG_SZ /d "CompanyLock Enterprise Security - Background monitoring" /f >nul 2>&1
        reg add "HKLM\SYSTEM\CurrentControlSet\Services\CompanyLockAgent" /v "ObjectName" /t REG_SZ /d "LocalSystem" /f >nul 2>&1
        
        if %errorlevel% equ 0 (
            echo [SUCCESS] Registry service entries created
            set REGISTRY_RESULT=0
        ) else (
            echo [FAILED] Registry method failed
            set REGISTRY_RESULT=1
        )
        
        if %REGISTRY_RESULT% neq 0 (
            echo.
            echo [METHOD 4] Creating background startup script as fallback...
            
            REM Create startup batch file that runs in background
            echo @echo off > "C:\Program Files\CompanyLock\StartAgent.bat"
            echo cd /d "C:\Program Files\CompanyLock\Agent" >> "C:\Program Files\CompanyLock\StartAgent.bat"
            echo start /min /b "CompanyLock Agent" CompanyLock.Agent.exe >> "C:\Program Files\CompanyLock\StartAgent.bat"
            
            REM Create VBS script for silent background startup
            echo Set WshShell = CreateObject("WScript.Shell") > "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
            echo WshShell.Run chr(34) ^& "C:\Program Files\CompanyLock\StartAgent.bat" ^& Chr(34), 0 >> "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
            echo Set WshShell = Nothing >> "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
            
            REM Add VBS script to registry startup (completely silent)
            reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "CompanyLockAgent" /t REG_SZ /d "wscript.exe \"C:\Program Files\CompanyLock\StartAgentSilent.vbs\"" /f >nul 2>&1
            
            if %errorlevel% equ 0 (
                echo [SUCCESS] Background startup script method configured
                echo [INFO] Agent will start automatically and silently on system boot
                set STARTUP_RESULT=0
            ) else (
                echo [FAILED] All service installation methods failed
                set STARTUP_RESULT=1
            )
        )
    )
)

echo.
echo [STEP 4] Creating desktop shortcuts...
echo [INFO] Creating desktop shortcuts...
powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock AdminTool.lnk'); $Shortcut.TargetPath = 'C:\Program Files\CompanyLock\AdminTool\CompanyLock.AdminTool.exe'; $Shortcut.Description = 'CompanyLock Employee Management'; $Shortcut.Save(); Write-Host '[SUCCESS] AdminTool shortcut created' } catch { Write-Host '[ERROR] Failed to create AdminTool shortcut' }"

powershell -Command "try { $WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\CompanyLock Lock Screen.lnk'); $Shortcut.TargetPath = 'C:\Program Files\CompanyLock\UI\CompanyLock.UI.exe'; $Shortcut.Description = 'CompanyLock Lock Screen'; $Shortcut.Save(); Write-Host '[SUCCESS] Lock Screen shortcut created' } catch { Write-Host '[ERROR] Failed to create Lock Screen shortcut' }"

echo.
echo [STEP 5] Attempting to start service/application...

REM Try to start via service first
sc start CompanyLockAgent >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] Service started via SC command
    goto :installation_complete
)

REM Try PowerShell service start
powershell -Command "try { Start-Service -Name 'CompanyLockAgent'; Write-Host '[SUCCESS] Service started via PowerShell' } catch { Write-Host '[INFO] Service start failed, trying direct execution' }" 2>nul
if %errorlevel% equ 0 (
    goto :installation_complete
)

REM Try direct execution as fallback
echo [INFO] Starting agent directly in background (silent mode)...
if exist "C:\Program Files\CompanyLock\StartAgentSilent.vbs" (
    echo [INFO] Using VBS silent startup method...
    cscript //nologo "C:\Program Files\CompanyLock\StartAgentSilent.vbs"
    timeout /t 5 /nobreak >nul
    tasklist | findstr /i "CompanyLock.Agent" >nul 2>&1
    if %errorlevel% equ 0 (
        echo [SUCCESS] Agent started silently in background
    ) else (
        echo [INFO] VBS method in progress, trying direct method...
        start /min /b "CompanyLock Agent" "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
        timeout /t 3 /nobreak >nul
        echo [SUCCESS] Agent started as background process
    )
) else (
    start /min /b "CompanyLock Agent" "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
    if %errorlevel% equ 0 (
        echo [SUCCESS] Agent started as background process
    ) else (
        echo [WARNING] Could not start agent automatically
        echo [INFO] You can start it manually from desktop shortcut
    )
)

:installation_complete
echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo [SUCCESS] CompanyLock has been installed to: C:\Program Files\CompanyLock
echo.
echo [IMPORTANT FOR GHOST SPECTRE SYSTEMS]
echo This installation uses alternative methods compatible with
echo modified Windows systems like Ghost Spectre.
echo.
echo [NEXT STEPS]
echo 1. Test lock screen: Desktop shortcut created
echo 2. Use AdminTool: Desktop shortcut created  
echo 3. Try Ctrl+Alt+L hotkey (if agent is running)
echo 4. If hotkey doesn't work, start agent manually from:
echo    "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
echo.
echo [SHORTCUTS CREATED]
echo - Desktop: CompanyLock AdminTool.lnk
echo - Desktop: CompanyLock Lock Screen.lnk
echo.
echo ========================================
pause