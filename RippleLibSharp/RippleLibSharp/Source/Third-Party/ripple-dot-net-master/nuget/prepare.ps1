param (
    [string]$configuration="Release"
)

$scriptpath = $MyInvocation.MyCommand.Path
$wd = Split-Path $scriptpath
$buildpath = Join-Path $wd build
$assemblies = Join-Path $buildpath lib\net45
rm -Recurse -Force $buildpath
Push-Location $wd

msbuild  /property:Configuration=$configuration ..\Ripple.TxSigning\Ripple.TxSigning.csproj
msbuild  /property:Configuration=$configuration ..\Ripple.Testing\Ripple.Testing.csproj
mkdir $assemblies
cp ..\Ripple.TxSigning\bin\$configuration\Ripple*.dll $assemblies
cp ..\Ripple.TxSigning\bin\$configuration\Chaos*.dll $assemblies
cp ..\Ripple.Testing\bin\$configuration\Ripple*.dll $assemblies

nuget pack Ripple.NET.nuspec -BasePath build

Pop-Location
