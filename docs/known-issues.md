# Known issues

This file records defects found in Sannr, whether or not they are fixed. Items are kept after they
are resolved so that anyone evaluating a specific released version can see what applied to it.

| # | Issue | Severity | Affects | Status |
|---|---|---|---|---|
| 1 | ASP.NET Core integration did not enforce validation | High | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 2 | `WithSannrValidation` ambiguous between two classes | Low | 1.6.0 | **Fixed in 1.7.0** |
| 3 | Vulnerable transitive `Microsoft.OpenApi` | High | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 4 | Vulnerable transitive `OpenTelemetry` | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 5 | `global.json` made the repository unbuildable | High | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 6 | Dead `TestGenerator` shipped to every consumer | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 7 | Generator targeted `netstandard2.1` (RS1041) | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 8 | `dotnet pack --no-build` failed on the solution | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 9 | Fluent validators silently not generated | High | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 10 | Generator state leaked across compilations; output non-deterministic | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 11 | Debug scaffolding injected into every consumer | Moderate | ≤ 1.6.0 | **Fixed in 1.7.0** |
| 12 | Obsolete package pins warned on the .NET 11 SDK (NU1510) | Low | build only | **Fixed in 1.7.0** |
| 13 | Command-line AOT flag broke generator build (NETSDK1207) | High | CI only | **Fixed in 1.7.0** |
| 14 | IL-warning list parsing and missing test-file newline broke CI | High | CI only | **Fixed in 1.7.0** |

---

## 1. The ASP.NET Core integration did not enforce validation

**Severity: high. Security-relevant fail-open.**
**Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

`WithSannrValidation` did not reject invalid payloads. Observed against an endpoint bound to a
model whose generated validator rejects the payload outright:

```json
{ "flightCode": "X", "passportNumber": "ab", "seatPreference": "Balcony" }
```

The request was answered `200 OK` with the invalid values accepted. `[Sanitize]` also did not run:
`"  va123 "` arrived at the handler untrimmed. Reproduced with both public entry points, under both
JIT and Native AOT. All four combinations accepted the payload.

The generated validator was never at fault. Resolving it from the registry and invoking it directly
rejected exactly these payloads and applied `[Sanitize]` correctly.

### Root cause

Two unrelated classes were both named `SannrValidatorRegistry`:

* `Sannr.SannrValidatorRegistry` — the real registry, populated by the generator through a
  `[ModuleInitializer]`.
* `Sannr.AspNetCore.SannrValidatorRegistry` — a second class that nothing ever wrote to.

C# binds an unqualified name against the containing namespace first, so every lookup written inside
`Sannr.AspNetCore` — including the one in the endpoint filter — resolved to the permanently empty
registry. The filter found no validator and let the request through.

This was compounded by `ValidateAsync` returning **success** for an unregistered type. A
registration, wiring, or generator failure therefore presented as "every input is valid" rather than
as an error, with nothing in the logs to indicate it. For a validation library, silently validating
nothing is a security issue.

### Why the existing tests could not catch it

Sannr's tests resolved the validator from the registry and asserted on its result. That is a test of
the generator, and the generator was correct. Nothing exercised the filter through a real HTTP
request, which is the only place the namespace shadowing became observable. The bug was found by
building an application that used the library the way a consumer would.

### The fix

1. All registry lookups are fully qualified. The duplicate filter and extension classes are
   removed, and `Sannr.AspNetCore.SannrValidatorRegistry` is now an `[Obsolete]` forwarder that
   **throws** for an unregistered type instead of reporting success.
2. `WithSannrValidation` fails closed. At startup it verifies that a generated validator exists for
   each parameter it is asked to guard and throws if one is missing. Opt out explicitly, so that it
   shows up in review:

   ```csharp
   builder.Services.Configure<SannrValidationOptions>(o => o.RequireValidator = false);
   ```
3. `tests/Sannr.Tests/EndpointFilterIntegrationTests.cs` adds seven tests that drive a real request
   pipeline and assert that a known-invalid payload produces HTTP 400.

### Verifying it yourself

