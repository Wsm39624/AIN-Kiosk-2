# AIN Kiosk - Visitor Management System

An enterprise-grade Windows Presentation Foundation (WPF) desktop kiosk prototype designed for secure, localized corporate visitor registration and legal compliance logging.

---

## What was Built:
* **Core Kiosk Workflow**: A multi-step WPF graphical user interface facilitating visitor data capturing (Host Name, Mobile Number) and dynamic state transitions.
* **Local Secure QR Generation**: Entirely offline, in-memory QR code generation leveraging a randomized opaque token (`AIN-DEMO:{token}`) to completely isolate visitor PII and eliminate external data leakage.
* **Compliance Presentation**: A dedicated privacy layout to present data retention statements safely using sanitized local configurations.
* **Strict Repository Hygiene**: A fully audited repository layout completely isolated from IDE metadata (`.vs`), compilation binaries (`bin`/`obj`), and localized AI cache tools.

---

## Tech Stack & Prerequisites:
* **Operating System**: Microsoft Windows 10 or Windows 11 desktop environments.
* **Framework**: .NET 10.0 (or .NET SDKs compatible with .NET 5+ requirements).
* **UI Architecture**: Windows Presentation Foundation (WPF) utilizing XAML layouts.
* **Core Packages**: `QRCoder` NuGet Package (Version 1.8.0) utilized for local image stream generation.
* **Version Control**: Git for managing incremental, meaningful codebase checkpoints.

---

## How to Run the Project:

### 1. Environment Setup
Clone the sanitized repository structure and navigate to the project root:
```bash
git clone [https://github.com/Wsm39624/AIN-Kiosk-2.git](https://github.com/Wsm39624/AIN-Kiosk-2.git)
cd AIN-Kiosk-2