using FluentValidation;

public partial class TestValidator : AbstractValidator<TestModel>
{
    public TestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public partial class TestModel
{
    public string Name { get; set; }
}