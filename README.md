# Forge

![CI](https://github.com/dartur123/Forge/actions/workflows/ci.yml/badge.svg)

A workflow-adaptive inventory & procurement system for Philippine SME manufacturers.

## Why

Most ERP rollouts in small manufacturing shops die the same way: the software
demands the company change its process to fit the system. People quietly go
back to spreadsheets, and the ERP becomes expensive shelfware.

Forge starts from the opposite assumption — the system bends to the company's
workflow, not the other way around. Configurable approval rules now; natural-
language queries and an AI procurement agent later, all built to respect how
each company actually operates.

## Tech

- ASP.NET Core Web API (.NET 10), Clean Architecture (Domain / Application / Infrastructure / API)
- EF Core + PostgreSQL
- Docker — full stack runs with one `docker compose up`
- Azure Container Registry + Container Apps
- CI on GitHub Actions: restore, build, integration tests on every push to develop
- xUnit + Testcontainers — tests run against real PostgreSQL, not in-memory fakes

Planned: Azure OpenAI + RAG, then an agent layer via Microsoft Agent Framework.

## Domain

Material (lot/batch tracked) · Supplier · Subcontractor · Location ·
Lot (PHP costing) · Stock Movement (append-only ledger) ·
Purchase Order · Subcon Order · Bill of Materials (multi-level) ·
Approval Rules (per-company) · Company Settings · Users + Roles

## Status

**Backend — in active development**

- 16-entity domain model, factory pattern + guarded invariants throughout
- PO / Subcon / BOM refactor complete (status machines, immutable approved BOMs)
- StockLedgerService with atomic transactions and row-level locking
- Role-based authorization on approval steps
- Integration test suite on Testcontainers
- Live on Azure Container Apps (PostgreSQL connected; CD pipeline auto-deploys and migrates on push to main)
