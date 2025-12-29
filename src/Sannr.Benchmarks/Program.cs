using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.ComponentModel.DataAnnotations;
using Sannr;
using System.Threading.Tasks;
using DA = System.ComponentModel.DataAnnotations;

BenchmarkRunner.Run<ValidationBenchmarks>();

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private SimpleModel _simpleModel = null!;
    private ComplexModel _complexModel = null!;

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