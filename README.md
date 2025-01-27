# Setup

## Run locally

Run editor locally
`yarn start`


## Known Todo's

- [ ] Publish lexy-typescript as npm package 'lexy-compiler' and include it from the npm repository
- [ ] Check how state management can be improved. It feels a bit messy.
- [ ] Develop a backend (preferably in node.js or dotnet) so it can run locally as an editor and publicly as a playground application.
- [ ] Document versioning strategy for lexy-langage and it's dependencies.

# Implementations notes

## Submodules

**lexy-language** and **lexy-editor** are both included as git submodules. 
- **lexy-language** is used in the automated tests to 
ensure that the parser and compiler are running against the latest lexy language specifications.
- **lexy-editor** is included in Lexy.Web.Editor which is a starting point for a dotnet backend for the editor web app. This is still an empty project atm.