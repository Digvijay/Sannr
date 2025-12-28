using FluentValidation;

public class TestValidator : AbstractValidator<TestModel>
{
    public TestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class TestModel
{
    public string Name { get; set; }
}