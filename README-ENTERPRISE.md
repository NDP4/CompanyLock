# 🔒 CompanyLock Enterprise Security System

**CompanyLock** adalah solusi keamanan workstation offline yang dirancang khusus untuk lingkungan enterprise. Sistem ini menyediakan autentikasi lokal, monitoring aktivitas, dan kontrol akses komprehensif tanpa memerlukan koneksi internet.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-Enterprise-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## ✨ Fitur Utama

### 🔐 Keamanan Enterprise

- **Autentikasi Offline**: Login lokal tanpa koneksi internet
- **Enkripsi Tingkat Enterprise**: Argon2 password hashing + Windows DPAPI
- **Kontrol Akses Multi-Level**: Role-based access (User/Admin)
- **Lock Screen Security**: Alt+F4, Alt+Tab, Windows key blocking
- **Startup Auto-Lock**: Otomatis mengunci setelah system boot

### 🖥️ Monitoring & Kontrol

- **Session Monitoring**: Real-time pemantauan sesi pengguna
- **Idle Detection**: Auto-lock setelah 30 detik idle
- **Global Hotkey**: Shortcut Ctrl+Alt+L untuk lock cepat
- **Duplicate Prevention**: Mencegah multiple instances

### 📊 Manajemen Data

- **Database Offline**: SQLite untuk penyimpanan lokal
- **Employee Management**: CRUD operations via AdminTool
- **Audit Logging**: Pencatatan aktivitas lengkap dengan timestamp
- **CSV Import/Export**: Bulk import karyawan
- **Auto Log Cleanup**: Retensi 30 hari otomatis

### 🎨 User Interface

- **Modern Design**: Glassmorphism lock screen
- **Admin Dashboard**: WPF-based management tools
- **Professional Icons**: Custom branding support
- **Keyboard Navigation**: Full accessibility support

## 🚀 Instalasi Cepat

### Persyaratan Sistem

- **OS**: Windows 10/11 (termasuk Ghost Spectre)
- **RAM**: Minimum 4GB
- **Storage**: 50MB free space
- **Privileges**: Administrator access

### Langkah Instalasi

1. **Download & Extract** komponen CompanyLock
2. **Run Installer** dengan hak Administrator:
   ```bash
   # Untuk Windows standar dan Ghost Spectre
   installer/CompanyLock-GhostSpectre/Install-Simple-Startup.bat
   ```
3. **Restart sistem** untuk aktivasi otomatis
4. **Test lock screen** dengan Ctrl+Alt+L

### Uninstall

```bash
installer/CompanyLock-GhostSpectre/Uninstall-Complete.bat
```

## 💼 Penggunaan Enterprise

### Admin Setup

1. Jalankan **CompanyLock AdminTool** dari desktop
2. Import data karyawan dari `sample_employees.csv`
3. Kelola user accounts dan permissions
4. Monitor audit logs dan aktivitas sistem

### Employee Usage

- **Auto-Lock**: Sistem otomatis terkunci saat startup dan idle
- **Manual Lock**: Tekan Ctrl+Alt+L kapan saja
- **Unlock**: Login dengan credentials perusahaan
- **Security**: Tidak ada bypass - sistem fully locked

### Corporate Deployment

- **Silent Installation**: Script mendukung automated deployment
- **Centralized Management**: AdminTool untuk IT management
- **Audit Compliance**: Lengkap logging untuk compliance
- **Scalable**: Deploy ke ratusan workstation

## 🔧 Customization

### Branding

- Replace `assets/icons/CompanyLock.ico` dengan logo perusahaan
- Rebuild dengan `build_with_icon.bat`
- Custom colors dan themes tersedia di source code

### Configuration

- Employee data: `sample_employees.csv`
- Settings: Tersimpan di SQLite database
- Logs: Auto-cleanup setelah 30 hari

## 🛡️ Keamanan

- **Encryption**: Argon2 + Windows DPAPI
- **Session Security**: Full session isolation
- **Audit Trail**: Comprehensive activity logging
- **Bypass Prevention**: Multi-layer protection
- **Ghost Spectre Ready**: Compatible dengan modified Windows

## 📞 Support

Sistem ini dirancang untuk enterprise deployment dengan fokus pada:

- **Reliability**: 99.9% uptime
- **Performance**: Minimal resource usage (< 100MB RAM)
- **Compatibility**: Windows 10/11 semua varian
- **Maintenance**: Self-managing dengan auto-cleanup

---

**CompanyLock** - _Securing Enterprise Workstations Since 2025_
