[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [string]$PackageVersion = "10.1.2",
    [switch]$SkipPackageDownload
)

$ErrorActionPreference = "Stop"

function Copy-DirectoryContents {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Destination)) {
        New-Item -ItemType Directory -Path $Destination | Out-Null
    }

    Get-ChildItem -LiteralPath $Source -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $Destination -Recurse -Force
    }
}

function Resolve-ExecutablePath {
    param(
        [string]$PreferredPath,
        [Parameter(Mandatory = $true)][string]$CommandName
    )

    if (-not [string]::IsNullOrWhiteSpace($PreferredPath) -and
        (Test-Path -LiteralPath $PreferredPath -PathType Leaf)) {
        return (Resolve-Path -LiteralPath $PreferredPath).Path
    }

    $command = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return $command.Source
    }

    return $null
}

function Invoke-Codex {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $output = @(& $script:codexPath @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0 -and -not $AllowFailure) {
        $details = ($output | Out-String).Trim()
        throw "Codex command failed ($exitCode): codex $($Arguments -join ' ')`n$details"
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = $output
    }
}

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
} else {
    $ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
}

$packagesDirectory = Join-Path $ProjectRoot "Packages"
$packagePath = Join-Path $packagesDirectory "com.coplaydev.unity-mcp"
$codexHomePath = $env:CODEX_HOME
if ([string]::IsNullOrWhiteSpace($codexHomePath)) {
    $codexHomePath = Join-Path ([Environment]::GetFolderPath("UserProfile")) ".codex"
}
$codexConfigPath = Join-Path $codexHomePath "config.toml"
$uvxPath = Resolve-ExecutablePath -PreferredPath "D:\Python\Scripts\uvx.exe" -CommandName "uvx.exe"
$script:codexPath = Resolve-ExecutablePath -PreferredPath $env:CODEX_CLI_PATH -CommandName "codex.exe"

if (-not (Test-Path -LiteralPath $packagesDirectory -PathType Container)) {
    throw "Not a Unity project: Packages directory was not found at $packagesDirectory"
}
if ($null -eq $uvxPath) {
    throw "uvx was not found. Install uv or pass it through PATH; expected D:\Python\Scripts\uvx.exe."
}
if ($null -eq $script:codexPath) {
    throw "Codex CLI was not found. Start this script from a Codex installation that provides codex.exe."
}

$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ("AIOnly-UnityMcp-" + [Guid]::NewGuid().ToString("N"))
$packageBackupPath = Join-Path $tempRoot "existing-package"
$stagedPackagePath = Join-Path $tempRoot "staged-package"
$zipPath = Join-Path $tempRoot ("unity-mcp-v" + $PackageVersion + ".zip")
$configBackupPath = Join-Path $tempRoot "config.toml.backup"
$packageExisted = Test-Path -LiteralPath $packagePath -PathType Container
$configExisted = Test-Path -LiteralPath $codexConfigPath -PathType Leaf
$packageWasChanged = $false
$configWasChanged = $false

