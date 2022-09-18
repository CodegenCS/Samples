# How to run: .\build.ps1

$configuration="Release"

Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\samplesourcegenerator.simplepocos"

New-Item -ItemType Directory -Force -Path ".\packages-local"

Get-ChildItem $Path -Recurse | Where{$_.FullName -Match ".*\\obj\\.*project.assets.json$"} | Remove-Item
dotnet clean 

Remove-Item -Recurse -Force -ErrorAction Ignore ".\SampleSourceGenerator.SimplePocos\bin\"
Remove-Item -Recurse -Force -ErrorAction Ignore ".\SampleSourceGenerator.SimplePocos\obj\"
Remove-Item -Recurse -Force -ErrorAction Ignore ".\SampleSourceGenerator.SimplePocos.DemoProject\bin\"
Remove-Item -Recurse -Force -ErrorAction Ignore ".\SampleSourceGenerator.SimplePocos.DemoProject\obj\"


& dotnet pack .\SampleSourceGenerator.SimplePocos\SampleSourceGenerator.SimplePocos.csproj -o .\packages-local\

& dotnet run --project SampleSourceGenerator.SimplePocos.DemoProject\SampleSourceGenerator.SimplePocos.DemoProject.csproj

