namespace AdminPanel.ViewModels.Common
{
    public class PaginationModel
    {
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public string Action { get; }
        public string Controller { get; }
        public Dictionary<string, string?> RouteData { get; }

        public PaginationModel(IListViewModel vm, string action, string controller)
        {
            Page = vm.Page;
            PageSize = vm.PageSize;
            TotalCount = vm.TotalCount;
            TotalPages = vm.TotalPages;
            Action = action;
            Controller = controller;
            RouteData = vm.RouteData;
        }
    }
}
