#!/usr/bin/env pwsh
[CmdletBinding()]
param(
	[Parameter(Mandatory = $false)]
	[string]
	$Verb = "run"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Join-Path "$PSScriptRoot" ".." | Resolve-Path
$Cv2LDPath = Join-Path -Path $ProjectRoot -ChildPath "/bin/Debug/net6.0/runtimes/ubuntu.18.04-x64/native" | Resolve-Path

$env:LD_LIBRARY_PATH = "$Cv2LDPath"  

& dotnet $Verb
