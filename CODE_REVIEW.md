# Forge — Code Review Summary

**Branch:** `feature/approval-instance-workflow` · **Reviewed:** 2026-07-02
**Stack:** .NET 10, EF Core (Npgsql/PostgreSQL 17), layered architecture (`Domain` / `Application` / `Infrastructure` / `Api` / `Tests`)

## 1. Domain entities — factory-pattern conformance

Criteria: `protected` ctor · `private` setters · static `Create` w/ validation · `Update`/`Deactivate` mutators · `DomainException` on invalid state.

| Entity | Status | Notes |
|---|---|---|
| `Material` | Done | Full pattern; the reference implementation. |
| `Supplier` | Done | Full pattern. |
| `Location` | Done | Full pattern. |
| `LocationType` | Done | Full pattern. |
| `Lot` | Done | Full pattern. |
| `ApprovalInstance` | Done (rich-domain variant) | Encapsulated + `Create`; uses state-transition methods (`Approve`/`Reject`/`Resubmit`/`AdvanceToNextStep`) instead of generic `Update`/`Deactivate`. Appropriate. |
| `StockMovement` | Partial (by design) | Protected ctor, private setters, `Create` w/ validation, `DomainException`. No `Update`/`Deactivate` — correct for an append-only ledger row, but worth confirming that's intentional. |
| `ApprovalDecision` | Partial (by design) | Same as above — immutable record, `Create` only. |
| `ApprovalRule` | Partial | Encapsulated + `Create` + `Deactivate`, but no `Update`. |
| `User` | Not started | Anemic: public get/set, no ctor/factory/validation. |
| `Role` | Not started | Anemic. |
| `CompanySettings` | Not started | Anemic. |
| `PurchaseOrder` | Not started | Anemic. |
| `PurchaseOrderLine` | Not started | Anemic. |
| `SubconOrder` | Not started | Anemic. |
| `SubconOrderLine` | Not started | Anemic. |
| `Subcontractor` | Not started | Anemic. |
| `BillOfMaterials` | Not started | Anemic. |
| `BillOfMaterialsLine` | Not started | Anemic. |

**Tally:** 6 done · 3 partial · 10 not started (19 entities). The unrefactored cluster is entirely the PO / Subcon / BOM domain plus `User`/`Role`/`CompanySettings`.

## 2. Before / after reference

### Completed — `Forge.Domain/Material.cs`
```csharp
    using Forge.Domain.Enums;
    using Forge.Domain.Exceptions;

    namespace Forge.Domain;

    public class Material
    {
        protected Material() { }

        public int Id { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public MaterialType Type { get; private set; }
        public string? Description { get; private set; }
        public string UnitOfMeasure { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public List<Lot> Lots { get; private set; } = new();

        public static Material Create(string sku, string name, MaterialType materialType, string? description, string unitOfMeasure)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new DomainException("SKU is required.");

            if(string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required.");

            if (string.IsNullOrWhiteSpace(unitOfMeasure))
                throw new DomainException("Unit of measure is required.");

            return new Material
            {
                Sku = sku,
                Name = name,
                Type = materialType,
                Description = description,
                UnitOfMeasure = unitOfMeasure,
            };
        }

        public void Update(string sku, string name, MaterialType materialType, string? description, string unitOfMeasure)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new DomainException("SKU is required.");

            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required.");

            if (string.IsNullOrWhiteSpace(unitOfMeasure))
                throw new DomainException("Unit of measure is required.");

            Sku = sku;
            Name = name;
            Type = materialType;
            Description = description;
            UnitOfMeasure = unitOfMeasure;
        }

        public void Deactivate() => IsActive = false;
    }
```
> Note: the entire file is indented ~4 spaces (an anomaly vs. every other domain file).

### Not yet refactored — `Forge.Domain/PurchaseOrder.cs`
```csharp
using Forge.Domain.Enums;

namespace Forge.Domain;

public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; set; } = "PHP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public decimal TotalAmountForeign { get; set; }
    public decimal TotalAmountPhp { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public List<PurchaseOrderLine> Lines { get; set; } = new();
}
```
> Fully mutable, no invariants — `Status`, totals, and currency can be set to anything from anywhere.

## 3. Application services & public methods

| Service (interface) | Public methods |
|---|---|
| `MaterialService` (`IMaterialService`) | `CreateMaterialAsync`, `UpdateMaterialAsync`, `GetMaterialAsync`, `GetAllMaterialsAsync`, `GetMaterialByTypeAsync`, `DeactivateMaterialAsync` |
| `SupplierService` (`ISupplierService`) | `CreateSupplierAsync`, `UpdateSupplierAsync`, `GetSupplierAsync`, `GetAllSuppliersAsync`, `DeactivateSupplierAsync` |
| `LotService` (`ILotService`) | `CreateLotAsync`, `UpdateLotAsync`, `GetLotByIdAsync`, `GetAllLotsAsync`, `DeactivateLotAsync` |
| `StockLedgerService` (`IStockLedgerService`) | `PostMovementAsync`, `PostMovementWithinTransactionAsync`, `GetLotHistoryAsync`, `GetLotCurrentQuantity`, `GetLotQuantitiesAsync` |
| `ApprovalService` (`IApprovalService`) | `GetRequiredApprovalsAsync`, `RequiresApprovalAsync`, `GetRuleAsync`, `CreateRuleAsync`, `DeactivateRuleAsync`, `StartApprovalAsync`, `ApproveStepAsync`, `RejectStepAsync`, `ResubmitAsync`, `GetInstanceAsync` |

