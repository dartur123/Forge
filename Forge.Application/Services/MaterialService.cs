using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services;

public class MaterialService : IMaterialService
{
    private readonly ForgeDbContext _context;

    public MaterialService(ForgeDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialResult> CreateMaterialAsync(PostMaterialRequest request)
    {
        var material = Material.Create(request.Sku, request.Name, request.Type, request.Description, request.UnitOfMeasure);
        _context.Materials.Add(material);
        await _context.SaveChangesAsync();
        return await GetMaterialAsync(material.Id);
    }

    public async Task DeactivateMaterialAsync(int materialId)
    {
        var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if(material == null)
            throw new NotFoundException("Material not found");

        material.Deactivate();
        await _context.SaveChangesAsync();
    }

    public async Task<List<MaterialResult>> GetAllMaterialsAsync()
    {
        var materials = await _context.Materials.ToListAsync();

        return materials.Select(MaterialResult.FromEntity).ToList();
    }

    public async Task<MaterialResult> GetMaterialAsync(int materialId)
    {
        var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if (material == null)
            throw new NotFoundException("Material not found");

        return MaterialResult.FromEntity(material);
    }

    public async Task<List<MaterialResult>> GetMaterialByTypeAsync(MaterialType materialType)
    {
        var materials = await _context.Materials.Where(m => m.Type == materialType).ToListAsync();
        return materials.Select(MaterialResult.FromEntity).ToList();
    }

    public async Task<MaterialResult> UpdateMaterialAsync(int id, PostMaterialRequest request)
    {
        var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (material == null)
            throw new NotFoundException("Material not found");

        material.Update(request.Sku, request.Name, request.Type, request.Description, request.UnitOfMeasure);
        await _context.SaveChangesAsync();
        return await GetMaterialAsync(material.Id);
    }
}

