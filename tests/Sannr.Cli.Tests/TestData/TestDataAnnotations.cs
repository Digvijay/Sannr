using System.ComponentModel.DataAnnotations;

public partial class TestModel
{
    [Required]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
}