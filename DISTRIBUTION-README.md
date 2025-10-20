# 📦 CompanyLock Distribution Guide

## ⚠️ File Eksekusi Tidak Disertakan

File eksekusi (.exe) dan paket distribusi (.zip) **tidak disertakan** dalam repository Git karena ukurannya yang besar (>100MB per file).

## 🔧 Cara Membuat Distribusi

### 1. Build Komponen
```bash
# Build semua komponen dengan icon
build_with_icon.bat
```

### 2. Buat Paket Distribusi
```bash
# Membuat ZIP distribusi profesional
create_distribution.bat
```

### 3. Hasil Build
Setelah build, akan menghasilkan:
- `CompanyLock-Enterprise-YYYY-MM-DD.zip` (~200MB)
- `installer/CompanyLock-Enterprise/Agent/CompanyLock.Agent.exe` (~160MB)
- `installer/CompanyLock-Enterprise/AdminTool/CompanyLock.AdminTool.exe` (~160MB)
- `installer/CompanyLock-Enterprise/UI/CompanyLock.UI.exe` (~160MB)

## 📋 Struktur Distribusi

```
CompanyLock-Enterprise/
├── Install-Simple-Startup.bat     # Installer utama
├── Uninstall-Complete.bat         # Uninstaller
├── README.md                      # Panduan
├── DEPLOYMENT-CHECKLIST.md       # Checklist deployment
├── sample_employees.csv           # Template data
├── VERSION.txt                    # Info versi
├── Agent/
│   └── CompanyLock.Agent.exe      # [Build required]
├── AdminTool/
│   └── CompanyLock.AdminTool.exe  # [Build required]
└── UI/
    └── CompanyLock.UI.exe         # [Build required]
```

## 🚀 Quick Start untuk Developer

1. **Clone repository:**
```bash
git clone https://github.com/NDP4/CompanyLock.git
cd CompanyLock
```

2. **Build distribusi:**
```bash
build_with_icon.bat
create_distribution.bat
```

3. **Hasil:** File `CompanyLock-Enterprise-YYYY-MM-DD.zip` siap distribusi

## 📁 File yang Diabaikan Git

File berikut diabaikan oleh Git (lihat `.gitignore`):
- `*.exe` - File eksekusi
- `*.zip` - Paket distribusi  
- `dist/` - Folder build sementara
- `installer/**/Agent/*.exe`
- `installer/**/AdminTool/*.exe`
- `installer/**/UI/*.exe`

## 💡 Catatan untuk Production

Untuk deployment produksi:
1. Build di environment yang bersih
2. Test installer di sistem target
3. Verifikasi compatibility Ghost Spectre
4. Distribusikan ZIP ke target computers

---

**Untuk mendapatkan file eksekusi, lakukan build menggunakan script yang disediakan.**