namespace LmpLink.MAUI.Services.Interfaces;

/// <summary>
/// Naver Maps JS Interop service for WebView communication.
/// Handles map initialization, marker rendering, and circle drawing.
/// </summary>
public interface IMapService
{
    /// <summary>
    /// Set the WebView reference for JS Interop.
    /// Must be called before any map operations.
    /// </summary>
    /// <param name="webView">WebView instance</param>
    void SetWebView(WebView webView);

    /// <summary>
    /// Initialize the Naver Map with center coordinates and zoom level.
    /// </summary>
    /// <param name="lat">Latitude (default: 37.6688 for Dobong-gu Office)</param>
    /// <param name="lng">Longitude (default: 127.0471 for Dobong-gu Office)</param>
    /// <param name="zoom">Zoom level (default: 13)</param>
    Task InitMapAsync(double lat = 37.6688, double lng = 127.0471, int zoom = 13);

    /// <summary>
    /// Add a marker to the map.
    /// </summary>
    /// <param name="person">Person object containing location and metadata</param>
    Task AddMarkerAsync(Person person);

    /// <summary>
    /// Add multiple markers in batch.
    /// </summary>
    /// <param name="persons">List of persons to add as markers</param>
    Task AddMarkersAsync(IEnumerable<Person> persons);

    /// <summary>
    /// Draw a circle (radius overlay) on the map.
    /// </summary>
    /// <param name="lat">Center latitude</param>
    /// <param name="lng">Center longitude</param>
    /// <param name="radiusKm">Radius in kilometers</param>
    Task DrawCircleAsync(double lat, double lng, double radiusKm);

    /// <summary>
    /// Clear the circle overlay from the map.
    /// </summary>
    Task ClearCircleAsync();

    /// <summary>
    /// Set marker visibility by person ID.
    /// </summary>
    /// <param name="personId">Person ID (1-based)</param>
    /// <param name="visible">True to show, false to hide</param>
    Task SetMarkerVisibleAsync(int personId, bool visible);

    /// <summary>
    /// Show all markers on the map.
    /// </summary>
    Task ShowAllMarkersAsync();

    /// <summary>
    /// Clear all markers from the map.
    /// </summary>
    Task ClearAllMarkersAsync();

    /// <summary>
    /// Get current map center coordinates.
    /// </summary>
    /// <returns>Tuple of (lat, lng)</returns>
    Task<(double lat, double lng)> GetMapCenterAsync();

    /// <summary>
    /// Set map center to specific coordinates.
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lng">Longitude</param>
    Task SetMapCenterAsync(double lat, double lng);

    /// <summary>
    /// Focus on a marker (center map + zoom + open InfoWindow).
    /// </summary>
    /// <param name="personId">Person ID</param>
    Task FocusMarkerAsync(int personId);

    /// <summary>
    /// Get count of visible markers on the map (actually displayed).
    /// </summary>
    /// <returns>Tuple of (userCount, assistantCount)</returns>
    Task<(int users, int assistants)> GetVisibleMarkerCountAsync();

    /// <summary>
    /// Apply map styling settings (marker colors, circle style).
    /// </summary>
    /// <param name="userMarkerColor">User marker color (hex)</param>
    /// <param name="assistantMarkerColor">Assistant marker color (hex)</param>
    /// <param name="markerSize">Marker size (Small/Medium/Large)</param>
    /// <param name="userBorderWidth">User marker border width (px)</param>
    /// <param name="assistantBorderWidth">Assistant marker border width (px)</param>
    /// <param name="userShape">User marker shape (Circle/Square/Diamond)</param>
    /// <param name="assistantShape">Assistant marker shape (Circle/Square/Diamond)</param>
    /// <param name="highlightColor">Highlight border color (hex)</param>
    /// <param name="highlightBorderWidth">Highlight border width (px)</param>
    /// <param name="highlightSizeMultiplier">Highlight size multiplier (1.5-3.0x)</param>
    /// <param name="circleColor">Circle fill/stroke color (hex)</param>
    /// <param name="circleOpacity">Circle fill opacity (0.0-1.0)</param>
    /// <param name="circleBorderWidth">Circle border width (px)</param>
    /// <param name="uiScale">UI scale factor (0.8-1.5)</param>
    Task ApplyMapSettingsAsync(
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
        double uiScale);
}
