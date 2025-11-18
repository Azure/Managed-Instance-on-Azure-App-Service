---
title: Directory of Example Installation Scripts for different scenarios
description: See examples of Installation Scripts which setup common dependencies.
author: anwestg
ms.topic: getting-started
ms.date: 11/17/2025
ms.author: anwestg
ms.reviewer:
---

# Example Installation Scripts

This directory contains a number of example Installation Scripts, each demonstrate how to setup key dependencies.  Examples inclide how to set up certain Windows Server Roles or Features, how to instal common third party components, install libraries to the Global Assembly Cache (GAC) and install Windows Services to support your applications.

## Examples

| Script Filename | Scenario |
|----------|------|
|[InstallSMTP.ps1](InstallSMTP.ps1)| Script which installs the SMTP Service feature, management tools and supporting scripts.  This script also setups relay from localhost and sets port forwarding up for port 25 to 587 |
|[InstallSMTP_WithExternalRelay.ps1)](InstallSMTP_WithExternalRelay.ps1)| Script which installs the SMTP Service feature, management tools and supporting scripts.  Also takes in details of the Smart Host and Valid Domain Name.  The Smart Host could be a service such as Azure Communication Service or a third party provider and the domain name is the valid domain name for mail distribution.  Finally this script allso sets port forwarding for port 25 to 587 |
|[InstallMSMQ.ps1](InstallMSMQ.ps1)| Script which installs MSMQ Role and creates a queue ready to receive messages |
|[InstallComponent.ps1](InstallComponent.ps1)| Script which demonstrates how to run component installers such as Crystal Reports, Drivers, MSIs, Exes. |
|[InstallFont.ps1](InstallFont.ps1)| Script which installs provided font. |