<div align="center">

![ATLAS Logo](images/atlas-logo.png)

# **ATLAS**
### *Amdocs Technical Lifecycle & Asset System*

[![Build](https://github.com/webtracer/ATLAS/actions/workflows/build.yml/badge.svg)](https://github.com/webtracer/ATLAS/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Private-informational)](#license)

A Blazor Server platform with an ASP.NET Core API and MySQL backend, designed to manage servers, environments, assets, metadata, and lifecycle operations across multiple business units.

</div>

---

## 📘 Overview

ATLAS is an internal lifecycle and asset management platform designed to bring clarity, structure, and automation to distributed infrastructure environments. Its purpose is to unify the tracking of:

- Servers and roles  
- Customer environments  
- VCenters and clusters  
- Metadata and configuration  
- Business-lifecycle processes  
- History, auditing, and compliance  

The project is built for expansion, modularity, and data-driven workflows — including future Excel import, API integration, and site-specific customization.

---

## 🧱 Architecture

ATLAS is structured into a clean, scalable solution:

```text
Amdocs.Atlas.sln
│
├── Amdocs.Atlas.Core        # Entity models, DTOs, shared contracts
├── Amdocs.Atlas.Data        # EF Core DbContext + Repositories
├── Amdocs.Atlas.Api         # ASP.NET Core REST API
└── Amdocs.Atlas.Web         # Blazor Server front-end (UI)
```

### Tech Stack

- **Blazor Server (.NET 8/9)**  
- **ASP.NET Core Web API**  
- **Entity Framework Core**  
- **MySQL / MariaDB**  
- **AutoMapper**  
- **Dependency Injection throughout**  
- **IHttpClientFactory for API calls**  

---

## 📂 Current Modules

### Servers

- API CRUD completed  
- Blazor UI responsive with Add/Edit/Delete  
- Environment & Role selection  
- Validation + DTO mapping  

### Environments

- Model & DTO structure complete  
- CRUD planned after VCenters  

### VCenters

- Controller scaffolded  
- Next module scheduled  

---

## 🛣️ Roadmap

### Phase 1 — Core Entities

- [x] Servers API CRUD  
- [ ] Servers Add/Edit/Delete UI polish  
- [ ] VCenters API CRUD  
- [ ] VCenters Blazor UI  
- [ ] Environments API CRUD  
- [ ] Environments UI  

### Phase 2 — Metadata Tables

- [ ] Customers  
- [ ] Environment Types  
- [ ] Server Roles  
- [ ] VCenter Clusters  

### Phase 3 — Enhancements

- [ ] Advanced filtering & searching  
- [ ] Excel import for Servers  
- [ ] Bulk update operations  
- [ ] Audit history & versioning  
- [ ] Dark/Light UI themes  

### Phase 4 — Deployment & Ops

- [ ] Dockerization  
- [ ] Build pipeline (GitHub Actions)  
- [ ] Automated schema migrations  
- [ ] Staging environment  

---

## 📦 Database Design

ATLAS uses MySQL with normalized tables for:

- `servers`  
- `environments`  
- `vcenters`  
- `customers`  
- Metadata extension tables  

Foreign keys define a consistent, relational structure that avoids duplication and supports filtering, grouping, and future BI reporting.

---

## 🚀 Getting Started

### Prerequisites

- .NET 8 or later  
- MySQL Server or MySQL Cluster node  
- Rider or Visual Studio  
- Git  

### Setup

```bash
git clone https://github.com/webtracer/ATLAS.git
cd ATLAS
```

Configure your connection string in:

```text
Amdocs.Atlas.Api/appsettings.json
Amdocs.Atlas.Web/appsettings.json
```

Then run:

```bash
dotnet restore
dotnet build
```

Start in development mode:

```bash
dotnet run --project Amdocs.Atlas.Web
```

---

## 🖼 Assets

The repository expects image assets under an `images/` folder at the solution root:

- `images/atlas-logo-square.png` – Square ATLAS logo for README / GitHub usage  
- `images/atlas-logo-variant-c.png` – Full logo with ATLAS text (for splash screens, docs, etc.)  

You can adjust these paths if you prefer a different layout; just update the image references at the top of this README.

---

## 🤝 Contributions

ATLAS is an evolving system. Contributions, feature ideas, and architecture improvements are always welcome — this platform is designed to grow with the organization’s needs.

---

## 📄 License

Private internal project – © 2025 Randy Bitts / RAB Consulting  
Unauthorized redistribution is prohibited.

---

<div align="center">

*Built with care, caffeine, and a bit of arcane magic from Aeris.* ✨

</div>
