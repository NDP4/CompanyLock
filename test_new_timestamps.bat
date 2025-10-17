@echo off
echo ========================================================
echo     CompanyLock - Test New Timestamp System
echo ========================================================
echo.
echo This will test the new timestamp system with local time.
echo.
echo CURRENT TIME:
echo Local Time: %date% %time%
echo.
echo TESTING STEPS:
echo 1. Start CompanyLock Agent (background service)
echo 2. Start Lock Screen UI for authentication test
echo 3. Check AdminTool for correct timestamp display
echo.
pause

echo.
echo [1/3] Starting CompanyLock Agent...
start /D "src\CompanyLock.Agent\bin\Release\net8.0-windows" CompanyLock.Agent.exe
timeout /t 3 /nobreak >nul

echo.
echo [2/3] Starting Lock Screen UI for authentication test...
start /D "src\CompanyLock.UI\bin\Release\net8.0-windows" CompanyLock.UI.exe
timeout /t 2 /nobreak >nul

echo.
echo [3/3] Opening AdminTool to check timestamp display...
start /D "src\CompanyLock.AdminTool\bin\Release\net8.0-windows" CompanyLock.AdminTool.exe

echo.
echo ================================================================
echo TEST INSTRUCTIONS:
echo.
echo 1. In Lock Screen UI:
echo    - Try to login with any employee (test01, password: password123)
echo    - This will create authentication audit logs
echo.
echo 2. In AdminTool:
echo    - Go to Audit Logs tab
echo    - Check if timestamps show LOCAL TIME (not UTC)
echo    - Format should be: DD/MM/YYYY HH:MM:SS
echo.
echo Expected Result: 
echo - Timestamps should match current local time
echo - No more timezone mismatch issues
echo ================================================================
echo.
pause