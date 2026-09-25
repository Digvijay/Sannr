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

## Supported frameworks

As of 1.7.0, Sannr multi-targets `net8.0` (LTS) and `net10.0` (current), rather than forcing
consumers onto the newest runtime. `net11.0` is built and tested in CI behind an opt-in switch, so
preview regressions surface during the preview window, but it is not shipped in the released
package until it is a supported release.

---

## Reporting

Security-relevant issues should follow [SECURITY.md](../SECURITY.md) rather than being filed as
public issues.
