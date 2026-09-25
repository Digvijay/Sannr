# Known issues

## 1. The ASP.NET Core integration does not enforce validation

**Severity: high. Security-relevant.**

**Status: open. Affects 1.6.0 and earlier.**

`WithSannrValidation` does not reject invalid payloads. Observed against an endpoint bound to a
model whose generated validator rejects the payload outright:

```json
{ "flightCode": "X", "passportNumber": "ab", "seatPreference": "Balcony" }
```

The request was answered `200 OK` with the invalid values accepted. `[Sanitize]` also did not
run: `"  va123 "` arrived at the handler untrimmed.

Reproduced with both public entry points:

- `SannrEndpointExtensions.WithSannrValidation(RouteGroupBuilder)`
- `RouteHandlerBuilderExtensions.WithSannrValidation(RouteHandlerBuilder)`

and under both JIT and Native AOT. All four combinations accepted the payload.

The generated validator is not at fault. Resolving it from `SannrValidatorRegistry` and invoking
it directly rejects exactly these payloads and applies `[Sanitize]` correctly. The defect is in
the endpoint filter layer.

### Why this is worse than an ordinary bug

`SannrValidatorRegistry.ValidateAsync` returns **success** when no validator is registered for
the requested type. A registration, wiring, or generator failure therefore presents as "every
input is valid" rather than as an error. A consumer who trusts the filter ends up with an API
that performs no input validation, with nothing in the logs to indicate it.

For a validation library, silently validating nothing is a security issue.

### Workaround

Do not rely on the filter. Resolve the validator explicitly and fail closed:

```csharp
// At startup: refuse to run without the validator you expect.
if (!SannrValidatorRegistry.TryGetValidator(typeof(BookingRequest), out _))
{
    throw new InvalidOperationException(
        "Sannr validator for BookingRequest is not registered. Refusing to start: " +
        "the registry reports success for unregistered types, so this would " +
        "otherwise silently disable validation.");
}

// In the handler:
if (!SannrValidatorRegistry.TryGetValidator(typeof(BookingRequest), out var validate))
{
    return Results.Problem("Validator unavailable.", statusCode: 500);
}

var result = await validate(new SannrValidationContext(request));
if (!result.IsValid)
{
    return Results.ValidationProblem(
        result.Errors
            .GroupBy(e => e.MemberName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()));
}
```

A worked example is in the
[viking-air](https://github.com/Digvijay/viking-air) demo's `VikingAir.Api/Program.cs`.

### Planned fix

1. Make an unregistered type fail closed. If an opt-out is needed, make it explicit —
   `AddSannr(o => o.AllowUnvalidatedTypes = true)` — so it appears in review.
2. Have `WithSannrValidation` throw at startup when it cannot find a validator for the
   endpoint's bound parameter types, instead of registering a filter that passes everything.
3. Add integration tests that drive a real `WebApplicationFactory` and assert that a
   known-invalid payload produces HTTP 400. Unit tests over the generated validator pass today
   and cannot catch this class of defect.

---

## 2. `WithSannrValidation` is ambiguous between two extension classes

**Severity: low. Affects 1.6.0.**

`WithSannrValidation` is declared for the same receiver type in both
`RouteHandlerBuilderExtensions` and `SannrEndpointExtensions`. Calling it in extension-method
form fails to compile:

```
error CS0121: The call is ambiguous between the following methods or properties
'RouteHandlerBuilderExtensions.WithSannrValidation(RouteHandlerBuilder)' and
'SannrEndpointExtensions.WithSannrValidation(RouteHandlerBuilder)'
```

Consumers must qualify the call with the declaring type, which defeats the purpose of an
extension method.

**Fix:** remove or rename one of the overload sets.

---

## 3. Transitive `Microsoft.OpenApi` carries a High-severity advisory

**Severity: moderate. Affects 1.6.0.**

Sannr 1.6.0 resolves `Microsoft.OpenApi` 2.4.1, which carries **GHSA-v5pm-xwqc-g5wc** (High).
Any consumer running the following inherits the finding:

```bash
dotnet list package --vulnerable --include-transitive
```

**Workaround:** pin a fixed version in the consuming project.

```xml
<PackageReference Include="Microsoft.OpenApi" Version="2.12.2" />
```

**Fix:** raise Sannr's version floor so consumers do not have to.

---

## Reporting

Security-relevant issues should follow [SECURITY.md](../SECURITY.md) rather than being filed as
public issues.
