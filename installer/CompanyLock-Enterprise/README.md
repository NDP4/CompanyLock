# 🔒 CompanyLock Distribution Package

## 📦 Paket Distribusi Enterprise

Paket ini berisi installer siap deploy untuk **CompanyLock Enterprise Security System** - solusi keamanan workstation offline untuk lingkungan perusahaan.

### 📁 Struktur Paket

```
CompanyLock-Enterprise/
├── Install-Simple-Startup.bat     # Installer utama (Ghost Spectre ready)
├── Uninstall-Complete.bat         # Uninstaller lengkap
├── sample_employees.csv           # Template data karyawan
├── README.md                      # Panduan instalasi
├── AdminTool/
│   ├── CompanyLock.AdminTool.exe  # Tool manajemen admin
│   └── CompanyLock.AdminTool.dll  # Dependencies
├── Agent/
│   ├── CompanyLock.Agent.exe      # Service utama (background)
│   └── CompanyLock.Agent.dll      # Dependencies  
└── UI/
    ├── CompanyLock.UI.exe         # Lock screen interface
    └── CompanyLock.UI.dll         # Dependencies
```

## 🚀 Instalasi Cepat

### 1. Persiapan
- Extract file ZIP ke lokasi sementara
- Pastikan menjalankan sebagai **Administrator**
- Tutup semua aplikasi yang tidak perlu

### 2. Instalasi
```bash
# Klik kanan -> "Run as Administrator"
Install-Simple-Startup.bat
```

### 3. Restart
Restart sistem untuk aktivasi otomatis startup.

## ✅ Verifikasi Instalasi

Setelah restart, pastikan:
- [x] Agent berjalan di Task Manager
- [x] Shortcut AdminTool di Desktop
- [x] Shortcut Lock Screen di Desktop  
- [x] Ctrl+Alt+L mengaktifkan lock screen

## 🔧 Manajemen Sistem

### Admin Tool
1. **Buka AdminTool** dari desktop shortcut
2. **Import data** dari `sample_employees.csv`
3. **Kelola users** - tambah/edit/hapus karyawan
4. **Monitor logs** - audit trail aktivitas

### Employee Management
Format CSV: `Name,EmployeeId,Department,Password`
```csv
John Doe,EMP001,IT Department,password123
Jane Smith,EMP002,HR Department,hr2024
```

## 🛠️ Troubleshooting

### Issue: Agent tidak berjalan
**Solusi**: Restart sistem atau jalankan manual dari shortcut

### Issue: Lock screen tidak muncul  
**Solusi**: Tekan Ctrl+Alt+L, pastikan Agent aktif

### Issue: Instalasi gagal
**Solusi**: 
1. Uninstall dengan `Uninstall-Complete.bat`
2. Restart sistem
3. Install ulang sebagai Administrator

## 🗑️ Uninstall

```bash
# Klik kanan -> "Run as Administrator" 
Uninstall-Complete.bat
```

Uninstaller akan menghapus:
- Semua file program
- Registry entries  
- Startup configurations
- Database dan logs
- Desktop shortcuts

## 🛡️ Kompatibilitas

### ✅ Sistem Operasi Didukung
- Windows 10 (semua edisi)
- Windows 11 (semua edisi)  
- Windows Ghost Spectre
- Windows Modified/Lite versions

### ⚙️ Persyaratan Minimal
- **RAM**: 4GB
- **Storage**: 50MB free
- **Privileges**: Administrator access
- **Framework**: Tidak perlu install .NET (self-contained)

## 📞 Informasi Teknis

### Komponen Utama
- **Agent**: Background service untuk monitoring
- **UI**: Lock screen interface  
- **AdminTool**: Management dashboard
- **Database**: SQLite offline storage

### Keamanan
- **Encryption**: Argon2 password hashing
- **Data Protection**: Windows DPAPI
- **Session Security**: Full isolation
- **Audit Logging**: Comprehensive tracking

### Performance
- **Memory Usage**: < 100MB total
- **CPU Usage**: < 1% idle
- **Startup Time**: < 3 seconds
- **Database Size**: Auto-cleanup 30 hari

---

**CompanyLock Enterprise** - Deployed with confidence ✅