try {
    New-Item -ItemType Directory -Path $tempRoot | Out-Null

    if ($packageExisted) {
        Copy-DirectoryContents -Source $packagePath -Destination $packageBackupPath
    }

    if (-not $SkipPackageDownload) {
        $downloadUrl = "https://github.com/CoplayDev/unity-mcp/archive/refs/tags/v$PackageVersion.zip"
        Write-Host "Downloading MCP for Unity $PackageVersion..."
        Invoke-WebRequest -UseBasicParsing -Uri $downloadUrl -OutFile $zipPath

        $extractRoot = Join-Path $tempRoot "source"
        Expand-Archive -LiteralPath $zipPath -DestinationPath $extractRoot -Force

        $packageSource = Join-Path $extractRoot ("unity-mcp-$PackageVersion\MCPForUnity")
        if (-not (Test-Path -LiteralPath (Join-Path $packageSource "package.json") -PathType Leaf)) {
            $packageSource = $null
            Get-ChildItem -LiteralPath $extractRoot -Filter "package.json" -Recurse -File | ForEach-Object {
                if ($null -eq $packageSource) {
                    $metadata = Get-Content -Raw -LiteralPath $_.FullName | ConvertFrom-Json
                    if ($metadata.name -eq "com.coplaydev.unity-mcp") {
                        $packageSource = $_.Directory.FullName
                    }
                }
            }
        }

        if ([string]::IsNullOrWhiteSpace($packageSource)) {
            throw "The downloaded archive did not contain the MCP for Unity package."
        }

        $metadata = Get-Content -Raw -LiteralPath (Join-Path $packageSource "package.json") | ConvertFrom-Json
        if ($metadata.name -ne "com.coplaydev.unity-mcp" -or $metadata.version -ne $PackageVersion) {
            throw "Downloaded package metadata does not match com.coplaydev.unity-mcp $PackageVersion."
        }

        Copy-DirectoryContents -Source $packageSource -Destination $stagedPackagePath
        if (-not (Test-Path -LiteralPath (Join-Path $stagedPackagePath "Editor\MCPForUnity.Editor.asmdef") -PathType Leaf)) {
            throw "Downloaded package failed validation: Editor assembly definition is missing."
        }

        $packageWasChanged = $true
        if (Test-Path -LiteralPath $packagePath) {
            Remove-Item -LiteralPath $packagePath -Recurse -Force
        }
        New-Item -ItemType Directory -Path $packagePath | Out-Null
        Copy-DirectoryContents -Source $stagedPackagePath -Destination $packagePath
        Write-Host "Unity package installed at $packagePath"
    } else {
        Write-Host "Skipping Unity package download."
    }

    if (-not (Test-Path -LiteralPath (Join-Path $packagePath "package.json") -PathType Leaf)) {
        throw "Unity package is missing at $packagePath. Remove -SkipPackageDownload to restore it."
    }
    $installedMetadata = Get-Content -Raw -LiteralPath (Join-Path $packagePath "package.json") | ConvertFrom-Json
    if ($installedMetadata.name -ne "com.coplaydev.unity-mcp" -or $installedMetadata.version -ne $PackageVersion) {
        throw "Installed Unity package is not com.coplaydev.unity-mcp $PackageVersion."
    }

    if ($configExisted) {
        Copy-Item -LiteralPath $codexConfigPath -Destination $configBackupPath -Force
    }

    $null = Invoke-Codex -Arguments @("mcp", "remove", "unityMCP") -AllowFailure

    $addArguments = @("mcp", "add", "unityMCP")
    if (-not [string]::IsNullOrWhiteSpace($env:SystemRoot)) {
        $addArguments += @("--env", ("SystemRoot=" + $env:SystemRoot))
    }
    $addArguments += @(
        "--",
        $uvxPath,
        "--from",
        ("mcpforunityserver==" + $PackageVersion),
        "mcp-for-unity",
        "--transport",
        "stdio"
    )
    $null = Invoke-Codex -Arguments $addArguments
    $configWasChanged = $true

    $verification = Invoke-Codex -Arguments @("mcp", "get", "unityMCP")
    $verificationText = ($verification.Output | Out-String)
    if ($verificationText -notmatch [regex]::Escape($uvxPath) -or
        $verificationText -notmatch [regex]::Escape("mcpforunityserver==$PackageVersion")) {
        throw "Codex reported unityMCP, but its command or pinned server version did not match the installation."
    }

    Write-Host "Codex MCP registered: unityMCP"
    Write-Host "Server: $uvxPath --from mcpforunityserver==$PackageVersion mcp-for-unity --transport stdio"
    Write-Host "Restart Codex or start a new Codex task to load the configuration."
}
catch {
    Write-Error $_

    if ($configWasChanged -or $configExisted) {
        if ($configExisted -and (Test-Path -LiteralPath $configBackupPath -PathType Leaf)) {
            Copy-Item -LiteralPath $configBackupPath -Destination $codexConfigPath -Force
            Write-Warning "Codex config was restored from its backup."
        } elseif (-not $configExisted -and (Test-Path -LiteralPath $codexConfigPath)) {
            Remove-Item -LiteralPath $codexConfigPath -Force
        }
    }

    if ($packageWasChanged) {
        if (Test-Path -LiteralPath $packagePath) {
            Remove-Item -LiteralPath $packagePath -Recurse -Force
        }
        if ($packageExisted) {
            New-Item -ItemType Directory -Path $packagePath | Out-Null
            Copy-DirectoryContents -Source $packageBackupPath -Destination $packagePath
            Write-Warning "Unity package was restored from its backup."
        }
    }

    throw
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
