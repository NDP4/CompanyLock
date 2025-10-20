@echo off
setlocal enabledelayedexpansion

echo ========================================
echo CompanyLock Installation - SC TEST v8
echo ========================================

echo [TEST 1] Basic SC command without quotes in service name
sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
set RESULT1=%errorlevel%
echo [RESULT 1] Error code: %RESULT1%
if %RESULT1% equ 0 (
    echo [SUCCESS] Basic service created - deleting for next test
    sc delete CompanyLockAgent >nul 2>&1
) else (
    echo [FAILED] Basic service creation failed
)

echo.
echo [TEST 2] Using shorter path with environment variable
sc create CompanyLockAgent binPath= "%%ProgramFiles%%\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
set RESULT2=%errorlevel%
echo [RESULT 2] Error code: %RESULT2%
if %RESULT2% equ 0 (
    echo [SUCCESS] Environment variable path worked - deleting for next test
    sc delete CompanyLockAgent >nul 2>&1
) else (
    echo [FAILED] Environment variable path failed
)

echo.
echo [TEST 3] Minimal parameters only
sc create CompanyLockAgent binPath= "C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe"
set RESULT3=%errorlevel%
echo [RESULT 3] Error code: %RESULT3%
if %RESULT3% equ 0 (
    echo [SUCCESS] Minimal parameters worked - deleting for next test
    sc delete CompanyLockAgent >nul 2>&1
) else (
    echo [FAILED] Minimal parameters failed
)

echo.
echo [TEST 4] Using escaped quotes method
sc create CompanyLockAgent binPath= ^"C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe^" start= auto
set RESULT4=%errorlevel%
echo [RESULT 4] Error code: %RESULT4%
if %RESULT4% equ 0 (
    echo [SUCCESS] Escaped quotes worked - deleting for next test
    sc delete CompanyLockAgent >nul 2>&1
) else (
    echo [FAILED] Escaped quotes failed
)

echo.
echo [TEST 5] Using double quotes around entire path
sc create CompanyLockAgent "binPath=C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe" start= auto
set RESULT5=%errorlevel%
echo [RESULT 5] Error code: %RESULT5%
if %RESULT5% equ 0 (
    echo [SUCCESS] Double quotes method worked - deleting for next test
    sc delete CompanyLockAgent >nul 2>&1
) else (
    echo [FAILED] Double quotes method failed
)

echo.
echo ========================================
echo TEST SUMMARY
echo ========================================
echo Test 1 (no quotes in name): %RESULT1%
echo Test 2 (environment var): %RESULT2%
echo Test 3 (minimal params): %RESULT3%
echo Test 4 (escaped quotes): %RESULT4%
echo Test 5 (double quotes): %RESULT5%
echo.
echo Any result of 0 means that method works
echo All other numbers are error codes
echo ========================================

pause