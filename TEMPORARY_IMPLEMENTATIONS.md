# 📝 Temporary Implementations Log
> **Baseline:** Engineering Honesty Manifest & Prototype Tracking  
> **Compliance Target:** Kiosk Prototype Clearance Validation  

---

## 1. Simulated & Mock Components (Not Persisted)

The table below outlines the operational elements currently decoupled from physical target backends for simulation integrity:

| Component Name | Current Mock Behavior | Production Target Transition |
| :--- | :--- | :--- |
| **Database & API Storage** | Processed strictly inside ephemeral view-model runtime memory. | Enterprise Database Cluster Router |
| **Receptionist Notification** | Warning panel `BorderReceptionistAlert` simulates UI states only; no sockets pinged. | SignalR Real-Time Push Gateway |
| **Device Adapters** | `ScannerAdapter` executes a simulated static loop returning static document parameters. | Physical Hardware SDK / Driver Integration |

---

## 2. Hardcoded & Unapproved Test Configuration Values

> ⚠️ **Critical Compliance Notice:** The values listed below are configured exclusively for verification layouts and must be overwritten prior to operational deployment.

* 📅 **Data Retention Default:** The 10-year retention rule displayed in the Privacy Receipt is a synthetic default managed entirely outside the UI view context layer by `IKioskPrivacyNoticeProvider`. The WPF Kiosk behaves strictly as a stateless consumer and does not hardcode this configuration value.
* 🔐 **Receipt Integrity Check:** The generation of a pseudo-SHA-256 hash substring has been completely removed from the UI layer per engineering instructions. The kiosk now strictly exposes the opaque session token as a baseline public access reference.
---

## 3. Interfaces Declared But Not Wired

The following behavioral interfaces are fully declared in the architecture baseline but run on localized dummy loops:

1. `IVisitorSyncTransport`
   * **Current Status:** Placeholder endpoint mapping.
   * **Missing Wire:** Awaiting target RESTful cloud sync pipeline integration.

2. `IBadgePrinter`
   * **Current Status:** Routed into safe hardware bypass logic.
   * **Missing Wire:** Awaiting integration with physical Zebra badge printer spooling routines.

3. `IRegistrationQueue`
   * **Current Status:** Routed via `MockRegistrationQueue` to prevent runtime crashes.
   * **Missing Wire:** Ephemeral memory tracing only; requires a persistent offline local database or encrypted file queue before production clearance.