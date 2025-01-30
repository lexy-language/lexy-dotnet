# lexy-typescript

An implementations of the [lexy-language](https://github.com/lexy-language/lexy-language) in typescript.
Check the [lexy-language](https://github.com/lexy-language/lexy-language) or the online [demo](https://lexy-language.github.io/lexy-demo/)
to understand the purpose of Lexy.

NuGet .NET package: **todo**

# Contribution

Check [lexy-language](https://github.com/lexy-language/lexy-language) for more information about how to contribute.

# Known improvements

- [ ] Code: get rid of all warning 
- [ ] Benchmarking: add performance tests and improve compilation time 

# Implementations notes

## Run locally

Ensure .NET (7 or above) and node.js (v16 or above) are installed.
Tested with .NET 7.0.410 and node.js v16.20.2.~~~~

Run build 'dotnet build'
Run automated tests 'dotnet test'

## Submodules

**lexy-language** and **lexy-editor** are both included as git submodules. 
- **lexy-language** is used in the automated tests to 
ensure that the parser and compiler are running against the latest lexy language specifications.
- **lexy-editor** is included in Lexy.Web.Editor which is a starting point for a dotnet backend for the editor web app. This is still an empty project atm.

You can use `yarn update-submodules` to pull the latest content from git.