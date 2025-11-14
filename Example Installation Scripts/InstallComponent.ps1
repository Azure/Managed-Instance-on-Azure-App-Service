
# Install Components, for example Crystal Reports, Control Library, Database Driver
$ComponentInstaller = "myInstallerFileNameGoesHere.msi"
try {
    $Component = Join-Path $PSScriptRoot $ComponentInstaller
    Start-Process $Component -ArgumentList "/q" -Wait -ErrorAction Stop
} catch {
    Write-Error "Failed to install ${ComponentInstaller}: $_"
    exit 1
}