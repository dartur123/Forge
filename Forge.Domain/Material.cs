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