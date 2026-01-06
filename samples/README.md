# Sannr Validation Framework - Complete Demo Suite

[![NuGet](https://img.shields.io/nuget/v/Sannr.svg)](https://www.nuget.org/packages/Sannr)
[![Build Status](https://github.com/Digvijay/Sannr/actions/workflows/ci.yml/badge.svg)](https://github.com/Digvijay/Sannr/actions)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-Compatible-green)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

**Blazingly fast validation** - Up to 19.5x faster than DataAnnotations with 95% less memory usage. This comprehensive demo suite showcases Sannr's enterprise-grade validation capabilities through a complete microservices architecture.

## 🚀 Why Sannr? The Performance Revolution

Sannr delivers **unprecedented validation performance** by leveraging Roslyn source generators to transform runtime reflection into compile-time static code. Experience sub-microsecond validation with near-zero memory allocation.

### Performance That Matters
- **⚡ 19.5x faster** than DataAnnotations for complex models
- **💾 95% less memory** usage with minimal GC pressure
- **🔒 Zero runtime reflection** - AOT-compatible by design
- **📊 Real-time metrics** via Aspire Dashboard integration

## 🏗️ Demo Architecture Overview

This demo suite implements a **production-ready microservices architecture** using .NET Aspire, featuring:

### Services Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web Frontend  │◄──►│   API Service   │◄──►│  App Host       │
│   (Blazor SPA)  │    │ (REST API)      │    │ (Orchestration) │
│                 │    │ • 15+ Endpoints │    │ • Service       │
│ • Client-Side   │    │ • Model Binding │    │   Discovery     │
│   Validation    │    │ • OpenAPI Docs  │    │ • Health Checks │
│ • Interactive   │    │ • Metrics       │    │ • Dashboard     │
│   Testing       │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Technology Stack
- **🎯 .NET 8.0** with Native AOT support
- **🏗️ .NET Aspire** for cloud-native orchestration
- **📡 REST APIs** with comprehensive validation
- **🌐 Interactive Web UI** with client-side validation
- **📊 Real-time Observability** via Aspire Dashboard

## 🎯 Comprehensive Validation Showcase

### Core Validation Features
- ✅ **Required Fields** - Mandatory field validation with custom error messages
- ✅ **String Length** - Min/max length constraints with detailed feedback
- ✅ **Email Validation** - RFC-compliant email format validation
- ✅ **Range Validation** - Numeric ranges for age, price, quantity, dates
- ✅ **Phone Numbers** - International phone number format validation
- ✅ **URLs** - HTTP/HTTPS URL format validation with protocol requirements
- ✅ **Credit Cards** - Payment card number validation (13-19 digits)
- ✅ **File Extensions** - Image and document file type validation

### Advanced Enterprise Features
- ✅ **Conditional Validation** - `RequiredIf` for dynamic business rules
- ✅ **Data Sanitization** - Automatic trimming, case conversion, formatting
- ✅ **Nested Object Validation** - Complex hierarchical data structures
- ✅ **Anti-Spam Protection** - Honeypot validation for bot prevention
- ✅ **Custom Business Rules** - Domain-specific validation logic
- ✅ **Cross-Field Validation** - Multi-property business rule enforcement

### Enterprise-Grade Capabilities
- ✅ **Native AOT Compatibility** - Zero-trim deployment ready
- ✅ **OpenAPI Integration** - Automatic schema generation for APIs
- ✅ **Client-Side Validation** - Generated JavaScript validators
- ✅ **Real-Time Observability** - Metrics and monitoring via Aspire Dashboard
- ✅ **Performance Monitoring** - Validation timing and throughput metrics
- ✅ **Health Checks** - Service availability and dependency monitoring

## 📋 API Endpoints Showcase

### User Management (`/api/users/`)
- `POST /api/users/register` - User registration with sanitization
- `POST /api/users/profile` - Profile updates with conditional validation

### E-Commerce (`/api/products/`, `/api/orders/`)
- `POST /api/products` - Product creation with business rules
- `POST /api/orders` - Complex order processing with nested validation

### Business Operations
- `POST /api/contact` - Contact forms with anti-spam protection
- `POST /api/newsletter/subscribe` - Subscription with duplicate prevention
- `POST /api/weather` - External API integration validation

### Comprehensive Testing Suite (`/api/test/`)
- `POST /api/test/validation` - All validation features in one endpoint
- `POST /api/test/user-profile` - User profile management scenarios
- `POST /api/test/advanced` - Advanced validation patterns
- `POST /api/test/order` - Complex nested object validation
- `POST /api/test/product` - E-commerce business rules
- `POST /api/test/appointment` - Date/time and scheduling validation
- `POST /api/test/client-validation` - Client-side validation testing

## 🏃‍♂️ Getting Started

### Prerequisites
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** (17.8+) or **VS Code** with C# extensions
- **.NET Aspire workload** (install via: `dotnet workload install aspire`)

### Quick Start

1. **Navigate to the demo directory:**
   ```bash
   cd samples
   ```

2. **Launch the complete demo suite:**
   ```bash
   dotnet run --project Sannr.Demo.AppHost
   ```

3. **Access the services:**
   - **🌐 Web Frontend**: `http://localhost:15000` (Interactive validation testing)
   - **📊 Aspire Dashboard**: `http://localhost:15888` (Real-time metrics & observability)
   - **📡 API Service**: `http://localhost:15001` (REST API with OpenAPI docs)
   - **🏥 Health Checks**: `http://localhost:15001/health`

### Alternative: Run Individual Services

```bash
# Terminal 1: API Service
dotnet run --project Sannr.Demo.ApiService

# Terminal 2: Web Frontend
dotnet run --project Sannr.Demo.Web

# Terminal 3: App Host (Dashboard)
dotnet run --project Sannr.Demo.AppHost
```

## 🎮 Interactive Testing

### Web Interface Features
- **🔍 Real-time Validation** - See validation results instantly
- **📝 Form Testing** - Test all validation scenarios interactively
- **📊 Error Display** - Detailed error messages and field highlighting
- **⚡ Performance Metrics** - Validation timing and throughput display

### API Testing
Use the included `.http` files or tools like:
- **Postman** - Import collection from `Sannr.Demo.ApiService.http`
- **curl** - Command-line testing examples in documentation
- **Swagger UI** - Auto-generated from OpenAPI specifications

## 📊 Observability & Monitoring

### Aspire Dashboard Features
- **📈 Real-time Metrics** - Validation performance and throughput
- **🏥 Service Health** - Dependency monitoring and status
- **📊 Structured Logging** - Request/response tracing
- **⚡ Performance Insights** - Bottleneck identification

### Key Metrics Tracked
- Validation execution time per request
- Memory allocation patterns
- Error rates and validation failure types
- API response times and throughput

## 🏢 Enterprise Use Cases Demonstrated

### Financial Services
- Credit card validation with security requirements
- Transaction amount validation with business rules
- Anti-fraud validation patterns

### E-Commerce
- Product catalog validation with pricing rules
- Order processing with inventory checks
- Customer data sanitization and validation

### Healthcare
- Patient data validation with privacy requirements
- Appointment scheduling with business rules
- Medical record validation patterns

### SaaS Applications
- User registration with duplicate prevention
- Subscription management validation
- Multi-tenant data isolation rules

## 🔧 Development & Customization

### Adding New Validations
1. Define your model with Sannr attributes
2. Register validators in `Program.cs`
3. Test via API endpoints or web interface
4. Monitor performance via Aspire Dashboard

### Extending Business Rules
- Implement custom validation attributes
- Add domain-specific business logic
- Integrate with existing enterprise systems
- Leverage AOT compatibility for production deployment

## 📚 Learning Resources

### Documentation
- **API Reference** - Complete attribute and method documentation
- **Migration Guide** - Transitioning from DataAnnotations/FluentValidation
- **Performance Tuning** - Optimization techniques and best practices
- **Troubleshooting** - Common issues and solutions

### Sample Code
- **Model Definitions** - Comprehensive validation examples
- **API Endpoints** - REST API implementation patterns
- **Client Integration** - JavaScript validation integration
- **Monitoring Setup** - Observability configuration

## 🤝 Contributing

This demo suite serves as both a learning resource and a testing ground for new Sannr features. Contributions that enhance the validation showcase or add new enterprise scenarios are highly welcome.

## 📄 License

This demo suite is part of the Sannr validation framework, licensed under the MIT License. See the main repository for complete licensing information.

---

**Experience the future of .NET validation** - Where performance meets enterprise-grade reliability. Sannr delivers the speed your applications need with the robustness your business demands. ⚡✨
   - View real-time metrics and logs

4. **Explore the API:**
   - API documentation: `http://localhost:PORT/swagger`
   - Health check: `http://localhost:PORT/`
   - Validation metrics: `http://localhost:PORT/metrics/validation`

## 📋 API Endpoints

### Weather Forecast (Enhanced)
```http
GET /weatherforecast?days=7&location=NewYork&units=fahrenheit&includeDetails=true
```
**Validates:** Range (1-14 days), required location, enum units

### User Registration
```http
POST /api/users/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "age": 25,
  "phoneNumber": "+1-555-0123",
  "website": "https://johndoe.dev",
  "creditCard": "4111111111111111"
}
```
**Validates:** All fields with comprehensive rules, sanitization

### Profile Update (Conditional)
```http
PUT /api/users/{userId}/profile
Content-Type: application/json

{
  "email": "new.email@example.com",
  "currentPassword": "required_when_changing_sensitive_data"
}
```
**Validates:** Current password required only when changing email/phone

### Contact Form (Anti-Spam)
```http
POST /api/contact
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "subject": "Product Inquiry",
  "message": "I have a question about your product...",
  "honeypot": "" // Must be empty (anti-spam)
}
```
**Validates:** Anti-spam honeypot, comprehensive field validation

### Product Creation
```http
POST /api/products
Content-Type: application/json

{
  "name": "Wireless Headphones",
  "description": "High-quality wireless headphones",
  "price": 199.99,
  "stockQuantity": 50,
  "category": "Electronics",
  "imageFileName": "headphones.jpg",
  "isFeatured": true
}
```
**Validates:** File extensions, decimal ranges, business rules

### Order Placement (Complex)
```http
POST /api/orders
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440001",
      "quantity": 2,
      "unitPrice": 99.99
    }
  ],
  "shippingAddress": {
    "streetAddress": "123 Main St",
    "city": "Anytown",
    "state": "CA",
    "postalCode": "12345",
    "country": "USA"
  },
  "paymentMethod": "CreditCard",
  "creditCardDetails": {
    "cardNumber": "4111111111111111",
    "expirationMonth": 12,
    "expirationYear": 2025,
    "cvv": "123",
    "cardholderName": "John Doe"
  }
}
```
**Validates:** Nested objects, conditional credit card requirements

## 📊 Monitoring & Observability

### Aspire Dashboard Metrics
The demo automatically exposes metrics to the Aspire Dashboard:

- **Validation Performance:** Request counts, error rates, duration histograms
- **API Metrics:** Endpoint usage, response codes, throughput
- **System Metrics:** CPU, memory, GC statistics

### Custom Metrics Endpoint
```http
GET /metrics/validation
```
Returns real-time validation statistics and performance data.

## 🎯 Key Benefits Showcased

### Performance
- **15-20x faster** than DataAnnotations/FluentValidation
- **Zero reflection** in production code paths
- **Native AOT compatible** for optimal deployment

### Developer Experience
- **Automatic registration** - no manual validator setup
- **Rich error messages** with field names and validation rules
- **OpenAPI integration** - automatic schema documentation
- **Type safety** - compile-time validation rule verification

### Enterprise Features
- **Observability** - comprehensive metrics and monitoring
- **Scalability** - high-performance validation engine
- **Maintainability** - clean, declarative validation rules
- **Security** - input sanitization and validation

## 🧪 Testing Validation

Try these invalid inputs to see validation in action:

### Invalid User Registration
```json
{
  "username": "a",  // Too short
  "email": "invalid-email",  // Invalid format
  "age": 10,  // Too young
  "phoneNumber": "123"  // Invalid phone
}
```

### Invalid Order (Missing Credit Card)
```json
{
  "paymentMethod": "CreditCard",
  "creditCardDetails": null  // Required when payment method is CreditCard
}
```

## 📚 Learn More

- [Sannr Documentation](https://github.com/Digvijay/Sannr)
- [OpenAPI Specification](https://swagger.io/specification/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)

## 🤝 Contributing

This demo is part of the Sannr validation framework. Contributions welcome!

---

**Built with ❤️ using Sannr - The AOT-first validation engine for .NET**