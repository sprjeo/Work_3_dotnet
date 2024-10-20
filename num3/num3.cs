using System;
using ValidationLibrary;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var validator = new ValidatorBuilder<string>()
                .AddRule(data => string.IsNullOrEmpty(data) ? "Value cannot be empty." : null)
                .AddRule(data => data.Length < 5 ? "Value must be at least 5 characters long." : null)
                .Build();

            var testData = new[] { "fjfjffjfjfjfjfj", "abchello", "a" };

            foreach (var data in testData)
            {
                Console.WriteLine($"Validating: '{data}'");
                validator.Validate(data);
                Console.WriteLine("Validation passed.");
            }
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Validation failed with the following errors:");
            foreach (var inner in ex.InnerExceptions)
            {
                Console.WriteLine($"- {inner.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
