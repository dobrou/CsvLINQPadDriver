<#
.SYNOPSIS

Gets project version from Directory.Build.props and updates it for dependent files.

.DESCRIPTION

Gets project version from Directory.Build.props and updates it for dependent files. Prints version and release notes.

.EXAMPLE
pwsh ./Update-Version.ps1
Gets project version from Directory.Build.props and updates it for dependent files.
#>

[CmdletBinding()]
param()

#Requires -Version 7
Set-StrictMode -Version Latest

$ErrorActionPreference = 'Stop'
$Verbose = $VerbosePreference -ne 'SilentlyContinue'

$PSDefaultParameterValues['*:Verbose']     = $Verbose
$PSDefaultParameterValues['*:ErrorAction'] = $ErrorActionPreference

$ThisFolder = Split-Path (Get-Item (&{ $MyInvocation.ScriptName }))

function Main
{
    $projectFolder      = Join-Path $ThisFolder Src\CsvLINQPadDriver
    $buildPropsFile     = Join-Path $projectFolder Directory.Build.props
    $appManfestFile     = Join-Path $projectFolder app.manifest
    $lpxBuildScriptFile = Join-Path $ThisFolder Deploy\buildlpx.cmd

    Write-Output "Reading '$buildPropsFile'..."

    $buildPropsXml = ([xml](Get-Content $buildPropsFile)).Project.PropertyGroup
    $version = $buildPropsXml.Version
    $buildProps = [PSCustomObject]@{
        Version      = $version
        FullVersion  = "$version.0"
        ReleaseNotes = $buildPropsXml.PackageReleaseNotes
    }

    $fieldsFormat =
        @{ n = 'Version';       e = { $_.Version }      },
        @{ n = 'Full version';  e = { $_.FullVersion }  },
        @{ n = 'Release notes'; e = { $_.ReleaseNotes } }

    Write-Output "`n$(($buildProps | Format-List $fieldsFormat | Out-String).Trim())`n"

    Update-Content $appManfestFile '(?<=CsvLINQPadDriver[\s\S]+\s+version=")[^"]+(?=[\s\S]+?type)',$buildProps.FullVersion
    Update-Content $lpxBuildScriptFile '(?<=version=)[^\r\n]+',$buildProps.Version
}

function Update-Content($file, $replace)
{
    Write-Output "Updating '$file'..."

    (Get-Content $file -Raw) -replace $replace | Set-Content $file -NoNewline
}

. Main
