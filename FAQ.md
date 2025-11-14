---
title: Frequently Asked Questions about Managed Instance on Azure App Service
description: Answers to Frequently Asked Questions.
author: apwestgarth
ms.topic: FAQ
ms.date: 11/14/2025
ms.author: anwestg
ms.reviewer:
---
# Frequently Asked Questions about Managed Instance on Azure App Service

Here are answers to common questions asked about Managed Instance on Azure App Service and it's capabilities.

- [What is the version of Windows Server being used?](#what-os-is-running-on-managed-instance-on-azure-app-service-workers)
- [Can I endable additional Windows Roles and Features?](#can-i-enable-additional-windows-roles-and-features)
- [Does Managed Instance on Azure App Service receive regular platform and application stack updates?](#does-managed-instance-on-azure-app-service-receive-regular-platform-and-application-stack-updates)
- [Which Application Stacks and Versions are available pre-installed?](#which-application-stacks-and-versions-are-installed-on-managed-instance-on-azure-app-service-workers)
- [What limitations are there to the installation script?](#what-limitations-are-there-to-the-installation-script)
- [Under what permission level is a installation script executed?](#under-what-permission-level-is-a-installation-script-executed)
- [What role permissions does an operator have when connecting to a worker instance using Bastion?](#what-role-permissions-does-an-operator-have-when-connecting-to-a-worker-instance-using-bastion)
- [What is the addressable memory of a Managed Instance on Azure App Service worker instance?](#what-is-the-addressable-memory-of-a-managed-instance-on-azure-app-service-worker-instance)
- [Which Azure Storage service should I use to upload installation script?](#which-azure-storage-service-should-i-use-to-upload-installation-script)
- [Is there a restriction on naming and format for the installation script?](#is-there-a-restriction-on-naming-and-format-for-the-installation-script)
- [Is there a size limit for the dependencies that I can upload as part of the zip file?](#is-there-a-size-limit-for-the-dependencies-that-i-can-upload-as-part-of-the-zip-file)
- [Does adding or editing Managed Instance on App Service plan adapters restart the plan instances?](#does-adding-or-editing-managed-instance-on-app-service-plan-adapters-restart-the-plan-instances)
- [My Managed Instance on App Service plan has multiple instances can I restart a single instance?](#my-managed-instance-on-app-service-plan-has-multiple-instances-can-i-restart-a-single-instance)
- [My Managed Instance on App Service plan has multiple web applications can I restart a single web application?](#my-managed-instance-on-app-service-plan-has-multiple-web-applications-can-i-restart-a-single-web-application)
- [Can I assign Managed Identity to my web application within the Managed Instance on App Service plan?](#can-i-assign-managed-identity-to-my-web-application-within-the-managed-instance-on-app-service-plan)
- [Is there a limitation on number of adapters that I can create for Managed Instance on App Service plan?](#is-there-a-limitation-on-number-of-adapters-that-i-can-create-for-managed-instance-on-app-service-plan)
- [How can I set the Default Managed Identity for the Plan?](#how-can-i-set-the-default-managed-identity-for-the-plan)

## What OS is running on Managed Instance on Azure App Service workers?

All App Service Plan instances in Managed Instance on Azure App Service are running Windows Server 2022.

## Can I enable additional Windows Roles and Features?

Using a configuration script you can enable additional Windows Roles and Features.  However if a feature you depend on is [removed from a future release of Windows Server](https://learn.microsoft.com/windows-server/get-started/removed-deprecated-features-windows-server?tabs=ws25), it will also become available within Managed Instance on Azure App Service.

## Does Managed Instance on Azure App Service receive regular platform and application stack updates?

Managed Instance on Azure App Service worker instances receive regular platform updates and maintenance.  Application Stack updates are also regularly applied also, how these are limited to the [pre-installed application stacks](#.  Any components or additional application stacks installed by a configuration script need to be regularly maintained and updated by you, the user.

## Which Application Stacks and versions are installed on Managed Instance on Azure App Service workers?

- Microsoft .NET Framework 3.5 and 4.81
- Microsoft .NET Core 8.0 and 10

If you have a requirement for other runtimes, these can be installed using a installation script. Note any runtime versions you deploy will not be maintained and will be required to be regularly updated.

## What limitations are there to the Installation Script

The configuration script can be used to install application dependencies, enable additional roles and features at the OS level and make customisations. Destructive operations are not supported, for example if a configuration script deletes a key OS directory such as Windows\System32 and ultimately disrupts the stability of the underlying instance this will **not** be supported.

## Under what permission level is a Installation script executed?

In order to install and configure dependencies, enable windows features and perform configuration changes to the Windows Operating System, the configuration script is executed with Administrator permissions.

## What role permissions does an operator have when connecting to a worker instance using Bastion?

When connected to a Managed Instance on App Service worker, the operator has Administrator level privileges for the duration of the open session.

## How do I troubleshoot failures with my installation script or registry/storage adapters

Install script logs can be found on the individual App Service Plan workers in the directory c:\InstallScripts\Scriptn\Install.log or alternatively if outputting App Service Console Logs to Azure Monitor and Log Analytics, you can query directly.  
Adapter logs can be found in the root of the machine, alternatively they are logged into App Service Platform Logs.

## What is the addressable memory of a Managed Instance on Azure App Service worker instance?

The addressable memory of a Managed Instance on Azure App Service worker instance varies dependent on the SKU chosen for the App Service Plan.  The table below lists the addressable memory for the Managed Instance on Azure App Service worker instance.  It is important to consider if you have a configuration script which installs additional components, services etc, these will have an impact of memory available for use by your web applications.

| SKU | Cores | Memory (MB) |
|---|---|---|
| P0v4 | 1 | 2048 |
| P1v4 | 2 | 5952 |
| P2v4 | 4 | 13440 |
| P3v4 | 8 | 28672 |
| P1Mv4 | 2 | 13440 |
| P2Mv4 | 4 | 28672 |
| P3Mv4 | 8 | 60160 |
| P4Mv4 | 16 | 121088 |
| P5Mv4 | 32 | 246016 |

## Which Azure Storage service should I use to upload installation script?

Use Azure Storage blob service for uploading installation script and required dependencies. 

## Is there a restriction on naming and format for the installation script?

Yes . You need to name your installation script as install.ps1. Only PowerShell scripts are supported for installation scripts. Ensure to upload installation script and dependencies as a single Zip file. There is no forced naming format for the Zip file.

## Is there a size limit for the dependencies that I can upload as part of the zip file?

No. Currently no size limit is enforced . Please remember that overall size of the dependencies may impact the time taken to provision an instance for Managed Instance on App Service plan.

## Does adding or editing Managed Instance on App Service plan adapters restart the plan instance(s)?

Yes. Adding or editing Managed Instance on App Service plan adapters (installation script\storage\registry) do restart the underlying instance(s) and may impact all the web apps deployed to the plan. Remember that instance restarts removes all changes made via RDP session. Always use installation script to persist dependencies installation or other configuration changes required.

## My Managed Instance on App Service plan has multiple instances can I restart a single instance?

Yes. Navigate to Instances menu and click restart next to the instance name you want to restart. 

## My Managed Instance on App Service plan has multiple web applications can I restart a single web application?

Yes. Navigate to the overview blade for the desired web application and click restart.

## Can I assign Managed Identity to my web application within the Managed Instance on App Service plan?

Yes. You can assign a different Managed identity to a web application within the Managed Instance on App Service plan . Follow the guidance here https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp 

## Is there a limitation on number of adapters that I can create for Managed Instance on App Service plan?

No. There is no upper limit on number of Storage and Registry adapters that you may create for Managed Instance on App Service plan. You can only create a single Installation Script adapter for Managed Instance on App Service plan.  Please remember that number of adapters may impact the time taken to provision an instance for Managed Instance on App Service plan.

## How can I set the Default Managed Identity for the Plan?

If you have created a plan without setting any Managed Identities or during create did not specify the user identity to retrieve the configuration script zip file, you can update the App Service Plan Resource via ARM specifying the default plan identity.  To do this with Azure CLI use the following example, substituting values where placeholders exist:

```
az rest --method put --url "https://management.azure.com/subscriptions/<Your Subscription ID>/resourceGroups/<Your Resource Group Name>/providers/Microsoft.Web/serverFarms/<Your App Service Plan Name>?api-version=2024-11-01" --body "{'location': '<location of your App Service Plan>', 'properties':{'isCustomMode':'true','planDefaultIdentity':{'identityType':'userAssigned','userAssignedIdentityResourceId':'<resource id of your User Assigned Identity'}}, 'sku':{'name':'<SKU name of your App Service Plan, for example P1v4'}}"
```

This command will update the App Service Plan resource and set the Default Plan Identity.  This identity will now be used to retrieve the Configuration Script, read secrets for Storage Mounts and Registry Keys as applicable depending on which features you have configured.
