using Microsoft.Extensions.DependencyInjection;
using Sannr.AspNetCore;
using Sannr.Core;

var services = new ServiceCollection();
services.TryAddSannrServices(); // Demonstrate DI integration with TryAddEnumerable for idempotency

var provider = services.BuildServiceProvider();
var meter = Observability.Meter; // Access core metrics for demonstration

Console.WriteLine("Sannr Playground: Demonstrating AOT-compatible features with metrics and DI.");
