# Sannr vs Others

Sannr was built to replace `System.ComponentModel.DataAnnotations` (and popular third-party alternatives like FluentValidation) in modern cloud-native .NET applications.

## Performance Comparison

Standard validation libraries rely heavily on runtime Reflection. This implies continuous latency, boxed objects, and high GC pressure.

Sannr generates highly optimized static code that behaves exactly like hand-written logic.

| Feature | System.ComponentModel.DataAnnotations | FluentValidation | **Sannr** |
| :--- | :--- | :--- | :--- |
| **Runtime Mechanism** | Reflection (Slow) | Expression Trees/Reflection | **Static C# (Instant)** |
| **Native AOT** |    Requires warnings/trimming |    Custom interceptors needed | **  100% Trimming Safe** |
| **Async Support** |  Synchronous Only |   Supported | **  Native `Task<T>`** |
| **Dependency Injection** |  Service Locator Anti-Pattern |   Via Object Factory | **  `IServiceProvider` Support** |
| **Sanitization** |  Manual code in Controllers |  Manual/Complex hooks | **  `[Sanitize]` Built-in** |

## Benchmarks

Validation runs in microseconds. See for yourself.

| Scenario | DataAnnotations | FluentValidation | **Sannr** | **Difference** |
| :--- | :--- | :--- | :--- | :--- |
| **Speed (Simple)** | 2,802.4 ns | 1,371.3 ns | **207.8 ns** |   **13.5x faster** |
| **Speed (Complex)** | 12,156.7 ns | 5,682.9 ns | **623.5 ns** |   **20x faster** |
| **Memory (Complex)** | 8,192 B | 1,208 B | **392 B** |   **95% less RAM** |
