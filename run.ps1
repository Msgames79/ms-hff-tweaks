# https://github.com/constup/vdf-converter-powershell/blob/master/src%2Fvdf-converter.ps1
#Licensed under MPL 2.0
#https://www.mozilla.org/en-US/MPL/2.0/
function ConvertFrom-Vdf {
    param (
        [Parameter(Mandatory = $true)]
        [string]$vdfContent
    )

    $lines = $vdfContent -split "`r?`n"
    $keysBuffer = [System.Collections.Generic.List[string]]::new()
    $valuesBuffer = [System.Collections.Generic.List[PSObject]]::new()

    foreach ($line in $lines) {
        $trimmedLine = $line.Trim()

        if ($trimmedLine -eq "{") {
            if ($currentPSObject) {
                $valuesBuffer.Add($currentPSObject)
                $currentPSObject = [PSCustomObject]@{}
            }
            else {
                $currentPSObject = [PSCustomObject]@{}
            }
        }
        elseif ($trimmedLine -eq "}") {
            if ($keysBuffer.Count -gt 0) {
                $key = $keysBuffer[$keysBuffer.Count - 1]
                $keysBuffer.RemoveAt($keysBuffer.Count - 1)
            }

            if ($valuesBuffer.Count -gt 0) {
                $parentObject = $valuesBuffer[$valuesBuffer.Count - 1]
                $valuesBuffer.RemoveAt($valuesBuffer.Count - 1)
            }
            else {
                $parentObject = [PSCustomObject]@{}
            }

            if ($null -eq $parentObject) {
                $parentObject = [PSCustomObject]@{}
            }
            $parentObject | Add-Member -MemberType NoteProperty -Name $key -Value $currentPSObject
            $currentPSObject = $parentObject
        }
        else {
            $stringMatches = [regex]::Matches($trimmedLine, '"([^"]*)"')
            if ($stringMatches.Count -eq 1) {
                $trimmedLine = $trimmedLine.Trim("`"")
                $keysBuffer.Add($trimmedLine)
            }
            elseif ($stringMatches.Count -eq 2) {
                $currentPSObject | Add-Member -MemberType NoteProperty -Name $stringMatches[0].Groups[1].Value -Value $stringMatches[1].Groups[1].Value
            }
        }
    }

    return $currentPSObject
}

if ($PSVersionTable.PSVersion.Major -ge 6 -and -not $IsWindows) {
    "Only for Windows"
}
Set-Location $PSScriptRoot
[xml]$csproj = Get-ChildItem ".\*.csproj" | Get-Content -Raw
$pluginName = $csproj.Project.PropertyGroup.AssemblyName
$appId = "477160"

if (-not (Test-Path "HKCU:\Software\Valve\Steam")) {
    Write-Output "Steam is not installed"
    Read-Host
}

$flag = $true
$steampath = Join-Path (Get-ItemProperty -Path "HKCU:\Software\Valve\Steam").SteamPath "steamapps"
$vdfPSObject = ConvertFrom-Vdf -vdfContent (Get-Content -Raw (Join-Path "${steampath}" "libraryfolders.vdf"))
for ($i = 0; $i -lt ($vdfPSObject.libraryfolders | Get-Member -membertype noteproperty).Count; $i++) {
    if ($vdfPSObject.libraryfolders."$i".apps.psobject.Properties["${appId}"]) {
        $gamefolder = Join-Path ([Regex]::Replace(($vdfPSObject.libraryfolders."$i".path), "\\\\", "\").ToString()) "steamapps\common\human Fall Flat"
        $flag = $false
        break
    }
}
if ($flag) {
    Write-Output "Human Fall Flat is not installed"
    Read-Host
}


dotnet build
if ($?) {
    dotnet build -c Release
    $ErrorActionPreference = 'Continue'
    if (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
        "Waiting HFF to Close"
        Stop-Process -Name "Human"<#, "dnSpy"#> -ErrorAction SilentlyContinue
        while (Get-ItemPropertyValue "HKCU:\Software\Valve\Steam\Apps\477160" "Running") {
            Start-Sleep -Seconds 1
        }
    }
    do {
        Remove-Item "${gamefolder}\BepInEx\plugins\${pluginName}.dll" -Force
    } while (Test-Path "${gamefolder}\BepInEx\plugins\${pluginName}.dll")
    do {
        Copy-Item "bin\Debug\net46\${pluginName}.dll" "${gamefolder}\BepInEx\plugins\${pluginName}.dll" -Force
    } until (Test-Path "${gamefolder}\BepInEx\plugins\${pluginName}.dll")
    Start-Process "steam://rungameid/477160"
}
else {
    "Build failed"
}