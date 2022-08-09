# ValidationHelper
The `ValidationHelper` class provides a set of static methods for validating various types of input data, including strings, numbers, and semantic versions. These methods can be used to ensure that data conforms to expected formats and ranges, helping to prevent errors and exceptions in applications. The class includes methods for checking the validity of IDs, strings, emails, URLs, IP addresses, hostnames, ports, database names, usernames, and semantic versions, as well as methods for checking if a value is within a specified range or matches a given pattern.

## API
* `public static bool IsValidId`: Checks if a given ID is valid. Parameters: none. Return value: `true` if the ID is valid, `false` otherwise. Throws: none.
* `public static bool IsValidString`: Checks if a given string is valid. Parameters: none. Return value: `true` if the string is valid, `false` otherwise. Throws: none.
* `public static bool HasMinLength`: Checks if a given string has a minimum length. Parameters: the string to check, the minimum length. Return value: `true` if the string has the minimum length, `false` otherwise. Throws: none.
* `public static bool HasMaxLength`: Checks if a given string has a maximum length. Parameters: the string to check, the maximum length. Return value: `true` if the string has the maximum length, `false` otherwise. Throws: none.
* `public static bool HasLengthBetween`: Checks if a given string has a length between a minimum and maximum value. Parameters: the string to check, the minimum length, the maximum length. Return value: `true` if the string has a length between the minimum and maximum, `false` otherwise. Throws: none.
* `public static bool MatchesPattern`: Checks if a given string matches a specified pattern. Parameters: the string to check, the pattern. Return value: `true` if the string matches the pattern, `false` otherwise. Throws: none.
* `public static bool IsValidEmail`: Checks if a given email address is valid. Parameters: the email address to check. Return value: `true` if the email address is valid, `false` otherwise. Throws: none.
* `public static bool IsValidUrl`: Checks if a given URL is valid. Parameters: the URL to check. Return value: `true` if the URL is valid, `false` otherwise. Throws: none.
* `public static bool IsValidIpAddress`: Checks if a given IP address is valid. Parameters: the IP address to check. Return value: `true` if the IP address is valid, `false` otherwise. Throws: none.
* `public static bool IsValidHostname`: Checks if a given hostname is valid. Parameters: the hostname to check. Return value: `true` if the hostname is valid, `false` otherwise. Throws: none.
* `public static bool IsValidPort`: Checks if a given port number is valid. Parameters: the port number to check. Return value: `true` if the port number is valid, `false` otherwise. Throws: none.
* `public static bool IsValidDatabaseName`: Checks if a given database name is valid. Parameters: the database name to check. Return value: `true` if the database name is valid, `false` otherwise. Throws: none.
* `public static bool IsValidUsername`: Checks if a given username is valid. Parameters: the username to check. Return value: `true` if the username is valid, `false` otherwise. Throws: none.
* `public static bool IsOneOf`: Checks if a given value is one of a specified set of values. Parameters: the value to check, the set of values. Return value: `true` if the value is one of the specified values, `false` otherwise. Throws: none.
* `public static bool IsInRange`: Checks if a given value is within a specified range. Parameters: the value to check, the minimum value, the maximum value. Return value: `true` if the value is within the range, `false` otherwise. Throws: none.
* `public static bool IsNotEmpty<T>`: Checks if a given collection is not empty. Parameters: the collection to check. Return value: `true` if the collection is not empty, `false` otherwise. Throws: none.
* `public static bool HasMinimumCount<T>`: Checks if a given collection has a minimum count. Parameters: the collection to check, the minimum count. Return value: `true` if the collection has the minimum count, `false` otherwise. Throws: none.
* `public static bool HasMaximumCount<T>`: Checks if a given collection has a maximum count. Parameters: the collection to check, the maximum count. Return value: `true` if the collection has the maximum count, `false` otherwise. Throws: none.
* `public static bool IsValidSemanticVersion`: Checks if a given semantic version is valid. Parameters: the semantic version to check. Return value: `true` if the semantic version is valid, `false` otherwise. Throws: none.

## Usage
The following example demonstrates how to use the `ValidationHelper` class to validate a username and email address:
```csharp
string username = "johnDoe";
string email = "johndoe@example.com";

if (ValidationHelper.IsValidUsername(username) && ValidationHelper.IsValidEmail(email))
{
    Console.WriteLine("Username and email are valid");
}
else
{
    Console.WriteLine("Username or email is invalid");
}
```
The following example demonstrates how to use the `ValidationHelper` class to validate a collection of items:
```csharp
List<string> items = new List<string> { "item1", "item2", "item3" };

if (ValidationHelper.IsNotEmpty(items) && ValidationHelper.HasMinimumCount(items, 2))
{
    Console.WriteLine("Collection is not empty and has at least 2 items");
}
else
{
    Console.WriteLine("Collection is empty or has less than 2 items");
}
```

## Notes
The `ValidationHelper` class is designed to be thread-safe, as all methods are static and do not modify any shared state. However, it is still possible for concurrent calls to the same method to interfere with each other if they are modifying external state. Additionally, some methods may throw exceptions if the input parameters are null or invalid, so it is recommended to check for these conditions before calling the methods. Edge cases, such as very long strings or very large numbers, may also cause issues if not handled properly. It is recommended to test the `ValidationHelper` class thoroughly to ensure it meets the specific requirements of your application.
