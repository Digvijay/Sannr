using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.ComponentModel.DataAnnotations;
using Sannr;
using System.Threading.Tasks;
using DA = System.ComponentModel.DataAnnotations;
using FluentValidation;

BenchmarkRunner.Run<ValidationBenchmarks>();

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private SimpleModel _simpleModel = null!;
    private ComplexModel _complexModel = null!;
    private SimpleModelValidator _simpleValidator = null!;
    private ComplexModelValidator _complexValidator = null!;
    // Sannr Fluent Validators
    private SannrFluentSimpleValidator _sannrFluentSimpleValidator = null!;
    private SannrFluentComplexValidator _sannrFluentComplexValidator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleModel = new SimpleModel { Name = "John", Age = 25, Email = "john@example.com" };
        _complexModel = new ComplexModel
        {
            Name = "John Doe", Age = 30, Email = "john@example.com",
            Address = "123 Main St", City = "Anytown", ZipCode = "12345",
            Phone = "555-1234", Country = "USA", Website = "https://example.com",
            Salary = 50000, Department = "IT", Manager = "Jane Smith",
            HireDate = DateTime.Now, IsActive = true, Notes = "Good employee"
        };

        // Initialize FluentValidation validators
        _simpleValidator = new SimpleModelValidator();
        _complexValidator = new ComplexModelValidator();

        // Initialize Sannr Fluent validators
        _sannrFluentSimpleValidator = new SannrFluentSimpleValidator();
        _sannrFluentComplexValidator = new SannrFluentComplexValidator();

        // Register Sannr validators
        SannrValidatorRegistry.Register<SimpleModel>(async context =>
        {
            var result = new Sannr.ValidationResult();
            var model = (SimpleModel)context.ObjectInstance;
            if (string.IsNullOrEmpty(model.Name)) result.Add("Name", "Name is required");
            if (model.Age < 18) result.Add("Age", "Age must be 18+");
            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@")) result.Add("Email", "Invalid email");
            return result;
        });

        SannrValidatorRegistry.Register<ComplexModel>(async context =>
        {
            var result = new Sannr.ValidationResult();
            var model = (ComplexModel)context.ObjectInstance;
            // Add all 15 field validations here...
            if (string.IsNullOrEmpty(model.Name)) result.Add("Name", "Name is required");
            if (model.Age < 18) result.Add("Age", "Age must be 18+");
            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@")) result.Add("Email", "Invalid email");
            if (string.IsNullOrEmpty(model.Address)) result.Add("Address", "Address is required");
            if (string.IsNullOrEmpty(model.City)) result.Add("City", "City is required");
            if (string.IsNullOrEmpty(model.ZipCode)) result.Add("ZipCode", "ZipCode is required");
            if (string.IsNullOrEmpty(model.Phone)) result.Add("Phone", "Phone is required");
            if (string.IsNullOrEmpty(model.Country)) result.Add("Country", "Country is required");
            if (string.IsNullOrEmpty(model.Website) || !Uri.IsWellFormedUriString(model.Website, UriKind.Absolute)) result.Add("Website", "Invalid website");
            if (model.Salary < 0) result.Add("Salary", "Salary must be positive");
            if (string.IsNullOrEmpty(model.Department)) result.Add("Department", "Department is required");
            if (string.IsNullOrEmpty(model.Manager)) result.Add("Manager", "Manager is required");
            if (model.HireDate == default) result.Add("HireDate", "HireDate is required");
            if (!model.IsActive) result.Add("IsActive", "Must be active");
            if (string.IsNullOrEmpty(model.Notes)) result.Add("Notes", "Notes are required");
            return result;
        });
    }

    [Benchmark]
    public async Task Sannr_SimpleModel()
    {
        var context = new SannrValidationContext(_simpleModel);
        if (SannrValidatorRegistry.TryGetValidator(typeof(SimpleModel), out var validator) && validator != null)
        {
            await validator(context);
        }
    }

    [Benchmark]
    public void DataAnnotations_SimpleModel()
    {
        var context = new ValidationContext(_simpleModel);
        Validator.ValidateObject(_simpleModel, context, true);
    }

    [Benchmark]
    public async Task Sannr_ComplexModel()
    {
        var context = new SannrValidationContext(_complexModel);
        if (SannrValidatorRegistry.TryGetValidator(typeof(ComplexModel), out var validator) && validator != null)
        {
            await validator(context);
        }
    }

    [Benchmark]
    public void DataAnnotations_ComplexModel()
    {
        var context = new ValidationContext(_complexModel);
        Validator.ValidateObject(_complexModel, context, true);
    }

    [Benchmark]
    public async Task Sannr_AsyncValidation()
    {
        // For async, we'll simulate a simple async check
        var context = new SannrValidationContext(_simpleModel);
        if (SannrValidatorRegistry.TryGetValidator(typeof(SimpleModel), out var validator) && validator != null)
        {
            await validator(context);
        }
    }

    [Benchmark]
    public async Task FluentValidation_SimpleModel()
    {
        await _simpleValidator.ValidateAsync(_simpleModel);
    }

    [Benchmark]
    public async Task FluentValidation_ComplexModel()
    {
        await _complexValidator.ValidateAsync(_complexModel);
    }

    [Benchmark]
    public async Task SannrFluent_SimpleModel()
    {
        await _sannrFluentSimpleValidator.ValidateAsync(_simpleModel);
    }

    [Benchmark]
    public async Task SannrFluent_ComplexModel()
    {
        await _sannrFluentComplexValidator.ValidateAsync(_complexModel);
    }
}

