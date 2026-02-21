# Deep Cloning

Object cloning in the .NET ecosystem typically demands a significant performance compromise.

- **Option 1: JSON serialization roundtrips.** Huge memory consumption and latency penalties.
- **Option 2: Deep System.Reflection inspection**. Broken completely by Native AOT trimming.
- **Option 3: Manual explicit mapper code.** Error-prone and impossible to keep maintained at scale.

Sannr tackles this identically to its validation workflows: **Source-generated copying logic**.

## Implementation

Whenever you declare `[SannrReflect]` upon your models, the generator creates a custom `DeepClone()` method inside the Shadow Type structurally traversing object arrays without reflection or JSON bindings.

```csharp
using Sannr;

[SannrReflect]
public class User 
{ 
    public int Id { get; set; }
    public List<Address> Addresses { get; set; } 
}

[SannrReflect]
public class Address 
{ 
    public string City { get; set; } 
}
```

## Executing the Deep Clone

A clone is performed by invoking the hardcoded MSBuild logic explicitly emitted for the requested classes structure exactly.

```csharp
var original = new User 
{ 
    Id = 1, 
    Addresses = new List<Address> { new Address { City = "Oslo" } } 
};

// Creates a full, completely unreferenced deep copy!
var clone = UserShadow.DeepClone(original); 

clone.Addresses[0].City = "Bergen";

// original.Addresses[0].City is completely isolated and retains "Oslo"
```
