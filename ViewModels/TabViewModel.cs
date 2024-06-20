using Microsoft.AspNetCore.Html;

namespace Workflows.ViewModels
{
    public class TabViewModel
    {
        public List<TabItem>? Tabs { get; set; }

        public class TabItem
        {
            public string? Id { get; set; }
            public string? Title { get; set; }
            public string? PartialViewName { get; set; }
            public object? Model { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
