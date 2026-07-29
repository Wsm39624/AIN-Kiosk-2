# AIN Kiosk - Self-Service Visitor Management Platform (Prototype Demo)

A WPF (.NET 10) self-service kiosk solution for visitor registration, identity document scanning simulation (MRZ), visitor badge issuance, and digital privacy receipts.

---

## 🛠️ Quick Start & Clean-Clone Commands

To clone, restore, build, and run the automated behavioral test suite on a clean environment, execute:

```powershell
# 1. Clone repository
git clone <repository-url>
cd AIN_Kiosk

# 2. Restore dependencies
dotnet restore

# 3. Build solution
dotnet build --configuration Release

# 4. Execute test suite
dotnet test --configuration Release