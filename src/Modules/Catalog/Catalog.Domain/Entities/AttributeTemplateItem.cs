using Shared.Domain.Abstractions;

namespace Catalog.Domain.Entities
{
    public class AttributeTemplateItem : Entity<int>
    {
        public int TemplateId { get; private set; }
        public string AttributeName { get; private set; } = string.Empty;
        public string InputType { get; private set; } = string.Empty;
        // Text | Select | MultiSelect | Number | Boolean
        public string? Options { get; private set; }  // JSON array for Select types
        public bool IsRequired { get; private set; }
        public int SortOrder { get; private set; }

        private AttributeTemplateItem() { }  // EF Core

        public static AttributeTemplateItem Create(
            int templateId,
            string attributeName,
            string inputType,
            bool isRequired,
            int sortOrder,
            string? options = null)
        {
            return new AttributeTemplateItem
            {
                TemplateId = templateId,
                AttributeName = attributeName,
                InputType = inputType,
                IsRequired = isRequired,
                SortOrder = sortOrder,
                Options = options,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };
        }

        public void Update(
            string attributeName,
            string inputType,
            bool isRequired,
            int sortOrder,
            string? options,
            Guid updatedBy)
        {
            AttributeName = attributeName;
            InputType = inputType;
            IsRequired = isRequired;
            SortOrder = sortOrder;
            Options = options;
            SetUpdatedBy(updatedBy);
        }
    }

}
