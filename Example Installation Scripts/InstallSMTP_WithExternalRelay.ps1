# Install SMTP Server Role
try {
    # Todo: Add Windows Scripting and Tools
    Install-WindowsFeature -Name "Web-Scripting-Tools"
    Install-WindowsFeature -Name "Web-Lgcy-Scripting"

    Install-WindowsFeature -Name "SMTP-Server" -IncludeAllSubFeature -IncludeManagementTools

}
catch {
    Write-Error "Failed to install SMTP Server: $_"
    exit 1    
}

try {
    

    iisreset /stop

    # Load the XML file
    [xml]$xml = Get-Content "$env:windir\system32\inetsrv\metabase.xml"

    # Find the IIsSmtpService node
    $node = $xml.configuration.MBProperty.IIsSmtpService

    if ($node) {
        # Set RelayIpList to empty
        $node.RelayIpList = ""

        # Add 127.0.0.1 to RelayIpList
        $node.RelayIpList = "127.0.0.1"

        # Save the updated XML
        $xml.Save("$env:windir\system32\inetsrv\metabase.xml")
        Write-Host "RelayIpList updated successfully."
    } else {
        Write-Host "IIsSmtpService node not found."
    }

    iisreset /start
    net start smtpsvc
}
catch {
    Write-Error "Failed to configure SMTP Server: $_"
    exit 1    
}

# Set the SMTP port to 587
cscript.exe //nologo "$env:SystemDrive\Inetpub\AdminScripts\adsutil.vbs" set "smtpsvc/1/ServerBindings" ":587:"

# Set the Smart Host (replace with your actual smart host)
$smartHost = "smtp.office365.com"  # Change this to your smart host
cscript.exe //nologo "$env:SystemDrive\Inetpub\AdminScripts\adsutil.vbs" set "smtpsvc/1/SmartHost" "$smartHost"
Write-Host "Smart Host set to: $smartHost"

# Set the Fully Qualified Domain Name (replace with your actual FQDN)
$fqdn = "$env:COMPUTERNAME.$env:USERDNSDOMAIN"  # Or set a specific FQDN
if ([string]::IsNullOrEmpty($env:USERDNSDOMAIN)) {
    $fqdn = "$env:COMPUTERNAME.localdomain"  # Fallback if not domain-joined
}
cscript.exe //nologo "$env:SystemDrive\Inetpub\AdminScripts\adsutil.vbs" set "smtpsvc/1/FullyQualifiedDomainName" "$fqdn"
Write-Host "Fully Qualified Domain Name set to: $fqdn"

# Set the outbound SMTP port to 587
cscript.exe //nologo "$env:SystemDrive\Inetpub\AdminScripts\adsutil.vbs" set "smtpsvc/1/RemoteSmtpPort" 587
Write-Host "Outbound SMTP connection port set to: 587"

# Port forward TCP 25 to 587 using netsh
try {
    netsh interface portproxy add v4tov4 listenport=25 listenaddress=0.0.0.0 connectport=587 connectaddress=127.0.0.1
    Write-Host "Port forwarding from 25 to 587 has been configured."
} catch {
    Write-Error "Failed to set up port forwarding from 25 to 587: $_"
    exit 1
}

# Display current configuration
Write-Host "`n=== Current SMTP Configuration ===" -ForegroundColor Cyan
Write-Host "Smart Host: $smartHost"
Write-Host "FQDN: $fqdn"
Write-Host "Inbound Port: 587"
Write-Host "Outbound Port: 587"
Write-Host "Relay IP: 127.0.0.1"