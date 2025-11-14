# InstallMSMQ.ps1
# Installs the MSMQ feature and creates a private queue

# Ensure script is running as Administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Please run this script as Administrator."
    exit 1
}

# Install MSMQ feature
Write-Host "Installing MSMQ feature..."
Install-WindowsFeature MSMQ -IncludeAllSubFeature -Restart:$true

# Import MSMQ module if available
if (Get-Module -ListAvailable -Name MSMQ) {
    Import-Module MSMQ
}

# Add Type for MSMQ if not already loaded
if (-not ([System.Messaging.MessageQueue] -is [Type])) {
    Add-Type -AssemblyName System.Messaging
}

# Define queue name
$queueName = ".\private$\MyQueue"

# Check if queue exists
if ([System.Messaging.MessageQueue]::Exists($queueName)) {
    Write-Host "Queue '$queueName' already exists."
} else {
    # Create the queue
    Write-Host "Creating queue '$queueName'..."
    [System.Messaging.MessageQueue]::Create($queueName) | Out-Null
    Write-Host "Queue created successfully."
}