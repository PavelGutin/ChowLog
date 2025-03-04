$directory = "ChowLog.Blazor\ChowLog.Blazor\Components\Account"
$files = Get-ChildItem -Path $directory -Recurse -Filter "*.razor"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $updatedContent = $content -replace "ApplicationUser", "IdentityUser"
    Set-Content -Path $file.FullName -Value $updatedContent
}
