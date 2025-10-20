@echo off
setlocal enabledelayedexpansion

echo ========================================
echo CompanyLock Installation - SIMPLE TEST
echo ========================================

echo [INFO] Testing simplified service installation...
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
echo [STEP 3] SIMPLIFIED SERVICE INSTALLATION...

echo [DEBUG] Target service executable: C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe
if exist "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" (
    echo [OK] Service executable exists
    dir "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
) else (
    echo [ERROR] Service executable not found
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
echo [DEBUG] METHOD 1: Trying with quoted service name
echo [COMMAND] sc create "CompanyLock Agent" binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
sc create "CompanyLock Agent" binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
set METHOD1_RESULT=%errorlevel%
echo [RESULT] Method 1 result: %METHOD1_RESULT%

if %METHOD1_RESULT% neq 0 (
    echo.
    echo [DEBUG] METHOD 2: Trying without quotes in service name
    echo [COMMAND] sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
    sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
    set METHOD2_RESULT=%errorlevel%
    echo [RESULT] Method 2 result: %METHOD2_RESULT%
    
    if %METHOD2_RESULT% neq 0 (
        echo.
        echo [DEBUG] METHOD 3: Trying with environment variable
        echo [COMMAND] sc create CompanyLockAgent binPath= "%%ProgramFiles%%\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
        sc create CompanyLockAgent binPath= "%%ProgramFiles%%\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
        set METHOD3_RESULT=%errorlevel%
        echo [RESULT] Method 3 result: %METHOD3_RESULT%
        
        if %METHOD3_RESULT% neq 0 (
            echo.
            echo [DEBUG] METHOD 4: Trying minimal command
            echo [COMMAND] sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
            sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
            set METHOD4_RESULT=%errorlevel%
            echo [RESULT] Method 4 result: %METHOD4_RESULT%
        )
    )
)

echo.
echo ========================================
echo SERVICE CREATION SUMMARY
echo ========================================
echo Method 1 (quoted name): %METHOD1_RESULT%
if defined METHOD2_RESULT echo Method 2 (no quotes): %METHOD2_RESULT%
if defined METHOD3_RESULT echo Method 3 (env var): %METHOD3_RESULT%
if defined METHOD4_RESULT echo Method 4 (minimal): %METHOD4_RESULT%
echo.

REM Check which method worked
if %METHOD1_RESULT% equ 0 (
    echo [SUCCESS] Service created with quoted name method
    set SERVICE_NAME="CompanyLock Agent"
) else if defined METHOD2_RESULT if %METHOD2_RESULT% equ 0 (
    echo [SUCCESS] Service created with no quotes method
    set SERVICE_NAME=CompanyLockAgent
) else if defined METHOD3_RESULT if %METHOD3_RESULT% equ 0 (
    echo [SUCCESS] Service created with environment variable method
    set SERVICE_NAME=CompanyLockAgent
) else if defined METHOD4_RESULT if %METHOD4_RESULT% equ 0 (
    echo [SUCCESS] Service created with minimal method
    set SERVICE_NAME=CompanyLockAgent
) else (
    echo [ERROR] All service creation methods failed
    echo [INFO] Manual troubleshooting required
    goto :end
)

echo.
echo [STEP 4] Testing service...
echo [DEBUG] Querying created service...
sc query %SERVICE_NAME%
echo [DEBUG] Service query result: %errorlevel%

echo.
echo [STEP 5] Attempting to start service...
sc start %SERVICE_NAME%
set START_RESULT=%errorlevel%
echo [DEBUG] Service start result: %START_RESULT%

if %START_RESULT% equ 0 (
    echo [SUCCESS] Service started successfully
) else (
    echo [INFO] Service created but start failed - this may be normal
    echo [INFO] Service will start automatically on system boot
)

:end
echo.
echo ========================================
echo INSTALLATION COMPLETE
echo ========================================
pause