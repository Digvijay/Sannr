# Contributing to Sannr

Thank you for your interest in contributing to Sannr! We welcome contributions from the community to help make this the standard for AOT-first validation in .NET.

## 📋 Table of Contents
- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Pull Requests](#pull-requests)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)

---

## Code of Conduct
This project adheres to the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/). By participating, you are expected to uphold this code.

## How to Contribute

### Reporting Bugs
Bugs are tracked as GitHub issues. When filing an issue, please include:
1.  **Version**: The version of Sannr you are using.
2.  **Reproduction**: A code snippet or minimal reproduction repository.
3.  **Expected vs Actual**: What you expected to happen vs what actually happened.

### Suggesting Enhancements
We love new ideas! If you want to suggest a new "Power Attribute" (e.g., `[LuhnCheck]` or `[Isbn]`), please open a **Feature Request** issue first to discuss the API design before writing code.

### Pull Requests
1.  **Fork** the repository.
2.  **Branch** off `main` (e.g., `feature/add-isbn-validator` or `fix/regex-timeout`).
3.  **Commit** your changes. Please use clear, imperative commit messages (e.g., "Add ISBN validator", not "Added some code").
4.  **Test** your changes. Run `dotnet test` to ensure all standard and generator tests pass.
5.  **Push** to your fork and submit a Pull Request.

---

## Development Workflow

Sannr is a **Source Generator** project. This adds some complexity to the dev loop.

1.  **Open the Solution**: `Sannr.sln`.
2.  **Build**: `dotnet build`.
3.  **Test**: The `tests/Sannr.Tests` project references the generator as an analyzer. 
    * *Note:* Visual Studio sometimes requires a restart to pick up changes to the source generator logic itself.
    * We recommend running `dotnet test` from the CLI for the most reliable results during generator development.

## Coding Standards

* **Style**: We follow standard C# coding conventions (PascalCase for public members, `_camelCase` for private fields).
* **Performance**: This is a high-performance AOT library.
    * Avoid LINQ in hot paths (validation loops).
    * Avoid boxing where possible.
    * Use `StringBuilder` or string interpolation carefully in the generator.
* **Tests**: All new features must be accompanied by unit tests in `Sannr.Tests`.

## License
By contributing, you agree that your contributions will be licensed under its MIT License.
