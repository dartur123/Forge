# Forge

A workflow-adaptive inventory & procurement system 
for Philippine SME manufacturers.

## The Problem
Manufacturing companies abandon their ERP systems 
because the software forces them to change their 
process — instead of the other way around.

## The Solution
Forge bends to the company's workflow, not the 
other way around. Configurable approval rules, 
natural-language queries, and an AI procurement 
agent that respects how *your* company actually works.

## Tech Stack
- ASP.NET Core Web API (.NET 10)
- EF Core (coming Week 2)
- Azure (coming Phase 2)
- Azure OpenAI + RAG (coming Phase 3)
- Microsoft Agent Framework (coming Phase 4)

## Domain Model
- Material (with lot/batch tracking)
- Supplier & Subcontractor
- Location (Warehouse, Production Floor, FG Storage)
- Lot (batch tracking with PHP costing)
- Stock Movement (append-only ledger)
- Purchase Order + Lines (with multi-currency for overseas)
- Subcon Order + Lines (with multi-currency for overseas)
- Bill of Materials (recursive, multi-level)
- Approval Rules (configurable per company)
- Company Settings (costing method, base currency)
- Users + Roles

## Status
🚧 Phase 1 — Backend in progress
