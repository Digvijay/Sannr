using Microsoft.Extensions.DependencyInjection;
using Sannr.Core;
using Sannr.AspNetCore;
using System.Diagnostics.Metrics;

var services = new ServiceCollection();
services.TryAddSannrServices(); // Demonstrate DI integration with TryAddEnumerable for idempotency

var provider = services.BuildServiceProvider();
var meter = Metrics.Meter; // Access core metrics for demonstration

Console.WriteLine("Sannr Playground: Demonstrating AOT-compatible features with metrics and DI.");