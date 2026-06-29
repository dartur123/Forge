using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ForgeDbContext _context;

        public SupplierService(ForgeDbContext context)
        {
            _context = context;
        }

        public async Task<SupplierResult> CreateSupplierAsync(PostSupplierRequest request)
        {
            var supplier = Supplier.Create(request.Name, request.Currency, request.ContactPerson, request.ContactEmail, request.ContactPhone);
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return await GetSupplierAsync(supplier.Id);
        }

        public async Task DeactivateSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if(supplier==null)
                throw new NotFoundException("Supplier not found");

            supplier.Deactivate();
            await _context.SaveChangesAsync();
        }

        public async Task<List<SupplierResult>> GetAllSuppliersAsync()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return suppliers.Select(SupplierResult.FromEntity).ToList();
        }

        public async Task<SupplierResult> GetSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            return SupplierResult.FromEntity(supplier);
        }

        public async Task<SupplierResult> UpdateSupplierAsync(int id, PostSupplierRequest request)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            supplier.Update(request.Name, request.Currency, request.ContactPerson, request.ContactEmail, request.ContactPhone);
            await _context.SaveChangesAsync();
            return await GetSupplierAsync(id);
        }
    }
}
