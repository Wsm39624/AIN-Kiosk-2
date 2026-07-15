# 🔗 Integration Notes: Connecting to ABP.io Framework
> **Architectural Boundary:** Client-Side Decoupling Matrix  

---

## 1. Kiosk-Side Boundary Context

To preserve memory efficiency and maintain system speed on remote terminal hardware:
* 🛡️ **Zero Dynamic Embedding:** No direct ABP.io package framework assemblies or central SaaS platform libraries are compiled into the client executable.
* 🛰️ **Contract-Driven Pattern:** Communication with the enterprise core layer will execute exclusively via decentralized, stateless, JSON-based RESTful service interfaces.

---

## 2. Shared Multi-Tenant Mapping (`IMultiTenant` Core)

* **Data Isolation Layer:** The transient properties mapped inside `KioskConfigurationProvider` (such as `TenantName`) will be explicitly bound to the infrastructure context utilizing ABP's native backend `IMultiTenant` filters.
* **Dynamic Setting Injection:** The terminal software will hit a specialized gateway registration pipeline based on its physical installation token, allowing the ABP Commercial SaaS setup to dynamically deliver tailored theme definitions, localized variables, and compliance retention thresholds down to the device terminal cache.

---

## 3. Migrating Ephemeral Services to Authoritative Modules

The current prototype services will be phased out and routed into authoritative enterprise modules as follows: