@echo off
set configuration=Debug
clean ^
  && dotnet test test\Aqua.GraphCompare.Tests --configuration %configuration% ^
  && dotnet pack src\Aqua.GraphCompare        --configuration %configuration%