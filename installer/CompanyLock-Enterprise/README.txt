# CompanyLock Installation Guide v5

## Installation Options

### RECOMMENDED: Standard Installation
1. Right-click Install.bat -> "Run as administrator"
2. Follow prompts
3. Use desktop shortcuts after installation

### IF SERVICE ISSUES: Debug Installation
1. Right-click Install-Service-Debug.bat -> "Run as administrator"  
2. Shows detailed service installation process
3. Identifies specific problems with service creation

### ALTERNATIVE: Simple Installation
1. Right-click Install-CompanyLock.bat -> "Run as administrator"
2. Professional installer with detailed steps

## Troubleshooting Service Installation

If you see "Service installation failed":
1. Ensure running as Administrator (required)
2. Try Install-Service-Debug.bat for detailed diagnosis
3. Manual service creation: 
   sc create "CompanyLock Agent" binPath="\"C:\Program Files\CompanyLock\Agent\CompanyLock.Agent.exe\"" start=auto

## Default Credentials
Username: admin
Password: admin123

## What Gets Installed
- AdminTool: Employee management interface  
- Agent Service: Background monitoring (Windows service)
- Lock Screen UI: Secure authentication interface
- Desktop shortcuts for easy access

## System Requirements
- Windows 10/11 (64-bit)
- Administrator privileges for installation
- ~300 MB disk space
- No .NET installation required (embedded)
