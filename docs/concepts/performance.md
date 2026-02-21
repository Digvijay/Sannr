# Performance Deep Dive

Sannr was developed around the single premise that enterprise metadata scanning overheads destroy horizontal scaling metrics.

## Throughput Comparisons

*Tested on an Intel Core i7-4980HQ equivalent CPU, parsing heavily burdened enterprise entities across concurrent `IHost` implementations.*

**A "Simple" Request (3 string fields):**
- DataAnnotations: ~2.8 microseconds.
- FluentValidation: ~1.3 microseconds.
- **Sannr: ~0.2 microseconds.**

**A "Complex" Request (15 constrained hierarchy elements):**
- DataAnnotations: ~12.1 microseconds.
- FluentValidation: ~5.6 microseconds.
- **Sannr: ~0.6 microseconds.**

If you serve roughly `10,000` RPS at scale, older metadata models steal continuous seconds per minute exclusively reflecting type hierarchies. We remove that penalty instantly.

## Garbage Collection Pressure

`Sannr` is the single lowest-allocating bounds validation tool. Because it creates no anonymous types, reflection caches, or expression dynamic trees:

A complex evaluation allocates exactly **392 Bytes**.
DataAnnotations allocates **8.1 Kilobytes**.

This forces Gen0 heap cleanup substantially more aggressively on standard workloads, interrupting synchronous processes significantly less frequently with `Sannr`.

## Serverless Cost Impact

Cold starts incur tremendous compiler metadata inspection. By natively shifting execution to AOT, your serverless `IHost` doesn't inspect application models before routing a packetthe constraints are compiled seamlessly in MSIL static bounds directly upon the assembly launch.