// Models
public class SimpleModel
{
    [DA.Required]
    public string Name { get; set; }

    [DA.Range(18, 120)]
    public int Age { get; set; }

    [DA.EmailAddress]
    public string Email { get; set; }
}

public class ComplexModel
{
    [DA.Required]
    public string Name { get; set; }

    [DA.Range(18, 120)]
    public int Age { get; set; }

    [DA.EmailAddress]
    public string Email { get; set; }

    [DA.Required]
    public string Address { get; set; }

    [DA.Required]
    public string City { get; set; }

    [DA.Required]
    public string ZipCode { get; set; }

    [DA.Phone]
    public string Phone { get; set; }

    [DA.Required]
    public string Country { get; set; }

    [DA.Url]
    public string Website { get; set; }

    [DA.Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    [DA.Required]
    public string Department { get; set; }

    [DA.Required]
    public string Manager { get; set; }

    [DA.Required]
    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; }

    [DA.Required]
    public string Notes { get; set; }
}

// FluentValidation Validators
public class SimpleModelValidator : FluentValidation.AbstractValidator<SimpleModel>
{
    public SimpleModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be 18+");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email");
    }
}

public class ComplexModelValidator : FluentValidation.AbstractValidator<ComplexModel>
{
    public ComplexModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be 18+");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
        RuleFor(x => x.ZipCode).NotEmpty().WithMessage("ZipCode is required");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required");
        RuleFor(x => x.Website).NotEmpty().Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Invalid website");
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).WithMessage("Salary must be positive");
        RuleFor(x => x.Department).NotEmpty().WithMessage("Department is required");
        RuleFor(x => x.Manager).NotEmpty().WithMessage("Manager is required");
        RuleFor(x => x.HireDate).NotEmpty().WithMessage("HireDate is required");
        RuleFor(x => x.IsActive).Equal(true).WithMessage("Must be active");
        RuleFor(x => x.Notes).NotEmpty().WithMessage("Notes are required");
    }
}

// Sannr Fluent Validators (using Sannr's AbstractValidator)
public class SannrFluentSimpleValidator : global::Sannr.AbstractValidator<SimpleModel>
{
    public SannrFluentSimpleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be 18+");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email");
    }
}

public class SannrFluentComplexValidator : global::Sannr.AbstractValidator<ComplexModel>
{
    public SannrFluentComplexValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be 18+");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
        RuleFor(x => x.ZipCode).NotEmpty().WithMessage("ZipCode is required");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required");
        RuleFor(x => x.Website).NotEmpty().Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Invalid website");
        RuleFor(x => x.Salary).InclusiveBetween(0, int.MaxValue).WithMessage("Salary must be positive");
        RuleFor(x => x.Department).NotEmpty().WithMessage("Department is required");
        RuleFor(x => x.Manager).NotEmpty().WithMessage("Manager is required");
        RuleFor(x => x.HireDate).NotEmpty().WithMessage("HireDate is required");
        RuleFor(x => x.IsActive).Must(active => active == true).WithMessage("Must be active");
        RuleFor(x => x.Notes).NotEmpty().WithMessage("Notes are required");
    }
}