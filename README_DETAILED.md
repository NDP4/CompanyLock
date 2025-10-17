# 🔒 CompanyLock - Sistem Keamanan Perusahaan Offline

**CompanyLock** adalah solusi keamanan workstation berbasis offline yang dirancang khusus untuk lingkungan perusahaan. Sistem ini menyediakan autentikasi lokal, monitoring aktivitas, dan kontrol akses yang komprehensif tanpa memerlukan koneksi internet.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## 📋 Daftar Isi

- [Fitur Utama](#-fitur-utama)
- [Arsitektur Sistem](#-arsitektur-sistem)
- [Persyaratan Sistem](#-persyaratan-sistem)
- [Instalasi](#-instalasi)
- [Penggunaan](#-penggunaan)
- [Konfigurasi](#-konfigurasi)
- [Pengembangan](#-pengembangan)
- [Dokumentasi](#-dokumentasi)
- [Kontribusi](#-kontribusi)
- [Lisensi](#-lisensi)

## ✨ Fitur Utama

### 🔐 Keamanan & Autentikasi

- **Autentikasi Offline**: Login lokal tanpa memerlukan koneksi internet
- **Password Hashing**: Menggunakan Argon2 untuk keamanan password yang tinggi
- **Enkripsi Data**: Windows DPAPI untuk proteksi data sensitif
- **Kontrol Akses**: Sistem role-based dengan level User dan Admin

### 🖥️ Monitoring & Kontrol

- **Session Monitoring**: Memantau sesi login/logout pengguna
- **Idle Detection**: Deteksi aktivitas idle dengan timeout otomatis
- **Global Hotkey**: Shortcut Ctrl+Alt+L untuk lock screen cepat
- **Lock Prevention**: Mencegah multiple lock screen instances

### 📊 Manajemen Data

- **Database Offline**: SQLite untuk penyimpanan data lokal
- **Employee Management**: CRUD operations untuk data karyawan
- **Audit Logging**: Pencatatan aktivitas sistem yang lengkap
- **CSV Import/Export**: Import data karyawan dari file CSV

### 🎨 User Interface

- **Modern UI**: Glassmorphism design untuk lock screen
- **Admin Dashboard**: WPF-based admin tools untuk manajemen
- **Real-time Updates**: Live timestamp dan status monitoring
- **Responsive Design**: Optimal untuk berbagai resolusi layar

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

#### 🔧 **CompanyLock.Core**

- Model data dan konfigurasi sistem
- Logging factory dan utilities
- Security helpers (DPAPI, password hashing)

#### 🛡️ **CompanyLock.Agent**

- Windows Service untuk background monitoring
- Hotkey detection (Ctrl+Alt+L)
- Idle timeout monitoring
- Session state management
- Named Pipe server untuk komunikasi

#### 🔒 **CompanyLock.UI**

- Lock screen dengan glassmorphism design
- Autentikasi visual yang modern
- Real-time clock dan status display
- Keyboard navigation support

#### 👥 **CompanyLock.LocalAuth**

- SQLite database management
- Autentikasi dan session handling
- Audit logging system
- Employee data operations

#### ⚙️ **CompanyLock.AdminTool**

- WPF admin dashboard
- Employee management (CRUD)
- CSV import/export functionality
- Audit log viewer
- System configuration

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
- **Processor**: Multi-core processor

## 🚀 Instalasi

### 1. Clone Repository

```bash
git clone https://github.com/yourusername/CompanyLock.git
cd CompanyLock
```

### 2. Build Project

```bash
# Build seluruh solution
dotnet build CompanyLock.sln --configuration Release

# Atau jalankan build task dari VS Code
# Task: "Build CompanyLock Solution"
```

### 3. Setup Database

```bash
# Database akan dibuat otomatis di:
# %LOCALAPPDATA%\CompanyLock\local.db
```

### 4. Install Service (Opsional)

```bash
# Jalankan sebagai Administrator
sc create "CompanyLock Agent" binPath="C:\Path\To\CompanyLock.Agent.exe"
sc start "CompanyLock Agent"
```

## 📖 Penggunaan

### Menjalankan AdminTool

```bash
cd src/CompanyLock.AdminTool/bin/Release/net8.0-windows
./CompanyLock.AdminTool.exe
```

**Fitur AdminTool:**

- 👥 **Manajemen Karyawan**: Tambah, edit, hapus data karyawan
- 📥 **Import CSV**: Import bulk data dari file `sample_employees.csv`
- 📤 **Export Data**: Export data karyawan ke CSV
- 📋 **Audit Logs**: Review aktivitas sistem dan user
- ⚙️ **Konfigurasi**: Pengaturan sistem dan keamanan

### Menjalankan Lock Screen

```bash
cd src/CompanyLock.UI/bin/Release/net8.0-windows
./CompanyLock.UI.exe
```

**Cara Penggunaan:**

1. Masukkan **Username** dan **Password**
2. Tekan **Enter** atau klik **Unlock**
3. Gunakan **Tab** untuk navigasi antar field
4. **Ctrl+Alt+L** untuk lock screen cepat (jika Agent running)

### Menjalankan Service Agent

```bash
cd src/CompanyLock.Agent/bin/Release/net8.0-windows
./CompanyLock.Agent.exe
```

**Background Services:**

- 🔥 **Hotkey Monitoring**: Deteksi Ctrl+Alt+L
- ⏰ **Idle Detection**: Auto-lock setelah timeout
- 📡 **Communication**: Named Pipe untuk UI integration
- 📊 **Audit Logging**: Recording aktivitas sistem

## ⚙️ Konfigurasi

### Database Configuration

Database SQLite akan dibuat otomatis di:

```
%LOCALAPPDATA%\CompanyLock\local.db
```

### Sample Employee Data

File `sample_employees.csv` tersedia untuk testing:

```csv
Username,FullName,Department,Role,Password,IsActive
test01,Test User One,IT,User,password123,true
admin,Administrator,IT,Admin,admin123,true
```

### Konfigurasi Service

Edit file `appsettings.json` (jika ada):

```json
{
  "IdleTimeoutMinutes": 1,
  "HotkeyEnabled": true,
  "DatabasePath": "%LOCALAPPDATA%\\CompanyLock\\local.db"
}
```

## 🛠️ Pengembangan

### Prerequisites untuk Development

- Visual Studio 2022 atau VS Code
- .NET 8.0 SDK
- Git for version control

### Project Structure

```
CompanyLock/
├── src/
│   ├── CompanyLock.Core/           # Shared libraries
│   ├── CompanyLock.LocalAuth/      # Database & Auth
│   ├── CompanyLock.Agent/          # Background service
│   ├── CompanyLock.UI/             # Lock screen interface
│   ├── CompanyLock.AdminTool/      # Admin dashboard
│   └── CompanyLock.Installer/      # WiX installer
├── sample_employees.csv            # Sample data
├── test_*.bat                      # Testing scripts
├── CompanyLock.sln                 # Solution file
├── .gitignore                      # Git ignore rules
└── README.md                       # This file
```

### Development Commands

```bash
# Run AdminTool
dotnet run --project src/CompanyLock.AdminTool

# Run UI (Lock Screen)
dotnet run --project src/CompanyLock.UI

# Run Agent Service
dotnet run --project src/CompanyLock.Agent

# Build solution
dotnet build CompanyLock.sln --configuration Release
```

### Testing Scripts

- `test_companylock.bat` - Test basic functionality
- `test_new_timestamps.bat` - Test audit logging with timestamps
- `uninstall_service.bat` - Remove service installation

## 📚 Dokumentasi

### File Dokumentasi

- [`UI_DESIGN_DOCUMENTATION.md`](UI_DESIGN_DOCUMENTATION.md) - Dokumentasi design system UI dengan glassmorphism

### Database Schema

```sql
-- Employees table
CREATE TABLE Employees (
    Username TEXT PRIMARY KEY,
    FullName TEXT NOT NULL,
    Department TEXT,
    Role TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Sessions table
CREATE TABLE Sessions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL,
    LoginTime DATETIME NOT NULL,
    LogoutTime DATETIME,
    DeviceId TEXT
);

-- AuditEvents table
CREATE TABLE AuditEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT,
    EventType TEXT NOT NULL,
    EventDescription TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    DeviceId TEXT
);
```

### API Documentation

- **Named Pipe Communication**: Agent menggunakan `CompanyLockPipe` untuk IPC
- **Authentication Flow**: Argon2 hashing + DPAPI encryption
- **Service Management**: Windows Service Control Manager integration

## 🤝 Kontribusi

### Setup Git Repository

Untuk memulai kontribusi, setup repository Git terlebih dahulu:

```bash
# Initialize Git repository
git init

# Add remote origin (ganti dengan URL repository Anda)
git remote add origin https://github.com/yourusername/CompanyLock.git

# Add semua files (excluding yang ada di .gitignore)
git add .

# Commit initial
git commit -m "feat: initial CompanyLock offline system implementation"

# Push to GitHub
git push -u origin main
```

### Contribution Guidelines

1. **Fork** repository ini
2. **Create** feature branch (`git checkout -b feature/nama-fitur`)
3. **Commit** changes (`git commit -m 'feat: menambahkan fitur XYZ'`)
4. **Push** ke branch (`git push origin feature/nama-fitur`)
5. **Create** Pull Request

### Code Style Guidelines

- Gunakan **C# coding conventions**
- Tambahkan **XML documentation** untuk public methods
- **Unit tests** wajib untuk fitur baru
- **Consistent naming** untuk variables dan methods

## 🐛 Issue Tracking

Laporkan bug atau request fitur di [GitHub Issues](https://github.com/yourusername/CompanyLock/issues)

**Bug Report Template:**

- **Describe the bug**: Deskripsi singkat
- **Steps to reproduce**: Langkah-langkah reproduksi
- **Expected behavior**: Perilaku yang diharapkan
- **Environment**: OS, .NET version, dll

## 🔒 Security

### Security Features

- ✅ **Password Hashing**: Argon2id dengan salt
- ✅ **Data Encryption**: Windows DPAPI untuk data sensitif
- ✅ **Input Validation**: Semua input ter-validasi
- ✅ **Session Management**: Secure session dengan timeout
- ✅ **Audit Logging**: Complete activity tracking
- ✅ **Service Isolation**: Proper Windows service ACLs

### Reporting Security Issues

Untuk melaporkan security vulnerability, silakan buat issue dengan label "security" atau contact maintainer secara langsung.

## 📄 Lisensi

Proyek ini dilisensikan di bawah **MIT License**. Lihat file [LICENSE](LICENSE) untuk detail lengkap.

## 👨‍💻 Tim Pengembang

- **Lead Developer**: Offline Security System Specialist
- **UI/UX Design**: Modern Glassmorphism Design System
- **Security Architecture**: Windows Service & Authentication
- **Database Design**: SQLite Offline Database Architecture

## 🙏 Acknowledgments

- **Microsoft**: .NET Framework dan WPF technology stack
- **SQLite**: Reliable embedded database engine
- **Argon2**: Secure password hashing algorithm
- **Windows**: Enterprise-grade security infrastructure

---

## 📞 Support & Community

Untuk pertanyaan dan support:

- 🐛 **Issues**: [GitHub Issues](https://github.com/yourusername/CompanyLock/issues)
- 📖 **Documentation**: Check `UI_DESIGN_DOCUMENTATION.md` untuk UI guidelines
- 💻 **Development**: Contribution welcome via Pull Requests

---

**CompanyLock** - Solusi Keamanan Workstation Offline untuk Lingkungan Enterprise 🚀

_Built with .NET 8, SQLite, dan Windows Service Architecture_
