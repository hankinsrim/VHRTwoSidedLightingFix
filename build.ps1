<#
.SYNOPSIS
    Compile the vhrfix tool, patch Vanilla Hair Remake SMP's meshes and stage the result in .\dist.
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\build.ps1 -VHR "C:\Modding\MO2\mods\Vanilla hair remake SMP"
    powershell -ExecutionPolicy Bypass -File .\build.ps1 -VHR "C:\Modding\MO2\mods\Vanilla hair remake SMP" -Zip
#>
param(
    [Parameter(Mandatory)] [string] $VHR,
    [switch] $Zip
)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$version = '1.0'
$vhrVersion = '1.0.3'

$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"
$vs = & "$installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
if (-not $vs) { throw 'Visual Studio with MSBuild was not found.' }
$csc = "$vs\MSBuild\Current\Bin\Roslyn\csc.exe"
if (-not (Test-Path $csc)) { throw "csc.exe was not found at $csc" }

New-Item -ItemType Directory -Force build | Out-Null
& $csc -nologo -langversion:latest -out:build\vhrfix.exe tool\*.cs
if ($LASTEXITCODE) { throw 'Compiling vhrfix failed' }

Remove-Item dist -Recurse -Force -ErrorAction SilentlyContinue
& build\vhrfix.exe $VHR dist
if ($LASTEXITCODE) { throw 'vhrfix failed' }
Write-Host "Mod folder ready: $PSScriptRoot\dist"

if ($Zip) {
    # MO2 reads the mod version from fomod\info.xml, otherwise it falls back to the file date.
    $stage = "$PSScriptRoot\build\package"
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force "$stage\fomod" | Out-Null
    Copy-Item dist\* $stage -Recurse
    Copy-Item fomod\ModuleConfig.xml "$stage\fomod"
    Copy-Item LICENSE "$stage\LICENSE.txt"
    @"
VHR Two-Sided Lighting Fix $version, for Vanilla Hair Remake SMP $vhrVersion
Copyright (c) 2026 hankinsrim. The patch tool is released under the MIT License (LICENSE.txt).

The meshes are Vanilla Hair Remake SMP's by jg1
(https://www.nexusmods.com/skyrimspecialedition/mods/63979), with only their normals and
tangents changed. Load this mod after Vanilla Hair Remake SMP.

Source code: https://github.com/hankinsrim/VHRTwoSidedLightingFix
"@ | Out-File "$stage\README.txt" -Encoding utf8
    @"
<?xml version="1.0" encoding="UTF-8"?>
<fomod>
	<Name>VHR Two-Sided Lighting Fix</Name>
	<Author>hankinsrim</Author>
	<Version>$version</Version>
	<Website>https://github.com/hankinsrim/VHRTwoSidedLightingFix</Website>
	<Description>Fixes the dark inner layers of Vanilla Hair Remake SMP $vhrVersion hair.</Description>
</fomod>
"@ | Out-File "$stage\fomod\info.xml" -Encoding utf8
    $zipPath = "$PSScriptRoot\build\VHRTwoSidedLightingFix-$version.zip"
    Compress-Archive -Path "$stage\*" -DestinationPath $zipPath -Force
    Write-Host "Zip: $zipPath"
}
