@echo off
echo ======================================
echo Testing CompanyLock Log Management Features
echo ======================================
echo.

echo [INFO] Starting AdminTool for log management testing...
echo [INFO] New features added:
echo.
echo 1. Clear All Logs - Delete all audit logs with confirmation
echo 2. Cleanup Old Logs - Delete logs older than 30 days 
echo 3. Delete by Date Range - Filter and delete logs by date range
echo 4. Delete by Event Type - Delete logs by specific event types
echo 5. Delete Selected Logs - Bulk delete multiple selected logs
echo 6. Database Statistics - View log count and database size
echo 7. Automatic Cleanup - Background service cleans logs older than 30 days daily
echo.

echo [INFO] Log Management UI Features:
echo - Log count and database size display
echo - Date range picker for filtering
echo - Event type filter dropdown
echo - Multi-selection for bulk delete
echo - Confirmation dialogs for all delete operations
echo - Status messages for all operations
echo.

echo [TEST] Starting AdminTool to test log management...
echo [INFO] Navigate to the "Audit Logs" tab to see new features
echo.
pause

cd /d "%~dp0\src\CompanyLock.AdminTool\bin\Release\net8.0-windows"
if exist CompanyLock.AdminTool.exe (
    echo [INFO] Launching AdminTool...
    start CompanyLock.AdminTool.exe
    echo [INFO] AdminTool launched successfully!
    echo.
    echo [TEST GUIDE] In AdminTool Audit Logs tab:
    echo 1. Check log statistics display at top
    echo 2. Try "Export Logs" to backup before testing delete
    echo 3. Test "Cleanup Old Logs" (safe, only removes logs older than 30 days)
    echo 4. Select date range and test "Delete by Date Range"
    echo 5. Select event type and test "Delete by Type"
    echo 6. Select multiple logs and test "Delete Selected Logs"
    echo 7. Use "Clear All Logs" only if you want to remove ALL logs
    echo.
    echo [WARNING] Delete operations cannot be undone! Export logs first!
    echo.
) else (
    echo [ERROR] AdminTool executable not found!
    echo [INFO] Please build the solution first:
    echo        dotnet build CompanyLock.sln --configuration Release
    echo.
    pause
    exit /b 1
)

echo [INFO] Testing automatic log cleanup service...
echo [INFO] The Agent service now includes automatic log cleanup:
echo - Runs every 24 hours
echo - Removes logs older than 30 days
echo - Logs cleanup activities to audit log
echo - Prevents database from growing too large
echo.

echo [INFO] Starting Agent service to test automatic cleanup...
cd /d "%~dp0\src\CompanyLock.Agent\bin\Release\net8.0-windows"
if exist CompanyLock.Agent.exe (
    echo [INFO] Launching Agent service...
    echo [INFO] Look for log cleanup messages in console output
    echo [INFO] Agent will perform cleanup every 24 hours automatically
    echo.
    start CompanyLock.Agent.exe
    echo [INFO] Agent service launched! Check console for cleanup logs.
    echo.
) else (
    echo [ERROR] Agent executable not found!
    echo [INFO] Please build the solution first.
    pause
    exit /b 1
)

echo ======================================
echo Log Management Testing Summary
echo ======================================
echo.
echo [FEATURES ADDED]
echo ✓ AdminTool: Clear All Logs button
echo ✓ AdminTool: Cleanup Old Logs button (30 days)
echo ✓ AdminTool: Delete by Date Range with date pickers
echo ✓ AdminTool: Delete by Event Type with dropdown filter
echo ✓ AdminTool: Delete Selected Logs with multi-selection
echo ✓ AdminTool: Real-time log count and database size display
echo ✓ Agent: Automatic log cleanup service (daily, 30 days retention)
echo ✓ Database: Enhanced cleanup methods in LocalAuthService
echo.
echo [ROLLING WINDOW SYSTEM]
echo The system now maintains a rolling 30-day window of logs:
echo - Today: Oct 18, 2025
echo - Keeps: Sep 18, 2025 - Oct 18, 2025
echo - Removes: Everything before Sep 18, 2025
echo - Tomorrow: Sep 19, 2025 - Oct 19, 2025
echo This prevents database bloat while maintaining recent logs!
echo.
echo [NEXT STEPS]
echo 1. Test all log management features in AdminTool
echo 2. Verify automatic cleanup works in Agent service
echo 3. Create installer package for easy distribution
echo.
pause