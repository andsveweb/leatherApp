using MudBlazor;

namespace leatherApp.Data;

public class BreadcrumbService
{
    public List<BreadcrumbItem>? Custom { get; private set; }

    public event Action? OnChange;

    public void Set(List<BreadcrumbItem> items)
    {
        Custom = items;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        Custom = null;
        OnChange?.Invoke();
    }
}