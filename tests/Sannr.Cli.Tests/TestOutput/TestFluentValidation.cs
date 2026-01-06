using Sannr;

using FluentValidation;

public partial class TestValidator : AbstractValidator<TestModel>
{
    public TestValidator()
    {
            // TODO: Convert to Sannr fluent rules if necessary, or move to attributes
            // Original FluentValidation: RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public partial class TestModel
{
    public string Name { get; set; }
}