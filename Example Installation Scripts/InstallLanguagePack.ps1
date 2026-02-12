## Installation script example to install a language pack for .NET Framework 4.8.1 on Windows Server 2022.
## References: https://support.microsoft.com/topic/microsoft-net-framework-4-8-1-language-pack-on-windows-10-version-21h2-windows-10-version-22h2-windows-11-version-21h2-windows-server-2022-desktop-azure-editions-azure-stack-21h2-and-azure-stack-22h2-kb5027937-f75ac9c5-9b1c-428f-91e3-b62a92ba42ca

# Ensure script is running as Administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Please run this script as Administrator."
    exit 1
}
## Download the required language pack from the Microsoft Update Catalog and specify the path to the downloaded .msu file
$msuFilePath = "LanguagePack.msu"
# Install the language pack using DISM
Write-Host "Installing language pack from $msuFilePath..."  
$installResult = Start-Process -FilePath "dism.exe" -ArgumentList "/Online /Add-Package /PackagePath:$msuFilePath" -Wait -PassThru
if ($installResult.ExitCode -eq 0) {    
    Write-Host "Language pack installed successfully."    
} else {    
    Write-Error "Failed to install language pack. Exit code: $($installResult.ExitCode)"    
}

