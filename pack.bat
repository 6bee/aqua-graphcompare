@echo off
set configuration=Debug
set version-suffix="001"
clean ^
  && dotnet restore ^
  && dotnet build src\Aqua.GraphCompare --configuration %configuration% ^
  && dotnet build test\Aqua.GraphCompare.Tests --configuration %configuration% ^
  && dotnet test test\Aqua.GraphCompare.Tests\Aqua.GraphCompare.Tests.csproj --configuration %configuration% ^
  && dotnet pack src\Aqua.GraphCompare\Aqua.GraphCompare.csproj --output "..\..\artifacts" --configuration %configuration% --include-symbols --version-suffix %version-suffix%