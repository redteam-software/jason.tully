
dotnet new uninstall RedTeam.Templates

Remove-Item .\nuget\* -Recurse -Force

dotnet pack

dotnet new install .\nuget\RedTeam.Templates.1.0.0.nupkg