# lexy-dotnet  v0.1

[![Build lexy-dotnet](https://github.com/lexy-language/lexy-dotnet/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/lexy-language/lexy-dotnet/actions/workflows/build.yml) ![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/lexy-language/5cd196aad8e9065cdba88b922a8d7bd6/raw/coverage.svg)

Implementation of the [lexy-language](https://github.com/lexy-language/lexy-language) parser and compiler in dotnet (c#).
Check the [lexy-language](https://github.com/lexy-language/lexy-language) or the online [demo](https://lexy-language.github.io/lexy-demo/)
to understand the philosophy and use cases of Lexy.

NuGet .NET package: **todo**

# Contribution

Check [lexy-language](https://github.com/lexy-language/lexy-language) for more information about how to contribute.

# Known improvements

- [ ] Code: get rid of all warning 
- [ ] Benchmarking: add performance tests and improve compilation time 
- [ ] Packaging: publish NuGet package from GitHub Actions
- [ ] Editor API: develop API for editot (or in node.js) (Lexy.Web.Editori is currently excluded from solution)

# Implementations notes

## Run locally

Ensure .NET 7 are installed.
Tested with .NET 7.0.410 

Run build 'dotnet build'
Run automated tests 'dotnet test'

It contains a project **Lexy.Web.Editor** which could be a start for an API for lexy-editor. It is a react app and needs node.js (v16 or above). Tested with node.js v16.20.2.


## Submodules

**lexy-language** and **lexy-editor** are both included as git submodules. 
- **lexy-language** is used in the automated tests to 
ensure that the parser and compiler are running against the latest lexy language specifications.
- **lexy-editor** is included in Lexy.Web.Editor which is a starting point for a dotnet backend for the editor web app. This is still an empty project atm.

You can use `yarn update-submodules` to pull the latest content from git.
