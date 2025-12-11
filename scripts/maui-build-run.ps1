# Stop any running instance to avoid file lock during build
taskkill /IM LmpLink.MAUI.exe /F 2>$null | Out-Null

Set-Location -LiteralPath "C:\ai\projects\map-demo"

# Build
dotnet build "src\LmpLink.MAUI\LmpLink.MAUI.csproj" -f net10.0-windows10.0.19041.0
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Run
Set-Location -LiteralPath "C:\ai\projects\map-demo\src\LmpLink.MAUI\bin\Debug\net10.0-windows10.0.19041.0\win-x64"
& ".\LmpLink.MAUI.exe"

