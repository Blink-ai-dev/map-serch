using System.Text.Json;

namespace LmpLink.MAUI.Services.Implementation;

/// <summary>
/// Naver Maps JS Interop service implementation.
/// Wraps EvaluateJavaScriptAsync calls for map operations.
/// </summary>
public class MapService : IMapService
{
    private WebView? _webView;
    private bool _isMapInitialized;

    /// <summary>
    /// Set the WebView reference for JS Interop.
    /// Must be called before any map operations.
    /// </summary>
    public void SetWebView(WebView webView)
    {
        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
    }

    /// <inheritdoc />
    public async Task InitMapAsync(double lat = 37.6688, double lng = 127.0471, int zoom = 13)
    {
        ThrowIfWebViewNotSet();

        try
        {
            var script = $"window.initMap({lat}, {lng}, {zoom});";
            await EvaluateJavaScriptAsync(script);
            _isMapInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize map: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task AddMarkerAsync(Person person)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var type = person.Type == PersonType.User ? "user" : "assistant";
            var name = EscapeJavaScript(person.Name);
            var address = EscapeJavaScript(person.Address ?? "");
            var script = $"window.addMarker({person.Id}, '{type}', {person.Latitude}, {person.Longitude}, '{name}', '{address}');";
            await EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add marker for {person.Name}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task AddMarkersAsync(IEnumerable<Person> persons)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        foreach (var person in persons)
        {
            await AddMarkerAsync(person);
        }
    }

    /// <inheritdoc />
    public async Task DrawCircleAsync(double lat, double lng, double radiusKm)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var script = $"window.drawCircle({lat}, {lng}, {radiusKm});";
            await EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to draw circle: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ClearCircleAsync()
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            await EvaluateJavaScriptAsync("window.clearCircle();");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to clear circle: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task SetMarkerVisibleAsync(int personId, bool visible)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var visibleStr = visible ? "true" : "false";
            var script = $"window.setMarkerVisible({personId}, {visibleStr});";
            await EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set marker visibility for ID {personId}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ShowAllMarkersAsync()
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            await EvaluateJavaScriptAsync("window.showAllMarkers();");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to show all markers: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ClearAllMarkersAsync()
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            await EvaluateJavaScriptAsync("window.clearAllMarkers();");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to clear all markers: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(double lat, double lng)> GetMapCenterAsync()
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var result = await EvaluateJavaScriptAsync("window.getMapCenter();");
            
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException("Empty response from getMapCenter");
            }

            var json = JsonSerializer.Deserialize<MapCenterResult>(result);
            if (json == null)
            {
                throw new InvalidOperationException("Failed to parse getMapCenter response");
            }

            return (json.Lat, json.Lng);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get map center: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task SetMapCenterAsync(double lat, double lng)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var script = $"window.setMapCenter({lat}, {lng});";
            await EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set map center: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task FocusMarkerAsync(int personId)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var script = $"window.focusMarker({personId});";
            await EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to focus marker {personId}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(int users, int assistants)> GetVisibleMarkerCountAsync()
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var result = await EvaluateJavaScriptAsync("window.getVisibleMarkerCount();");
            
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException("Empty response from getVisibleMarkerCount");
            }

            // WebView returns escaped JSON string, need to unescape
            var unescaped = System.Text.RegularExpressions.Regex.Unescape(result);
            
            // Remove surrounding quotes if present
            if (unescaped.StartsWith("\"") && unescaped.EndsWith("\""))
            {
                unescaped = unescaped.Substring(1, unescaped.Length - 2);
            }

            var json = JsonSerializer.Deserialize<MarkerCountResult>(unescaped);
            if (json == null)
            {
                throw new InvalidOperationException("Failed to parse getVisibleMarkerCount response");
            }

            return (json.Users, json.Assistants);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get visible marker count: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ApplyMapSettingsAsync(
        string userMarkerColor, 
        string assistantMarkerColor, 
        string markerSize,
        int userBorderWidth,
        int assistantBorderWidth,
        string userShape,
        string assistantShape,
        string highlightColor,
        int highlightBorderWidth,
        double highlightSizeMultiplier,
        string circleColor, 
        double circleOpacity, 
        int circleBorderWidth,
        double uiScale)
    {
        ThrowIfWebViewNotSet();
        ThrowIfMapNotInitialized();

        try
        {
            var script = $"window.applyMapSettings('{userMarkerColor}', '{assistantMarkerColor}', '{markerSize}', {userBorderWidth}, {assistantBorderWidth}, '{userShape}', '{assistantShape}', '{highlightColor}', {highlightBorderWidth}, {highlightSizeMultiplier}, '{circleColor}', {circleOpacity}, {circleBorderWidth}, {uiScale});";
            await EvaluateJavaScriptAsync(script);
            MauiProgram.Log($"[MapService] Map settings applied: UserMarker={userMarkerColor}, Size={markerSize}, Circle={circleColor}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to apply map settings: {ex.Message}", ex);
        }
    }

    // ===== Private Helpers =====

    private async Task<string> EvaluateJavaScriptAsync(string script)
    {
        if (_webView == null)
        {
            throw new InvalidOperationException("WebView not set");
        }

        return await _webView.EvaluateJavaScriptAsync(script);
    }

    private void ThrowIfWebViewNotSet()
    {
        if (_webView == null)
        {
            throw new InvalidOperationException("WebView not set. Call SetWebView() first.");
        }
    }

    private void ThrowIfMapNotInitialized()
    {
        if (!_isMapInitialized)
        {
            throw new InvalidOperationException("Map not initialized. Call InitMapAsync() first.");
        }
    }

    private static string EscapeJavaScript(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    // ===== DTO for JSON parsing =====

    private class MapCenterResult
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private class MarkerCountResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("users")]
        public int Users { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("assistants")]
        public int Assistants { get; set; }
    }
}
