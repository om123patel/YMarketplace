using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class AttributeTemplate : Entity<int>
    {
        public int CategoryId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        // Navigation
        public Category Category { get; private set; } = default!;
        public ICollection<AttributeTemplateItem> Items { get; private set; } = [];

        private AttributeTemplate() { }  // EF Core

        public static AttributeTemplate Create(
            int categoryId, string name, Guid createdBy)
        {
            return new AttributeTemplate
            {
                CategoryId = categoryId,
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void AddItem(
            string attributeName,
            string inputType,
            bool isRequired,
            int sortOrder,
            string? options = null)
        {
            var item = AttributeTemplateItem.Create(
                Id, attributeName, inputType, isRequired, sortOrder, options);
            Items.Add(item);
        }

        public void Update(string name, Guid updatedBy)
        {
            Name = name;
            SetUpdatedBy(updatedBy);
        }

        public void ReplaceItems(
    List<(string attributeName, string inputType,
          bool isRequired, int sortOrder, string? options)> newItems,
    Guid updatedBy)
        {
            Items.Clear();

            foreach (var item in newItems.OrderBy(i => i.sortOrder))
            {
                AddItem(item.attributeName, item.inputType,
                        item.isRequired, item.sortOrder, item.options);
            }

            SetUpdatedBy(updatedBy);
        }

        public void Activate(Guid updatedBy) { IsActive = true; SetUpdatedBy(updatedBy); }
        public void Deactivate(Guid updatedBy) { IsActive = false; SetUpdatedBy(updatedBy); }
    }

}
