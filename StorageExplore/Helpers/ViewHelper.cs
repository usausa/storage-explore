namespace StorageExplore.Helpers;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Shared view helpers for Razor templates.
/// </summary>
public static class ViewHelper
{
    public static MarkupString SortIndicator(bool isActive, bool descending) =>
        isActive
            ? new MarkupString(descending
                ? "<i class=\"bi bi-chevron-down\" style=\"font-size:0.7rem\"></i>"
                : "<i class=\"bi bi-chevron-up\" style=\"font-size:0.7rem\"></i>")
            : new MarkupString(string.Empty);
}
