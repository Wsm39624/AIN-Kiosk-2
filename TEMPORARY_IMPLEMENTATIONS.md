# Temporary & Mock Implementation Register

This document tracks mock components, simulation adapters, and non-production logic in the current prototype build.

---

## 📋 Active Prototype Components

### 1. `MockReceiptReferenceProvider`
* **Interface**: `IReceiptReferenceProvider`
* **Behavior**: Generates synthetic reference tokens prefixed with `DEV-SYNTHETIC-` for digital privacy receipts.
* **Production Path**: Replace with production backend API token generator.

### 2. `MockRegistrationQueue`
* **Interface**: `IRegistrationQueue`
* **Behavior**: Logs retroactive visitor registration entries to Debug output without persistent storage.
* **Production Path**: Connect to secure encrypted database/queue transport.

### 3. `MockScannerAdapter`
* **Interface**: `IScannerAdapter`
* **Behavior**: Simulates identity document scanning and MRZ extraction.
* **Production Path**: Integrate production hardware drivers for OCR/MRZ readers.

### 4. Prototype Supervisor Gate
* **Location**: `MainWindow.Supervisor.cs`
* **Behavior**: PIN authentication using development code (`1234`).
* **Notice**: Explicitly labeled `[Prototype Supervisor Gate Not Production Authentication]`.

### 5. `BadgePrintService`
* **Behavior**: Simulates ZPL/Printer integration for badge printing without storing real individual names in code or templates.
* **Production Path**: Connect directly to Zebra API / hardware spooler.