@echo off
echo ========================================
echo CompanyLock - Rebuild All Distributions
echo ========================================
echo [INFO] Building all components with custom icon
echo.

REM Build all components with icon support
echo [STEP 1] Building CompanyLock Agent...
dotnet publish src/CompanyLock.Agent/CompanyLock.Agent.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "installer/CompanyLock-GhostSpectre/Agent"

echo.
echo [STEP 2] Building CompanyLock AdminTool...
dotnet publish src/CompanyLock.AdminTool/CompanyLock.AdminTool.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "installer/CompanyLock-GhostSpectre/AdminTool"

echo.
echo [STEP 3] Building CompanyLock UI...
dotnet publish src/CompanyLock.UI/CompanyLock.UI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "installer/CompanyLock-GhostSpectre/UI"

echo.
echo [STEP 4] Updating all distribution folders...

REM Update CompanyLock-Distribution
echo [INFO] Updating CompanyLock-Distribution...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Distribution\Agent\" >nul
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Distribution\AdminTool\" >nul
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Distribution\UI\" >nul

REM Update CompanyLock-Simple
echo [INFO] Updating CompanyLock-Simple...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Simple\Agent\" >nul
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Simple\AdminTool\" >nul
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Simple\UI\" >nul

REM Update CompanyLock-Simple-v6
echo [INFO] Updating CompanyLock-Simple-v6...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Simple-v6\Agent\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Simple-v6\AdminTool\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Simple-v6\UI\" >nul 2>&1

REM Update CompanyLock-Simple-v7
echo [INFO] Updating CompanyLock-Simple-v7...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Simple-v7\Agent\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Simple-v7\AdminTool\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Simple-v7\UI\" >nul 2>&1

REM Update CompanyLock-Simple-v8
echo [INFO] Updating CompanyLock-Simple-v8...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Simple-v8\Agent\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Simple-v8\AdminTool\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Simple-v8\UI\" >nul 2>&1

REM Update CompanyLock-Release
echo [INFO] Updating CompanyLock-Release...
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\Agent\*" "installer\CompanyLock-Release\Agent\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\AdminTool\*" "installer\CompanyLock-Release\AdminTool\" >nul 2>&1
xcopy /Y /Q "installer\CompanyLock-GhostSpectre\UI\*" "installer\CompanyLock-Release\UI\" >nul 2>&1

echo.
echo [SUCCESS] All distributions updated with custom icon support!
echo.
echo ========================================
echo Distribution Update Complete
echo ========================================
echo.
echo [ICON STATUS]
if exist "assets\icons\CompanyLock.ico" (
    echo - Custom icon: FOUND ✓
    echo - All apps will use custom icon
) else (
    echo - Custom icon: NOT FOUND ⚠️
    echo - Apps will use default Windows icon
    echo - Create CompanyLock.ico in assets\icons\ folder
)
echo.
echo [NEXT STEPS]
echo 1. Create your custom CompanyLock.ico file
echo 2. Place it in assets\icons\CompanyLock.ico
echo 3. Run this script again to rebuild with icon
echo 4. Test installer on target PC
echo.
pause