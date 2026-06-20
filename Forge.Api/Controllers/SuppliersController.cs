using Forge.Api.DTOs.Suppliers;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ForgeDbContext _context;

    public SuppliersController(ForgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierResponse>>> GetAll()
    {
        var suppliers = await _context.Suppliers.ToListAsync();
        return Ok(suppliers.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierResponse>> GetById(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();
        return Ok(ToResponse(supplier));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResponse>> Create(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            Currency = request.Currency,
            ContactPerson = request.ContactPerson,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, ToResponse(supplier));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();
        supplier.Name = request.Name;
        supplier.Currency = request.Currency;
        supplier.ContactPerson = request.ContactPerson;
        supplier.ContactEmail = request.ContactEmail;
        supplier.ContactPhone = request.ContactPhone;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound();
        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static SupplierResponse ToResponse(Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Currency = supplier.Currency,
        ContactPhone = supplier.ContactPhone,
        ContactPerson = supplier.ContactPerson,
        ContactEmail = supplier.ContactEmail
    };
}

