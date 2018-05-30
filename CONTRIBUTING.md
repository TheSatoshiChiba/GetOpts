# How to contribute

A pull request needs a detailed description of what it is fixing or adding to the product. There is no formal guideline on how this has to look like. Just be thorough. Any change must not break old user code! If an API needs to be removed it has to be deprecated first (via the `System.Obsolete` attribute).

## Code Style

Please follow the already established style. Make sure lines are not wider than 80 characters. XML Documentation for public API is absolutely necessary.

## Tests and coverage

Every new feature or code branch has to be covered by unit tests. Tests have to be added to `GetOpts.Tests.csproj`. Code coverage must be **100%**. To run unit tests with code coverage execute `dotnet test GetOpts.Tests/GetOpts.Tests.csproj -f netcoreapp2.0 -v n /p:CollectCoverage=true`.
