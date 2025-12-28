using Sannr;
using System.ComponentModel.DataAnnotations;

public class TestModel
{
    [Required]
    public string Name { get; set; }

    [Email]
    public string Email { get; set; }
}