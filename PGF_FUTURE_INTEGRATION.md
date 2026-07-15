# 🔮 PGF Future Integration Document
> **Reference:** Post-Cleanup Gate Integration Roadmap  
> **System Context:** Connecting AIN Kiosk with Enterprise ABP.io Core Systems  

---

## 1. Architectural Strategy
Once the AIN Kiosk passes the Engineering Cleanup Gate, the transition from local mockup simulation 
to a hardened production-ready status will follow a phased, decoupled integration strategy.
No core backend assemblies will be directly referenceable by the WPF client, maintaining strict boundary contexts.

---

## 2. Integration Roadmap & Milestones

### 📍 Phase 1: Robust Offline Persistence & Hardening
* **Objective:** Replace `MockRegistrationQueue` with an encrypted local persistence database.
* **Target Architecture:** Integrate SQLite or LiteDB secured with AES-256 encryption using keys protected by the **Windows Data Protection API (DPAPI)** or device **TPM (Trusted Platform Module)** chips.
* **Operational Logic:** The kiosk will safely buffer visitor transactions locally during network blackouts and auto-synchronize packets using exponential backoff retry algorithms when connection is restored.

### 📍 Phase 2: Authoritative Dynamic Provisioning
* **Objective:** Wire `IKioskConfigurationProvider` and `IKioskPrivacyNoticeProvider` directly to the centralized ABP.io backend.
* **Target Architecture:** Transition providers to consume dynamic JSON payloads fetched from secure RESTful endpoints matching the hardware terminal registration key.
* **Operational Logic:** Allows central administrators to alter data retention policies, contact emails, and organizational tenant names on-the-fly, distributing changes to all active kiosks instantly.

### 📍 Phase 3: Hardware Peripheral Integration
* **Objective:** Wire raw scanner and printer loops to physical device SDKs.
* **Target Architecture:** Swap simulated OCR models with actual vendor drivers for the MRZ Passport scanner, and replace the Zebra Printer Emulator with direct TCP/IP socket connections to physical Zebra industrial hardware.

### 📍 Phase 4: Secure Web Portal & GDPR/PDPL Rights Execution
* **Objective:** Safely bridge "View My Personal Data" and "Exercise My Rights" requests.
* **Target Architecture:** Ensure all data accesses are executed strictly within the secure corporate web portal after scanning the public receipt URL. Authentication will be enforced via secure transient SMS/Email OTP challenges managed natively by the ABP backend.