# Architectural Decisions Document (ADR)
Project Scope: AIN Kiosk System Targeted Closure and Refactoring
Status: Partially Separated / Targeted Closure Review

# 1. Context and Problem Statement
The legacy implementation of MainWindow.xaml.cs operated as a Massive View Controller. The targeted closure refactoring improves separation of concerns while maintaining prototype compatibility prior to enterprise backend integration.

# 2. Architecture Status and Components

## A. Separation of MainWindow Code Behind
Status: Partially Separated
Details: UI event orchestration remains in MainWindow partial files, while workflow state management, receipt generation, and localization are delegated to independent service layers.

## B. Event Driven Workflow Subsystem
Status: Completed
Details: KioskWorkflowService manages visitor state transitions across Walk In, Pre Registered, and Retroactive paths, handling idle timeouts and session resets.

## C. Hardware Peripheral Adapters
Status: Mock Implementation
Details: PrinterAdapter and ScannerAdapter wrap local mock providers (MockDocumentScanner and RawPrinterHelper). Direct hardware SDK calls are isolated behind adapter interfaces.

## D. Local Mock Configuration Providers
Status: Local Mock Implementation
Details: LocalMockKioskConfigurationProvider and LocalMockKioskPrivacyNoticeProvider supply local developmental values for tenant branding, privacy contacts, and data retention statements. Dynamic configuration loading is pending backend integration.

## E. Localization Service
Status: Completed
Details: LocalizationService resolves UI strings dynamically between Arabic and English, enforcing neutral action wording and excluding obsolete consent text.

## F. Data Privacy and Memory Handling
Status: Development Hardening
Details: Raw PII logging is strictly excluded from debug streams using synthetic correlation references. Due to .NET string immutability, immediate string erasure from RAM cannot be guaranteed by garbage collection; mutable byte buffers are zeroed where feasible.

## G. Audit Log Representation
Status: Pending Backend Audit Integration
Details: Local session references serve as developmental placeholders. Runtime evidence logging is pending integration with the approved enterprise audit service.