The [viking-air](https://github.com/Digvijay/viking-air) integration demo posts the payload above
to a running instance and asserts a `400` with per-field errors, through the real
`.WithSannrValidation()` filter with no workaround in the application code.

---

## 2. `WithSannrValidation` was ambiguous between two extension classes

**Severity: low. Status: fixed in 1.7.0. Affects 1.6.0.**

`WithSannrValidation` was declared for the same receiver type in both
`RouteHandlerBuilderExtensions` and `SannrEndpointExtensions`, so calling it in extension-method
form failed to compile:

```
error CS0121: The call is ambiguous between the following methods or properties
```

Consumers had to qualify the call with the declaring type, defeating the purpose of an extension
method. The duplicate class was the other half of the namespace-shadowing mistake in item 1;
removing it fixed both.

---

## 3. Transitive `Microsoft.OpenApi` carried a High-severity advisory

**Severity: high. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

Sannr 1.6.0 resolved `Microsoft.OpenApi` 2.4.1, carrying **GHSA-v5pm-xwqc-g5wc** (High). Any
consumer running `dotnet list package --vulnerable --include-transitive` inherited the finding.

Fixed by raising Sannr's floor to 2.12.2 so consumers do not have to pin it themselves. The entry
in `Directory.Packages.props` carries a comment naming the advisory so it is not casually lowered.

---

## 4. Transitive `OpenTelemetry` carried a Moderate advisory

**Severity: moderate. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

Masked by item 3 until that was resolved: `OpenTelemetry` 1.14.0 carries
**GHSA-g94r-2vxg-569j** (Moderate). All OpenTelemetry package floors are raised to 1.18.0.

`dotnet list package --vulnerable --include-transitive` is now clean across the solution.

---

## 5. `global.json` made the repository unbuildable

**Severity: high for contributors. Status: fixed in 1.7.0.**

`global.json` combined a `latestPatch` roll-forward policy with `allowPrerelease: false`, pinned to
an SDK band that is no longer installed on current machines. The repository could not be built on
**any** currently shipping SDK — a new contributor hit an SDK resolution error before reaching a
line of code.

Fixed by normalising `global.json` to accept the supported SDK band.

---

## 6. A dead `TestGenerator` shipped to every consumer

**Severity: moderate. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

`Sannr.Gen` contained a leftover non-incremental `ISourceGenerator` named `TestGenerator`. It was
packed into the shipped analyzer and injected a dead generated file into every consuming
compilation. It also violated RS1035 and RS1042 — the latter meaning it was not an incremental
generator, so it ran on every keystroke in the IDE for every consumer.

Deleted.

---

## 7. The generator targeted `netstandard2.1`

**Severity: moderate. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

Roslyn requires compiler extensions to target `netstandard2.0` (RS1041). Targeting `netstandard2.1`
makes generator behaviour depend on the host compiler's loading behaviour rather than being
deterministic.

Retargeted to `netstandard2.0`. This required replacing one API unavailable there
(`string.Split(char, StringSplitOptions)`) and correcting the hardcoded pack path.

---

## 8. `dotnet pack --no-build` failed on the solution

**Severity: moderate. Fixed in 1.7.0. Affected the build only, not consumers.**

`src/Sannr.AspNetCore/Sannr.AspNetCore.csproj` packs `Sannr.Core` into its own package through a
`CopyProjectReferencesToPackage` target that depends on `ResolveReferences`. Under
`dotnet pack --no-build` that dependency still invoked `Build` on `Sannr.Core`, which the SDK
forbids (`NETSDK1085`), for both `net8.0` and `net10.0`. Packing the solution after building it,
which is exactly the CI "Pack" step, produced `Sannr.Cli` only and failed. `GeneratePackageOnBuild`
packed on every build and hid the problem locally.

**Fix:** `BuildProjectReferences` is `false` when `NoBuild` is set, and `GeneratePackageOnBuild`
is removed (the publish workflow packs explicitly). `dotnet pack --no-build` now produces
`Sannr.1.7.0.nupkg` and `Sannr.Cli.1.7.0.nupkg`. The contents were inspected: `Sannr.AspNetCore`
and `Sannr.Core` for `net8.0` and `net10.0`, and `Sannr.Gen` under `analyzers/dotnet/cs`.

---

## 9. Fluent validators were silently not generated

**Severity: high. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

A class deriving from `ValidatorConfig<T>` is meant to produce a static `{Name}FluentValidator`.
The generator only emitted that output when the project had also set `EnableSannrSchemaGen` or
called `AddSannr()`, neither of which has anything to do with fluent rules. Without them the
validator was simply absent: no diagnostic, no warning, and a consumer calling it got `CS0103`.

The test project that should have caught this, `tests/Sannr.FluentValidation.Tests`, did not
compile for exactly that reason. It was not part of `Sannr.sln`, so neither local builds nor CI
ever built it, and it targeted `net8.0` only.

**Fix:** fluent output no longer depends on either switch. The test project is in the solution,
targets every supported framework, and passes (7 tests × 2 frameworks). `FluentGeneratorTests`
drives the generator directly with no opt-in and asserts the validator is produced; it failed
before the fix.

---

## 10. Generator state leaked across compilations, and its output was non-deterministic

**Severity: moderate. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

Three defects with one consequence — the same input did not reliably produce the same output:

* A `static HashSet<string>` recorded which validators had been emitted. Generator instances are
  reused by the IDE and the compiler server, so on the second compilation every validator was
  considered "already generated" and silently skipped.
* Fluent output used `Guid.NewGuid()` in its hint name, so every build produced a different file
  name and defeated incremental caching and deterministic builds.
* Two templates stamped `// Generated: {DateTime.Now}` into their output.

**Fix:** the set is per invocation, hint names are `{namespace}.{class}.g.cs`, and the timestamps
are gone. `FluentGeneratorTests` runs the generator twice in one process and asserts the validator
is produced both times, and that file names and contents are identical; both tests failed before
the fix.

---

## 11. Debug scaffolding was injected into every consumer

**Severity: moderate. Status: fixed in 1.7.0. Affects 1.6.0 and earlier.**

Entry 6 removed a dead `TestGenerator` class but not the rest of the scaffolding it belonged to.
`SannrGenerator` itself still added `GeneratorInitDebug.g.cs` (containing `DateTime.Now`),
`TestGenerator.g.cs`, `ValidationTargetsDebug.g.cs` and `FluentValidatorsDebug.g.cs` to every
consuming compilation, and emitted `// DEBUG: Raw method body` comments into generated validators.
A syntax-based fluent parser (`ParseFluentValidationFromSyntax` and its helpers) was never
called.

This entry exists because entry 6 was recorded as fixed when it was only partly fixed.

**Fix:** all debug output and the dead parser are deleted (about 300 lines). A test asserts that a
compilation containing a fluent validator produces no `*Debug*` or `TestGenerator.g.cs` file, no `// DEBUG`
comment and no timestamp.

---

## 12. Obsolete package pins warned on the .NET 11 SDK

**Severity: low. Fixed in 1.7.0. Affected the build only.**

Both test projects referenced `System.Net.Http` 4.3.4 and `System.Text.RegularExpressions` 4.3.1,
the usual pins against advisories in the 4.3.0 packages that old `netstandard1.x` dependencies pull
in. Every target framework here supplies both assemblies, and the SDK prunes those packages from
the graph, so the pins did nothing; SDK 11 reports that as `NU1510`, twelve times.

**Fix:** removed from both projects and from `Directory.Packages.props`. Restoring with
`NuGetAuditMode=all` reports no advisories, and neither package appears in the transitive graph.

---

## 13. `-p:PublishAot=true` on the command line broke the generator project (NETSDK1207)

**Severity: high. Fixed in 1.7.0. Affected CI only — the AOT gate had never run to completion.**

`aot-validation.yml` passed `-p:PublishAot=true` to `dotnet publish`. The flag was redundant, since
the target project already declares `PublishAot`. It was also harmful: a `-p:` switch on the command
line creates a **global property**, and MSBuild propagates global properties into every
`ProjectReference` it builds. The generator targets `netstandard2.0`, which cannot be AOT-compiled,
so the run failed with `error NETSDK1207: Ahead-of-time compilation is not supported for the target
framework.`

The identical property declared *inside* a project file does not flow across a `ProjectReference`.
That asymmetry is why this reproduced only on CI.

**Fix:** the flag is removed. AOT remains configured in the project file.

---

## 14. The IL-warning list was split on its commas, and one file broke `dotnet format`

**Severity: high. Fixed in 1.7.0. Affected CI only.**

The same AOT step passed `-p:WarningsAsErrors=IL2026,IL2046,IL2062,...`. The dotnet CLI splits
`-p:` values on commas, so every code after the first was parsed as a separate switch and the run
failed with `MSBUILD : error MSB1006: Property is not valid. Switch: IL2046` before compiling
anything. The gate had therefore never enforced a single trim or AOT warning as an error. A bare
`;` would not help either, because it is the property separator.

Separately, `dotnet format --verify-no-changes Sannr.sln` — which CI runs — failed with
`FINALNEWLINE` on `tests/Sannr.FluentValidation.Tests/FluentValidationTests.cs`, a file added
during this review without a trailing newline.

**Fix:** the codes are joined with `%3B`, the escaped semicolon, which reaches MSBuild as one
property value. The test file gained its trailing newline; `dotnet format --verify-no-changes
Sannr.sln` now exits 0.

Both were found by opening a pull request, which ran CI on GitHub-hosted x64 runners for the first
time. Neither could have been reproduced by building locally, because both are properties of how
the workflow invokes the CLI rather than of the code.

---

# Open

Nothing is open in Sannr.

Two caveats belong here rather than in the table, because neither is a defect and both bound what
the entries above are worth:

* Every result recorded here was produced on a single Windows ARM64 machine. CI has never executed
  on a GitHub-hosted runner, so nothing above is confirmed on x64 or on Linux.
* The `net11.0` leg is opt-in via `IncludePreviewTargetFramework`. It has been exercised on the
  same machine with SDK `11.0.100-rc.1.26425.128` (restore, build and every test, `net8.0`,
  `net10.0` and `net11.0`), with no failures. A release candidate is not a release; the leg
  should be re-run against the GA SDK.

A record of twelve fixed defects measures how hard this repository was looked at. It is not a claim
that there is nothing left to find.

---

## Supported frameworks

As of 1.7.0, Sannr multi-targets `net8.0` (LTS) and `net10.0` (current), rather than forcing
consumers onto the newest runtime. `net11.0` is built and tested in CI behind an opt-in switch, so
preview regressions surface during the preview window, but it is not shipped in the released
package until it is a supported release.

---

## Reporting

Security-relevant issues should follow [SECURITY.md](../SECURITY.md) rather than being filed as
public issues.
