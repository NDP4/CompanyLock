@echo off
echo ========================================
echo      CompanyLock Testing Script
echo ========================================
echo.

echo [1] Starting CompanyLock Agent...
start "CompanyLock Agent" /D "src\CompanyLock.Agent\bin\Debug\net8.0-windows" CompanyLock.Agent.exe
timeout /t 3 >nul

echo [2] Starting Admin Tool...
start "CompanyLock Admin" /D "src\CompanyLock.AdminTool\bin\Debug\net8.0-windows" CompanyLock.AdminTool.exe
timeout /t 2 >nul

echo [3] Starting Lock Screen for Testing...
start "CompanyLock UI" /D "src\CompanyLock.UI\bin\Debug\net8.0-windows" CompanyLock.UI.exe
timeout /t 2 >nul

echo.
echo ========================================
echo         Testing Instructions:
echo ========================================
echo 1. Use Admin Tool to import sample_employees.csv
echo 2. Test Lock Screen with these credentials:
echo    - Username: admin      Password: admin123
echo    - Username: john.doe   Password: password123
echo    - Username: jane.smith Password: password123
echo.
echo 3. Press Ctrl+Alt+L (if Agent is monitoring)
echo 4. Press Ctrl+C in Agent window to stop
echo ========================================
echo.
pause