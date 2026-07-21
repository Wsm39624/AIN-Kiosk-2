# Temporary Implementations Log
> **Baseline:** Engineering Honesty Manifest & Prototype Tracking  
> **Compliance Target:** Kiosk Prototype Clearance Validation  

---

## 1. Simulated & Mock Components (Not Persisted)

The table below outlines the operational elements currently decoupled from physical target backends for simulation integrity:

| Component Name | Current Behavior & Hardware Status | Production Target Transition |
| :--- | :--- | :--- |
| **Database & API Storage** | Processed strictly inside ephemeral view-model runtime memory (`Simulated - not persisted`). | Enterprise Database Cluster Router |
| **Receptionist Notification** | Warning panel `BorderReceptionistAlert` simulates UI states only; no backend sockets pinged (`Mock notification not sent`). | SignalR Real-Time Push Gateway |
| **OCR Scanner Adapter** | `ScannerAdapter` executes a simulated static loop returning verified document parameters (`Scanner fallback demo`). | Physical Hardware SDK / Driver Integration |
| **Zebra Badge Printer** | **Functional Hardware Support:** `RawPrinterHelper` contains active ZPL badge spooling routines tested on physical Zebra hardware. **Fallback Mode:** Includes an automatic emulator fallback (`Printer fallback demo`) when hardware is detached. | Direct Network/USB Hardware Socket Binding |

---

## 2. Hardcoded & Unapproved Test Configuration Values

> **Critical Compliance Notice:** The values listed below are configured exclusively for verification layouts and must be overwritten prior to operational deployment.

* **Data Retention Default:** The 10-year retention rule displayed in the Privacy Receipt is a synthetic default managed entirely outside the UI view context layer by `IKioskPrivacyNoticeProvider`. The WPF Kiosk behaves strictly as a stateless consumer and does not hardcode this configuration value.
* **Receipt Integrity Check:** The generation of a pseudo-SHA-256 hash substring has been completely removed from the UI layer per engineering instructions. The kiosk now strictly exposes the opaque session token as a baseline public access reference.

---

## 3. Interfaces Declared But Not Wired

The following behavioral interfaces are fully declared in the architecture baseline but run on localized dummy loops:

1. **`IVisitorSyncTransport`**
   * **Current Status:** Placeholder endpoint mapping.
   * **Missing Wire:** Awaiting target RESTful cloud sync pipeline integration.

2. **`IBadgePrinter`**
   * **Current Status:** Wired to `BadgePrintService` with dual-mode support (Active ZPL spooler + Fallback simulation for offline demos).
   * **Missing Wire:** Awaiting persistent print queue status monitoring from backend.

3. **`IRegistrationQueue`**
   * **Current Status:** Routed via `MockRegistrationQueue` to prevent runtime crashes (`Simulated - not persisted`).
   * **Missing Wire:** Ephemeral memory tracing only; requires a persistent offline local database or encrypted file queue before production clearance.

4. **`IReceiptReferenceProvider`**
   * **Current Status:** Fully implemented via `MockReceiptReferenceProvider` to handle the missing backend deployment.
   * **Engineering Truthfulness:** It generates an explicitly labeled synthetic development token (`DEV-SYNTHETIC-*`) or URL reference (`https://receipt.ain.ebtco.com/r/{token}`). It is strictly not presented as a production security implementation.