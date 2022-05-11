# Releasing Rownd for .NET

1. Bump the package version in `RowndSharedLib.csproj` using appropriate semver scheme.

2. Commit the changes and run `git tag <version>` based on the version you just set in the project file.

3. Run `dotnet pack -c Release` to create a new release package.

4. Upload the package to [NuGet](https://www.nuget.org/packages/manage/upload).