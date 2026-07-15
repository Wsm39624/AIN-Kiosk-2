# 🏛️ Architectural Decisions Document (ADR)
> **Project Scope:** AIN Kiosk System Clean-Up & Refactoring  
> **Status:** Approved / Architecture Secured  

---

## 1. Context & Problem Statement
The legacy implementation of `MainWindow.xaml.cs` suffered from severe structural coupling, operating as a classic 
**Massive View Controller (God Object)**. This created several critical engineering failures:

* 🛑 **UI & Business Blending:** UI layout rules were heavily intertwined with local state persistence logic.
* 🛑 **Hardware Tight Coupling:** Raw hardware printer ZPL commands and third-party device SDK calls were invoked directly 
* inside button events.
* 🛑 **Test Blockage:** Isolation boundaries were entirely breached, preventing any form of independent automated testing.

---

## 2. Architecture Decisions Implemented

To pass the Kiosk Engineering Cleanup Gate and comply with corporate software design regulations, 
the following decoupling steps were executed:

### 📑 A. Deconstruction of MainWindow.xaml.cs
All domain-specific and device-level tasks were permanently evacuated from the view code-behind layer. 
* **Target Destination:** Transferred into independent, loosely coupled services under the isolated `AIN_Kiosk.Services` namespace.

### 🔄 B. Event-Driven Workflow Subsystem
Eradicated inline conditional flow switching by engineering a dedicated state controller.
* **Component Built:** `KioskWorkflowService`
* **Responsibility:** Handles core visitor state transitions (*Walk-In, Pre-Registered, and Retroactive paths*)
* and triggers asynchronous idle session timeouts without maintaining any direct references to UI controls.

### 🔌 C. Boundary Isolation via Adapter Pattern
Introduced a strict abstraction layer between the application layer and physical peripheral components.
* **Component Built:** An independent `Adapters` namespace containing explicit wrappers (`PrinterAdapter` and `ScannerAdapter`).
* **Impact:** The UI view layer now maintains **0% direct dependency** on hardware vendor SDKs or native raw printer communication streams.

### 🌐 D. Dictionary-Based Localization Service
Eradicated all inline hardcoded multi-lingual toggle statements.
* **Mechanism:** Encapsulated the language translation pipeline into a unified `LocalizationService` 
* that dynamically resolves interface keys, establishing a clean multi-lingual provider architecture.