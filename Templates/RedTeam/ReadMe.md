# Authoring Custom (dotnet) Templates

Every template must have the following:

1.  A parent folder usually the name of your template "CommandApp"
2.  A subfolder of the parent `.template.config` containing a single file, `template.json`. This file defines everything you need to build your template.
3.  Whatever files you need to include in your directory directly beneath the parent folder.

- CommandApp

  - .template.config
    - template.json
  - RedTeamCommandAppTemplate.csproj
  - appsetting.json
  - Class1.cs

  # template.json

  For a complete schema of this file go [here](http://json.schemastore.org/template) and [Template Wiki](https://github.com/dotnet/templating/wiki/Naming-and-default-value-forms)