All five are registered `Scoped` in `Program.cs`. Each takes `ForgeDbContext` directly (no repository layer); `LotService` additionally depends on `IStockLedgerService`.

## 4. Deploy readiness

| Item | State |
|---|---|
| **API Dockerfile** | None. No Dockerfile anywhere in the repo — the API is not containerized. |
| **docker-compose.yml** | Postgres only. Single `postgres:17` service (`forge_user` / `forge_dev_password` / `forge_db`), port `5432`, named volume `forge_pg_data`. No API service — `docker compose up` starts the DB but not the app. Credentials hardcoded in plaintext (dev-only, but see section 5). |
| **GitHub Actions** | None effective. `.github/workflows/` exists but is empty — no CI/CD, no build/test/lint on push. |
| **Connection strings / secrets** | Base `appsettings.json` (tracked) ships an empty `ConnectionStrings:ForgeDb`. `appsettings.Development.json` holds the working dev string with password — correctly gitignored & untracked (only the empty base file is committed). `UserSecretsId` is configured in `Forge.Api.csproj`. `Program.cs` reads via `GetConnectionString("ForgeDb")`, so env-var override works, but no production secret source is wired or documented. |

**Bottom line:** local-dev ready (DB in compose + dev appsettings); **not** deploy-ready — no app container, no CI, no production secret path.

## 5. Inconsistencies, smells & architectural drift

**Architectural**
1. **Two-speed domain model.** Encapsulated factory entities coexist with fully-anemic ones (section 1). The PO/Subcon/BOM/User cluster has no invariants — the refactor is roughly a third done.
2. **No authn/authz despite an approval domain.** `Program.cs` calls `UseAuthorization()` but registers no authentication scheme and no `[Authorize]`. Critically, `ApproveStepAsync(instanceId, userId, comment)` never checks the user holds the step's `RequiredRoleId` — any `userId` can approve any step. Functional/security gap.
3. **No repository abstraction** — services depend directly on `ForgeDbContext`. Fine as a deliberate choice, but note it's inconsistent with the heavy encapsulation effort in the domain.

**Correctness / behavior**
4. **Soft-delete is written but never read.** `IsActive`/`Deactivate` exist, but no query filters on `IsActive` (`GetAllMaterialsAsync`, `GetAllSuppliersAsync`, etc. all return deactivated rows). Either add a global query filter or filter per query.
5. **Approval completion logic is fragile.** `ApproveStepAsync` finalizes when `requiredApprovals.Count == CurrentSequenceOrder`. If rule `SequenceOrder`s are non-contiguous (1, 2, 5), this mis-fires. Consider driving off the ordered rule sequence rather than a count.
6. **Stored timestamp vs. reported date diverge.** `StockMovement` sets `Timestamp = DateTime.UtcNow` at construction, but `PostMovementWithinTransactionAsync` returns `request.TransactionDate`. History reads `Timestamp`. The persisted value and the create-response value can differ.
7. **`PostMovementWithinTransactionAsync` is a public footgun.** It issues a `SELECT ... FOR UPDATE` pessimistic lock but opens no transaction of its own — the lock only means something if the caller wrapped it (as `PostMovementAsync`/`LotService` do). Called directly off the interface, the lock is a no-op. Consider making it non-public or asserting an ambient transaction.

**Consistency**
8. **Exception types are mixed.** "Not found" is `NotFoundException` in `LotService`/`MaterialService` but raw `InvalidOperationException` for a missing lot in `StockLedgerService`. Business-rule violations use both `DomainException` (in entities) and `InvalidOperationException` (in services) — e.g., `quantity <= 0` is validated in both `StockMovement.Create` (`DomainException`) and `StockLedgerService` (`InvalidOperationException`). Pick one mapping strategy.
9. **Namespace style split.** File-scoped namespaces in some files (`Material`, `Location`, `StockLedgerService`), block-scoped in others (`SupplierService`, `LotService`, `ILotService`). Plus `Material.cs`'s whole-file over-indentation.
10. **Async naming.** `GetLotCurrentQuantity` and `PostMovementWithinTransactionAsync` are async but the former lacks the `Async` suffix used everywhere else.
11. **Nullable-ref contradiction.** `StockMovement.ReleasedByUser` is `public User? ... = null!;` — nullable type initialized with the null-forgiving operator (`ReceivedByUser` correctly omits it).

**Efficiency (minor)**
12. `GetLotCurrentQuantity` / `GetLotQuantitiesAsync` pull all movement rows into memory and sum client-side. Fine at current scale; a DB-side `GroupBy`/`Sum` scales better and matters once ledgers grow.

---

**Suggested priorities:** (a) the missing role check in `ApproveStepAsync` (#2), (b) soft-delete never filtered on reads (#4), (c) the empty CI workflow + absent API Dockerfile (section 4), then the consistency cleanups.
