# CsvFormatterExtensions
The `CsvFormatterExtensions` type provides a set of extension methods for working with CSV data in C#. It offers functionality for formatting collections of objects into CSV strings, as well as parsing CSV content into lists of objects. This enables straightforward conversion between CSV data and .NET objects, simplifying tasks such as data import/export and serialization.

## API
* `public static string FormatCollection<T>(...)`: Formats a collection of objects of type `T` into a CSV string. The method takes a collection of `T` as a parameter and returns a string representing the CSV data. It throws an exception if the input collection is null.
* `public static string FormatCollection<T>(...)`: Overload of `FormatCollection` with different parameters, serving the same purpose but allowing for variations in formatting options.
* `public static string FormatCollectionWithoutHeader<T>(...)`: Similar to `FormatCollection`, but does not include the header row in the resulting CSV string.
* `public static List<T> ParseCsv<T>(this CsvFormatter formatter, string csvContent) where T : new()`: Parses a CSV string into a list of objects of type `T`. The method takes a `CsvFormatter` instance and a CSV string as parameters and returns a list of `T`. It throws an exception if the CSV content is malformed or if `T` cannot be instantiated.
* `public static string Format`: A method related to formatting, though its exact parameters and behavior are not detailed here due to the provided information.

## Usage
The following examples demonstrate how to use the `CsvFormatterExtensions` methods:
```csharp
// Example 1: Formatting a collection of objects
var users = new List<User> { new User("John", 25), new User("Alice", 30) };
var csv = CsvFormatterExtensions.FormatCollection(users);
Console.WriteLine(csv); // Output: "Name,Age\nJohn,25\nAlice,30"

// Example 2: Parsing CSV content into a list of objects
var csvContent = "Name,Age\nJohn,25\nAlice,30";
var formatter = new CsvFormatter();
var usersList = formatter.ParseCsv<User>(csvContent);
foreach (var user in usersList)
{
    Console.WriteLine($"Name: {user.Name}, Age: {user.Age}");
}
```

## Notes
When using `CsvFormatterExtensions`, consider the following:
- The `ParseCsv` method requires the type `T` to have a public parameterless constructor, as it instantiates objects of this type during parsing.
- The thread-safety of these methods depends on the implementation details not provided here. Generally, if the methods are stateless and do not access shared resources, they should be thread-safe.
- Edge cases such as handling quoted values, escaped characters, and different delimiter characters should be considered when working with CSV data. The `CsvFormatter` class and its extensions might provide options or settings to handle these cases.
