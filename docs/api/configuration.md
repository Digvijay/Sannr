# Configuration Dictionary

Sannr configuration enables strict diagnostic visibility and metrics execution for Enterprise-grade Web Applications. Providing tuning instructions within `Program.cs` alters what the underlying generated validators export when encountering anomalous data.

## Initializing Configuration

The `AddSannr(options => { ... })` setup defines global behaviors.

```csharp
builder.Services.AddSannr(options => 
{
    // Performance Metrics Flags
    options.EnableMetrics = true;          
    options.MetricsPrefix = "myapp_sannr"; 

    // Error Reporting Flags
    options.EnableEnhancedErrorResponses = true;
    options.IncludeValidationRuleMetadata = true; 
    options.IncludeValidationDuration = true;     
});
```

## Parameter Definitions

| Property | Type | Default | Expected Behavior |
| :--- | :--- | :--- | :--- |
| `EnableMetrics` | `bool` | `false` | Enables collection of `System.Diagnostics.Metrics`. Hooks automatically into OpenTelemetry. |
| `MetricsPrefix` | `string` | `"sannr_validation"` | Identifies histograms and validation failure counts natively for your Grafana or App Insights exports. |
| `EnableEnhancedErrorResponses` | `bool` | `false` | Expands generic `ProblemDetails` structures by providing correlation IDs, timestamps, and model identifiers natively to client responses. |
| `IncludeValidationRuleMetadata` | `bool` | `true` | Identifies which discrete property boundaries failed constraint evaluation across APIs. |
| `IncludeValidationDuration` | `bool` | `false` | Specifies exact execution lengths directly within your returned payload metrics. |

### Note on Compilation Overheads
These changes do **not** slow down execution times generically. They evaluate strictly off global static configuration parameters within the DI pipeline or short-circuit immediately.
