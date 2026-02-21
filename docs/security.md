# Security Policy

## Reporting Security Vulnerabilities

If you discover a security vulnerability in Sannr, please report it responsibly by emailing **security @ digvijay dot dev**.

**Please do NOT report security vulnerabilities through public GitHub issues, discussions, or pull requests.**

### What to Include in Your Report

To help us respond effectively, please include:

- A clear description of the vulnerability
- Steps to reproduce the issue
- Potential impact and severity
- Any suggested fixes (optional)
- Your contact information for follow-up

### Response Timeline

We will acknowledge receipt of your report within 48 hours and provide a more detailed response within 7 days indicating our next steps.

We will keep you informed about our progress throughout the process of fixing the vulnerability.

## Supported Versions

We actively support the following versions with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Disclosure Policy

- We follow a coordinated disclosure process
- Vulnerabilities will be disclosed publicly only after a fix has been released
- We will credit researchers who report vulnerabilities (unless they prefer to remain anonymous)
- We aim to release fixes within 90 days of receiving a report

## Safe Harbor

We consider security research conducted in accordance with this policy to be authorized. We will not pursue legal action against researchers who:

- Follow the reporting guidelines above
- Do not disrupt services or destroy data
- Do not access or modify user data without explicit permission
- Provide reasonable time for us to respond before public disclosure

## Security Best Practices for Sannr Users

While this policy focuses on reporting vulnerabilities in Sannr itself, here are key security considerations when using Sannr:

### Server-Side Validation is Mandatory

Sannr generates client-side validation code for user experience, but **all validation must be enforced on the server side**. Client-side validation can be bypassed by malicious users.

```csharp
// GOOD: Server validates all inputs
app.MapPost("/api/users", (UserModel user) =>
{
    // Sannr automatically validates during model binding
    return Results.Ok("User created");
});
```

### Input Sanitization

Use Sannr's sanitization attributes to prevent XSS and injection attacks:

```csharp
public class UserProfile
{
    [Sanitize(Trim = true, ToLower = true)]
    [StringLength(50)]
    public string? Username { get; set; }
}
```

### Authentication & Authorization

Sannr validation is **NOT** a replacement for authentication/authorization. Always combine with proper security measures:

```csharp
[Authorize]
app.MapPost("/api/admin/users", (CreateUserRequest request) =>
{
    // Both auth and validation protect the endpoint
});
```

## Contact

For security-related questions or concerns:
- **Email:** security @ digvijay dot dev
- **GitHub Issues:** For non-sensitive security improvements and questions

Thank you for helping keep Sannr and its users secure!