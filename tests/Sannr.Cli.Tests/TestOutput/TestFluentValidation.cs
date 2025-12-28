using Sannr;

using FluentValidation;

public class TestValidator : AbstractValidator<TestModel>
{
    public TestValidator()
    {
            // TODO: Convert to Sannr attributes on model properties
            // Original FluentValidation: RuleFor(x => x.Name).NotEmpty();
            // See migration guide: https://github.com/your-repo/sannr/migration
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class TestModel
{
    public string Name { get; set; }
}

            /*
             * MIGRATION NOTES:
             * 1. Move validation rules from validator classes to model properties as attributes
             * 2. Convert RuleFor(x => x.Name).NotEmpty() to [Required] on Name property
             * 3. Convert RuleFor(x => x.Name).Length(2, 50) to [StringLength(50)] on Name property
             * 4. Convert RuleFor(x => x.Email).EmailAddress() to [Email] on Email property
             * 5. Remove this validator class after migrating all rules
             */