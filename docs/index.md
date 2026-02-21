---
layout: home

hero:
  name: "Sannr"
  text: "AOT-First Validation Engine for .NET"
  tagline: "Blazingly fast validation -- up to 20x faster than DataAnnotations with 95% less memory."
  image:
    src: /icon.png
    alt: Sannr Logo
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Architecture
      link: /concepts/architecture
    - theme: alt
      text: View on GitHub
      link: https://github.com/Digvijay/Sannr

features:
  - title: "Native AOT First"
    details: "Roslyn source generators move validation from runtime reflection to compile-time static C#. Zero startup overhead, 100% trimming safe."
  - title: "Familiar API"
    details: "Use attributes you already know -- [Required], [Range], [EmailAddress]. Drop-in replacement for DataAnnotations."
  - title: "Async & Conditional Validation"
    details: "Native async support for database lookups and [RequiredIf] conditional logic, all generated at compile time."
  - title: "Auto-Sanitization"
    details: "Built-in [Sanitize] attribute trims, uppercases, and lowercases input before validation -- no manual controller code."
  - title: "Static Reflection (Shadow Types)"
    details: "Zero-allocation property access, PII tagging, and deep cloning without System.Reflection. AOT-compatible metadata at compile time."
  - title: "Enterprise Integrations"
    details: "Seamless ASP.NET Core, OpenAPI schema generation, client-side validation rules, and observability metrics out of the box."
---
