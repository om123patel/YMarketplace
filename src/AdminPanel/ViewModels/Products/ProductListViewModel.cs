using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Grid;

namespace AdminPanel.ViewModels.Products
{
    public class ProductListViewModel : IListViewModel
    {
        // ── Rows ─────────────────────────────────────────────────────────────
        public List<ProductListItem> Items { get; set; } = [];

        // ── IListViewModel: Pagination ────────────────────────────────────────
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // ── IListViewModel: Sort ──────────────────────────────────────────────
        public string SortBy { get; set; } = "createdat";
        public string SortDirection { get; set; } = "desc";

        // ── IListViewModel: Filters ───────────────────────────────────────────
        // public set (not private) so the interface contract is satisfied
        // across all compiler/runtime combinations.
        public TableFilterModel Filters { get; set; } = new();

        // ── IListViewModel: RouteData ─────────────────────────────────────────
        public Dictionary<string, string?> RouteData => new()
        {
            ["search"] = Search,
            ["status"] = Status,
            ["categoryId"] = CategoryId?.ToString(),
            ["brandId"] = BrandId?.ToString(),
            ["creatorType"] = CreatorType,
            ["flags"] = Flags,
            ["createdFrom"] = CreatedFrom,
            ["createdTo"] = CreatedTo,
            ["priceOp"] = PriceOp,
            ["priceMin"] = PriceMin,
            ["priceMax"] = PriceMax,
            ["sortBy"] = SortBy,
            ["sortDirection"] = SortDirection
        };

        // ── Filter values (bound from GET query string) ───────────────────────
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? CreatorType { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? Flags { get; set; }
        public string? CreatedFrom { get; set; }
        public string? CreatedTo { get; set; }
        public string? PriceOp { get; set; }
        public string? PriceMin { get; set; }
        public string? PriceMax { get; set; }

        // ── Dropdown data (set by controller before BuildFilters) ─────────────
        public List<FilterOption> Categories { get; set; } = [];
        public List<FilterOption> Brands { get; set; } = [];

        // ── BuildFilters() ────────────────────────────────────────────────────
        public void BuildFilters()
        {
            Filters = new TableFilterModel
            {
                FormId = "productFilterForm",
                ClearAllUrl = "/products",
                Columns =
                [
                    new()
                    {
                        Label        = "Name / SKU",
                        ParamName    = "search",
                        Type         = ColumnFilterType.Text,
                        Placeholder  = "Search name or SKU…",
                        CurrentValue = Search
                    },
                    new()
                    {
                        Label        = "Status",
                        ParamName    = "status",
                        Type         = ColumnFilterType.Select,
                        Placeholder  = "All statuses",
                        CurrentValue = Status,
                        Options =
                        [
                            new("Draft",           "Draft",            "dot-sky"),
                            new("PendingApproval", "Pending Approval", "dot-amber"),
                            new("Active",          "Active",           "dot-emerald"),
                            new("Rejected",        "Rejected",         "dot-rose"),
                            new("Archived",        "Archived",         "dot-neutral")
                        ]
                    },
                    new()
                    {
                        Label        = "Creator",
                        ParamName    = "creatorType",
                        Type         = ColumnFilterType.Radio,
                        CurrentValue = CreatorType,
                        Options =
                        [
                            new("",       "All"),
                            new("Admin",  "Admin"),
                            new("Seller", "Seller")
                        ]
                    },
                    new()
                    {
                        Label        = "Category",
                        ParamName    = "categoryId",
                        Type         = ColumnFilterType.Select,
                        Placeholder  = "All categories",
                        CurrentValue = CategoryId?.ToString(),
                        Options      = Categories
                            .Select(c => new FilterOption(c.Value, c.Label))
                            .ToList()
                    },
                    new()
                    {
                        Label        = "Brand",
                        ParamName    = "brandId",
                        Type         = ColumnFilterType.Select,
                        Placeholder  = "All brands",
                        CurrentValue = BrandId?.ToString(),
                        Options      = Brands
                            .Select(b => new FilterOption(b.Value, b.Label))
                            .ToList()
                    },
                    new()
                    {
                        Label        = "Flags",
                        ParamName    = "flags",
                        Type         = ColumnFilterType.Checkbox,
                        CurrentValue = Flags,
                        Options =
                        [
                            new("featured", "Featured only"),
                            new("digital",  "Digital only"),
                            new("active",   "Active only")
                        ]
                    },
                    new()
                    {
                        Label           = "Created Date",
                        ParamName       = "createdFrom",
                        Type            = ColumnFilterType.DateRange,
                        CurrentValue    = CreatedFrom,
                        DateToParamName = "createdTo",
                        CurrentValueTo  = CreatedTo
                    },
                    new()
                    {
                        Label             = "Price",
                        ParamName         = "priceMin",
                        Type              = ColumnFilterType.NumberComparison,
                        CurrentValue      = PriceMin,
                        OpParamName       = "priceOp",
                        CurrentOp         = PriceOp ?? "gte",
                        NumberToParamName = "priceMax",
                        CurrentValueMax   = PriceMax,
                        Unit              = "₹",
                        Placeholder       = "0"
                    }
                ]
            };
        }
    }
}
