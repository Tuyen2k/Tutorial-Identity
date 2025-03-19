using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;

namespace TutorialIdentity.Pages;

public class IndexModel : PageModel {
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger) {
        _logger = logger;
    }

    public void OnGet() {
        Load("blue", 1, 2, 4, 5, 6, 72, 123, 1212,21);
        Load("red", 999, [1, 22, 45, 123, 13]);
    }

    public void Load(string color, int number, params int[] numbers) {
        if (color.ToLower() == "red") {
            Console.ForegroundColor = ConsoleColor.Red;
        } else if (color.ToLower() == "blue") {
            Console.ForegroundColor = ConsoleColor.Blue;
        }
        // Add more colors as needed
        Console.WriteLine("--------------");
        Console.WriteLine($"The color is {number}");
        numbers.ToList().ForEach(n => Console.WriteLine(n));

        // Reset the color to the default
        Console.ResetColor();
    }
}
