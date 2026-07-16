using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Forge.Tests;

public class BillOfMaterialsTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public BillOfMaterialsTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<Material> SeedMaterialAsync(string sku)
    {
        var material = Material.Create(sku, $"Material {sku}", MaterialType.Raw, null, "kg");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();
        return material;
    }

    // ---------- Domain guards (no DB) ----------

    [Fact]
    public void Create_StartsAsDraftAndActive()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);

        Assert.Equal(BillOfMaterialStatus.Draft, bom.BillOfMaterialStatus);
        Assert.True(bom.IsActive);
        Assert.Empty(bom.Lines);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidOutputMaterialId_Throws(int outputMaterialId)
    {
        Assert.Throws<DomainException>(() => BillOfMaterials.Create(outputMaterialId));
    }

    [Fact]
    public void AddLine_WithOutputMaterial_ThrowsSelfReference()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);

        var ex = Assert.Throws<DomainException>(
            () => bom.AddLine(materialId: 1, quantity: 5, unitOfMeasure: "kg"));

        Assert.Contains("Output material", ex.Message);
    }

    [Fact]
    public void UpdateLine_ToOutputMaterial_ThrowsSelfReference()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");

        var ex = Assert.Throws<DomainException>(
            () => bom.UpdateLine(bom.Lines[0].Id, materialId: 1, quantity: 5, unitOfMeasure: "kg"));

        Assert.Contains("Output material", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2.5)]
    public void LineCreate_WithInvalidQuantity_Throws(decimal quantity)
    {
        Assert.Throws<DomainException>(
            () => BillOfMaterialsLine.Create(materialId: 2, quantity, unitOfMeasure: "kg"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LineCreate_WithEmptyUnitOfMeasure_Throws(string unitOfMeasure)
    {
        Assert.Throws<DomainException>(
            () => BillOfMaterialsLine.Create(materialId: 2, quantity: 5, unitOfMeasure));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LineCreate_WithInvalidMaterialId_Throws(int materialId)
    {
        Assert.Throws<DomainException>(
            () => BillOfMaterialsLine.Create(materialId, quantity: 5, unitOfMeasure: "kg"));
    }

    [Fact]
    public void Approve_WithNoLines_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);

        Assert.Throws<DomainException>(() => bom.Approve());
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Approve();

        Assert.Throws<DomainException>(() => bom.Approve());
    }

    [Fact]
    public void Approve_WhenRejected_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Reject();

        Assert.Throws<DomainException>(() => bom.Approve());
    }

    [Fact]
    public void Reject_WhenApproved_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Approve();

        Assert.Throws<DomainException>(() => bom.Reject());
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.Reject();

        Assert.Throws<DomainException>(() => bom.Reject());
    }

    [Fact]
    public void AddLine_OnApprovedBom_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Approve();

        Assert.Throws<DomainException>(
            () => bom.AddLine(materialId: 3, quantity: 1, unitOfMeasure: "pc"));
    }

    [Fact]
    public void UpdateLine_OnApprovedBom_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Approve();

        Assert.Throws<DomainException>(
            () => bom.UpdateLine(bom.Lines[0].Id, materialId: 2, quantity: 10, unitOfMeasure: "kg"));
    }

    [Fact]
    public void RemoveLine_OnApprovedBom_Throws()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Approve();

        Assert.Throws<DomainException>(() => bom.RemoveLine(bom.Lines[0].Id));
    }

    [Fact]
    public void AddLine_OnRejectedBom_FlipsToDraft()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Reject();

        bom.AddLine(materialId: 3, quantity: 1, unitOfMeasure: "pc");

        Assert.Equal(BillOfMaterialStatus.Draft, bom.BillOfMaterialStatus);
    }

    [Fact]
    public void UpdateLine_OnRejectedBom_FlipsToDraft()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Reject();

        bom.UpdateLine(bom.Lines[0].Id, materialId: 3, quantity: 10, unitOfMeasure: "pc");

        Assert.Equal(BillOfMaterialStatus.Draft, bom.BillOfMaterialStatus);
    }

    [Fact]
    public void RemoveLine_OnRejectedBom_FlipsToDraft()
    {
        var bom = BillOfMaterials.Create(outputMaterialId: 1);
        bom.AddLine(materialId: 2, quantity: 5, unitOfMeasure: "kg");
        bom.Reject();

        bom.RemoveLine(bom.Lines[0].Id);

        Assert.Equal(BillOfMaterialStatus.Draft, bom.BillOfMaterialStatus);
    }

    // ---------- Persistence (real DB) ----------

    [Fact]
    public async Task CreateApproveAndPersist_RoundTripsCorrectly()
    {
        var output = await SeedMaterialAsync("BOM-OUT-RoundTrip");
        var component = await SeedMaterialAsync("BOM-COMP-RoundTrip");

        var bom = BillOfMaterials.Create(output.Id);
        bom.AddLine(component.Id, quantity: 2.5m, unitOfMeasure: "kg");
        bom.Approve();

        _fixture.DbContext.BillOfMaterials.Add(bom);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.ChangeTracker.Clear();

        var saved = await _fixture.DbContext.BillOfMaterials
            .Include(b => b.Lines)
            .SingleAsync(b => b.Id == bom.Id);

        Assert.Equal(BillOfMaterialStatus.Approved, saved.BillOfMaterialStatus);
        Assert.Single(saved.Lines);
        Assert.Equal(component.Id, saved.Lines[0].MaterialId);
        Assert.Equal(2.5m, saved.Lines[0].Quantity);
    }

    [Fact]
    public async Task StatusColumn_StoresEnumAsString()
    {
        var output = await SeedMaterialAsync("BOM-OUT-StatusString");
        var component = await SeedMaterialAsync("BOM-COMP-StatusString");

        var bom = BillOfMaterials.Create(output.Id);
        bom.AddLine(component.Id, quantity: 1, unitOfMeasure: "kg");
        bom.Approve();

        _fixture.DbContext.BillOfMaterials.Add(bom);
        await _fixture.DbContext.SaveChangesAsync();

        var rawStatus = await _fixture.DbContext.Database
            .SqlQuery<string>($"""SELECT "BillOfMaterialStatus" AS "Value" FROM "BillOfMaterials" WHERE "Id" = {bom.Id}""")
            .SingleAsync();

        Assert.Equal("Approved", rawStatus);
    }

    [Fact]
    public async Task DeactivatedBom_IsFilteredFromQueries()
    {
        var output = await SeedMaterialAsync("BOM-OUT-Deactivated");

        var bom = BillOfMaterials.Create(output.Id);
        bom.Deactivate();
        _fixture.DbContext.BillOfMaterials.Add(bom);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.ChangeTracker.Clear();

        var found = await _fixture.DbContext.BillOfMaterials
            .FirstOrDefaultAsync(b => b.Id == bom.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task DeletingBom_CascadesToLines()
    {
        var output = await SeedMaterialAsync("BOM-OUT-Cascade");
        var component = await SeedMaterialAsync("BOM-COMP-Cascade");

        var bom = BillOfMaterials.Create(output.Id);
        bom.AddLine(component.Id, quantity: 3, unitOfMeasure: "kg");
        _fixture.DbContext.BillOfMaterials.Add(bom);
        await _fixture.DbContext.SaveChangesAsync();

        var lineId = bom.Lines[0].Id;
        Assert.NotEqual(0, lineId);

        _fixture.DbContext.BillOfMaterials.Remove(bom);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.ChangeTracker.Clear();

        var orphanedLines = await _fixture.DbContext.BillOfMaterialsLines
            .Where(l => l.BillOfMaterialsId == bom.Id)
            .ToListAsync();

        Assert.Empty(orphanedLines);
    }

    [Fact]
    public async Task AddingLinesToUnsavedBom_PersistsWithFixedUpForeignKey()
    {
        var output = await SeedMaterialAsync("BOM-OUT-Fixup");
        var component = await SeedMaterialAsync("BOM-COMP-Fixup");

        var bom = BillOfMaterials.Create(output.Id);
        // Parent has no Id yet, so the line is created with BillOfMaterialsId == 0.
        bom.AddLine(component.Id, quantity: 4, unitOfMeasure: "pc");
        Assert.Equal(0, bom.Lines[0].BillOfMaterialsId);

        _fixture.DbContext.BillOfMaterials.Add(bom);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.ChangeTracker.Clear();

        var saved = await _fixture.DbContext.BillOfMaterials
            .Include(b => b.Lines)
            .SingleAsync(b => b.Id == bom.Id);

        Assert.Single(saved.Lines);
        Assert.Equal(bom.Id, saved.Lines[0].BillOfMaterialsId);
    }
}
