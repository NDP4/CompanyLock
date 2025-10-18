# 🔒 CompanyLock - Sistem Keamanan Perusahaan Offline

**CompanyLock** adalah solusi keamanan workstation berbasis offline yang dirancang khusus untuk lingkungan perusahaan. Sistem ini menyediakan autentikasi lokal, monitoring aktivitas, dan kontrol akses yang komprehensif tanpa memerlukan koneksi internet.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## 📋 Daftar Isi

- [Fitur Utama](#-fitur-utama)
- [Arsitektur Sistem](#-arsitektur-sistem)
- [Persyaratan Sistem](#-persyaratan-sistem)
- [Instalasi & Setup](#-instalasi--setup)
- [Penggunaan](#-penggunaan)
- [Pengembangan](#-pengembangan)

## ✨ Fitur Utama

### 🔐 Keamanan & Autentikasi

- **Autentikasi Offline**: Login lokal tanpa memerlukan koneksi internet
- **Password Hashing**: Menggunakan Argon2 untuk keamanan password yang tinggi
- **Enkripsi Data**: Windows DPAPI untuk proteksi data sensitif
- **Kontrol Akses**: Sistem role-based dengan level User dan Admin
- **Enhanced Lock Security**: Alt+F4, Alt+Tab, Windows key blocking
- **Bypass Prevention**: Multi-layer security mencegah task switching dan window manipulation

### 🖥️ Monitoring & Kontrol

- **Session Monitoring**: Memantau sesi login/logout pengguna
- **Idle Detection**: Deteksi aktivitas idle dengan timeout otomatis (default: 1 menit)
- **Global Hotkey**: Shortcut Ctrl+Alt+L untuk lock screen cepat
- **Lock Prevention**: Mencegah multiple lock screen instances

### 📊 Manajemen Data

- **Database Offline**: SQLite untuk penyimpanan data lokal
- **Employee Management**: CRUD operations untuk data karyawan via AdminTool
- **Audit Logging**: Pencatatan aktivitas sistem yang lengkap dengan timestamp
- **CSV Import/Export**: Import data karyawan dari file `sample_employees.csv`
- **Log Management**: Clear, bulk delete, filter by date/type, automatic cleanup
- **Rolling Retention**: Automatic 30-day log retention untuk mencegah database bloat

### 🎨 User Interface

- **Modern UI**: Glassmorphism design untuk lock screen yang elegan
- **Admin Dashboard**: WPF-based admin tools untuk manajemen sistem
- **Real-time Updates**: Live timestamp dan status monitoring
- **Keyboard Navigation**: Full keyboard support untuk accessibility

## 🏗️ Arsitektur Sistem

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  CompanyLock.UI │    │CompanyLock.Agent│    │CompanyLock.Admin│
│   (Lock Screen) │◄──►│   (Service)     │◄──►│     (Tools)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         └────────────────────────┼────────────────────────┘
                                  │
                         ┌─────────────────┐
                         │CompanyLock.Core │
                         │  (Shared Logic) │
                         └─────────────────┘
                                  │
                         ┌─────────────────┐
                         │CompanyLock.Auth │
                         │   (Database)    │
                         └─────────────────┘
```

### Komponen Sistem

- **CompanyLock.Agent**: Windows Service untuk background monitoring dan hotkey detection
- **CompanyLock.UI**: WPF Lock screen dengan glassmorphism design dan real-time clock
- **CompanyLock.Core**: Shared library untuk models, utilities, dan logging
- **CompanyLock.LocalAuth**: Authentication module dengan SQLite dan Argon2 hashing
- **CompanyLock.AdminTool**: Admin dashboard untuk employee management dan audit logs
- **CompanyLock.Installer**: WiX installer untuk deployment (optional)

### Technology Stack

- **.NET 8 LTS** - Primary framework
- **SQLite** - Local database storage (`%LOCALAPPDATA%\CompanyLock\local.db`)
- **Argon2** - Password hashing untuk keamanan tinggi
- **Windows DPAPI** - Secure secret storage
- **WPF** - Modern user interface dengan glassmorphism
- **Named Pipes** - Inter-process communication (`CompanyLockPipe`)
- **Serilog** - Structured logging framework

## 💻 Persyaratan Sistem

### Minimum Requirements

- **OS**: Windows 10 (64-bit) atau Windows 11
- **Framework**: .NET 8.0 Runtime
- **Memory**: 2 GB RAM
- **Storage**: 100 MB ruang kosong
- **Privileges**: User privileges (untuk aplikasi), Admin (untuk service)

### Recommended Requirements

- **OS**: Windows 11 (64-bit)
- **Framework**: .NET 8.0 SDK (untuk development)
- **Memory**: 4 GB RAM atau lebih
- **Storage**: 500 MB ruang kosong

## 🚀 Instalasi & Setup

### 1. Clone Repository

```bash
# Clone repository dari GitHub
git clone https://github.com/yourusername/CompanyLock.git
cd CompanyLock
```

### 2. Build Project

```bash
# Build seluruh solution
dotnet build CompanyLock.sln --configuration Release

```

### 3. Setup Database & Testing

```bash
# Database akan dibuat otomatis di: %LOCALAPPDATA%\CompanyLock\local.db

# Import sample data untuk testing
# File sample_employees.csv sudah tersedia dengan data test
```

### 4. Install Service (Opsional)

```bash
# Jalankan sebagai Administrator untuk service
sc create "CompanyLock Agent" binPath="C:\Path\To\CompanyLock.Agent.exe"
sc start "CompanyLock Agent"
```

## 📖 Penggunaan

### 🔧 AdminTool - Manajemen Karyawan

```bash
# Jalankan admin dashboard
cd src/CompanyLock.AdminTool/bin/Release/net8.0-windows
./CompanyLock.AdminTool.exe
```

**Fitur AdminTool:**

- 👥 **Employee Management**: Tambah, edit, hapus data karyawan
- 📥 **CSV Import**: Import bulk data dari `sample_employees.csv`
- 📤 **Export Data**: Export data karyawan ke CSV format
- 📋 **Audit Logs**: Review aktivitas sistem dan login attempts
- ⚙️ **Configuration**: Pengaturan sistem dan timeout

### 🔒 Lock Screen - Interface Utama

```bash
# Jalankan lock screen
cd src/CompanyLock.UI/bin/Release/net8.0-windows
./CompanyLock.UI.exe
```

**Cara Penggunaan:**

1. Masukkan **Username** dan **Password**
2. Tekan **Enter** atau klik **Unlock**
3. Gunakan **Tab** untuk navigasi keyboard
4. **Ctrl+Alt+L** untuk quick lock (jika Agent aktif)

### 🛡️ Agent Service - Background Monitoring

```bash
# Jalankan background service
cd src/CompanyLock.Agent/bin/Release/net8.0-windows
./CompanyLock.Agent.exe
```

**Background Services:**

- 🔥 **Hotkey Detection**: Global Ctrl+Alt+L monitoring
- ⏰ **Idle Monitoring**: Auto-lock setelah 1 menit idle
- 📡 **IPC Communication**: Named Pipe `CompanyLockPipe`
- 📊 **Audit Logging**: Complete activity tracking dengan timestamps

### 📄 Sample Data Format

File `sample_employees.csv` berisi contoh data:

```csv
Username,FullName,Department,Role,Password,IsActive
test01,Test User One,IT,User,password123,true
admin,Administrator,IT,Admin,admin123,true
```

## 🛠️ Pengembangan

### Prerequisites untuk Development

- **Visual Studio 2022** atau **VS Code** dengan C# extensions
- **.NET 8.0 SDK**
- **Git** for version control
- **SQLite Browser** untuk database inspection (opsional)

### Project Structure

```
CompanyLock-offline/
├── src/
│   ├── CompanyLock.Core/           # Shared libraries & models
│   ├── CompanyLock.LocalAuth/      # Database & authentication
│   ├── CompanyLock.Agent/          # Background Windows service
│   ├── CompanyLock.UI/             # Lock screen WPF interface
│   ├── CompanyLock.AdminTool/      # Admin management dashboard
│   └── CompanyLock.Installer/      # WiX installer (opsional)
├── sample_employees.csv            # Sample employee data
├── test_*.bat                      # Testing & demo scripts
├── uninstall_service.bat          # Service removal script
├── UI_DESIGN_DOCUMENTATION.md     # UI design guidelines
├── CompanyLock.sln                 # Visual Studio solution
├── .gitignore                      # Git exclusion rules
└── README.md                       # Documentation (this file)
```

### Development Commands

```bash
# Run individual components
dotnet run --project src/CompanyLock.AdminTool    # Admin dashboard
dotnet run --project src/CompanyLock.UI           # Lock screen
dotnet run --project src/CompanyLock.Agent        # Background service

# Build & testing
dotnet build CompanyLock.sln --configuration Release
dotnet test                                        # Run unit tests (jika ada)
```

### Configuration & Database

- **Database Location**: `%LOCALAPPDATA%\CompanyLock\local.db`
- **Configuration**: Embedded dalam applications (tidak memerlukan config file)
- **Logging**: Console output dan Windows Event Log
- **Named Pipe**: `CompanyLockPipe` untuk IPC communication

### Testing Scripts

Tersedia berbagai script untuk testing:

- `test_companylock.bat` - Basic functionality test
- `test_new_timestamps.bat` - Timestamp & audit logging test
- `test_log_management.bat` - **NEW!** Log management features testing
- `test_security_features.bat` - **NEW!** Enhanced security bypass prevention testing
- `uninstall_service.bat` - Service cleanup script

### Log Management Features

**NEW!** Comprehensive log management untuk mencegah database bloat:

- **AdminTool**: Clear all, cleanup old, delete by date/type, bulk delete
- **Agent Service**: Automatic 30-day rolling retention
- **Statistics**: Real-time log count dan database size monitoring
- **Safety**: Confirmation dialogs dan export backup capability

Lihat [`LOG_MANAGEMENT_FEATURES.md`](LOG_MANAGEMENT_FEATURES.md) untuk dokumentasi lengkap.

### Enhanced Security Features

**NEW!** Advanced bypass prevention untuk lock screen security:

- **Keyboard Hooks**: Global Alt+F4, Alt+Tab, Windows key blocking
- **Window Security**: Taskbar hiding, topmost enforcement, manipulation prevention
- **Multi-layer Protection**: Hook level + window level + continuous monitoring
- **Real-time Logging**: Semua security events ter-log untuk audit

Lihat [`ENHANCED_SECURITY_DOCUMENTATION.md`](ENHANCED_SECURITY_DOCUMENTATION.md) untuk detail lengkap.

### Security Implementation

- **Password Hashing**: Argon2id dengan salt untuk setiap user
- **Data Encryption**: Windows DPAPI untuk sensitive information
- **Database Protection**: File permissions dan SQLite WAL mode
- **Input Validation**: Complete sanitization untuk semua user inputs
- **Audit Trail**: Comprehensive logging untuk compliance

---

## 🤝 Setup Git Repository

Untuk memulai development atau berkontribusi:

```bash
# Initialize Git repository (jika belum ada)
git init

# Add remote origin (ganti dengan URL repository Anda)
git remote add origin https://github.com/yourusername/CompanyLock.git

# Add semua files (kecuali yang ada di .gitignore)
git add .

# Commit initial implementation
git commit -m "feat: complete CompanyLock offline system implementation

- Add complete .NET 8 enterprise security system
- Implement glassmorphism UI design for lock screen
- Add SQLite offline database with Argon2 password hashing
- Create Windows service for background monitoring
- Add AdminTool for employee management with CSV import/export
- Implement audit logging with local timestamps
- Add hotkey support (Ctrl+Alt+L) and idle detection
- Include comprehensive .gitignore for clean repository"

# Push to GitHub
git push -u origin main
```

## 📚 Dokumentasi Tambahan

- **[UI_DESIGN_DOCUMENTATION.md](UI_DESIGN_DOCUMENTATION.md)** - Complete UI design guidelines dengan glassmorphism implementation
- **Database Schema** - SQLite tables: Employees, Sessions, AuditEvents dengan proper indexing
- **API Reference** - Named Pipe IPC protocol dan service communication patterns

---

## 🔒 Security & Compliance

### Security Features

- ✅ **Offline Operation** - No network dependency, full air-gapped security
- ✅ **Modern Encryption** - Argon2id password hashing + Windows DPAPI
- ✅ **Audit Compliance** - Complete activity logging dengan timestamps
- ✅ **Session Security** - Secure session management dengan timeout controls
- ✅ **Input Validation** - Comprehensive sanitization dan validation
- ✅ **Service Isolation** - Proper Windows service security dengan minimal privileges

### Enterprise Ready

- **Scalability** - SQLite mendukung ribuan users untuk mid-size enterprise
- **Reliability** - .NET 8 LTS dengan robust error handling
- **Maintainability** - Clean architecture dengan separation of concerns
- **Compliance** - Audit logging untuk ISO 27001 dan security compliance

---

**CompanyLock** - Enterprise-Grade Offline Workstation Security Solution 🚀

_Dibangun dengan .NET 8 LTS, SQLite Database, dan Windows Service Architecture untuk reliability dan security maksimal._
├── src/
│ ├── CompanyLock.Core/ # Shared libraries
│ ├── CompanyLock.LocalAuth/ # Authentication & database
│ ├── CompanyLock.Agent/ # Windows Service
│ ├── CompanyLock.UI/ # Lock screen interface
│ ├── CompanyLock.AdminTool/ # Management application
│ └── CompanyLock.Installer/ # WiX installer
├── sample_employees.csv # Sample employee data
├── install_service.bat # Service installer
└── uninstall_service.bat # Service uninstaller

```

## Troubleshooting

### Common Issues

1. **Service won't start**

   - Check Windows Event Log
   - Verify file permissions in `C:\ProgramData\CompanyLock`
   - Run as Administrator

2. **Can't unlock screen**

   - Verify user credentials in Admin Tool
   - Check if user account is active
   - Use emergency admin account

3. **Lock screen doesn't appear**
   - Check if service is running
   - Verify UI executable exists
   - Check Windows session permissions

### Log Files

- Service logs: `C:\ProgramData\CompanyLock\Logs\`
- Windows Event Log: Application and System logs
- Error details in log files with timestamps

## Offline Architecture Benefits

✅ **No Network Dependency**: Works without internet/network connectivity
✅ **Enhanced Security**: No external attack surface
✅ **Compliance**: Data remains within organizational boundaries
✅ **Performance**: No latency from network calls
✅ **Reliability**: Not affected by network outages

## Limitations

❌ **Centralized Management**: No remote administration
❌ **User Sync**: Manual employee data updates required
❌ **Audit Aggregation**: Logs stored locally per machine
❌ **Password Changes**: Must be updated on each machine

## License

Enterprise Internal Use Only

## Support

For technical support, contact your IT administrator or system integrator.
```
