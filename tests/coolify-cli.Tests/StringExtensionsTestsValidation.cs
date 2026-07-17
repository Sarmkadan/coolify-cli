#nullable enable

using CoolifyCli.Extensions;
using System;
using System.Collections.Generic;

/// <summary>
/// Validation helpers for test data used with <see cref="StringExtensions"/> extension methods.
/// Provides validation for parameters that would be passed to extension methods like ToPascalCase, Truncate, MaskSensitive, and SplitTrimmed.
/// </summary>
public static class StringExtensionsTestsValidation
{
	/// <summary>
	/// Validates test data for <see cref="StringExtensions.ToPascalCase(string?)"/> method.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="expectedResult">The expected PascalCase result.</param>
	/// <returns>A list of human-readable validation problems, or empty if valid.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="expectedResult"/> is <see langword="null"/></exception>
	public static IReadOnlyList<string> ValidateToPascalCase(string input, string expectedResult)
	{
		ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
		ArgumentException.ThrowIfNullOrEmpty(expectedResult, nameof(expectedResult));

		var problems = new List<string>();

		if (string.IsNullOrWhiteSpace(input))
		{
			problems.Add("Input string cannot be null or whitespace for ToPascalCase test");
		}

		if (string.IsNullOrWhiteSpace(expectedResult))
		{
			problems.Add("Expected result cannot be null or whitespace for ToPascalCase test");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates test data for <see cref="StringExtensions.Truncate(string?, int, bool)"/> method when input exceeds max length.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="maxLength">The maximum allowed length (must be positive).</param>
	/// <param name="expectedResult">The expected truncated result.</param>
	/// <returns>A list of human-readable validation problems, or empty if valid.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative</exception>
	public static IReadOnlyList<string> ValidateTruncateExceedsMax(string input, int maxLength, string expectedResult)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(maxLength, nameof(maxLength));
		ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
		ArgumentException.ThrowIfNullOrEmpty(expectedResult, nameof(expectedResult));

		var problems = new List<string>();

		if (maxLength > input.Length)
		{
			problems.Add("maxLength should be less than or equal to input length for meaningful Truncate test");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates test data for <see cref="StringExtensions.Truncate(string?, int, bool)"/> method when input is within max length.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="maxLength">The maximum allowed length (must be positive and &gt;= input length).</param>
	/// <param name="expectedResult">The expected result (should equal input).</param>
	/// <returns>A list of human-readable validation problems, or empty if valid.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative</exception>
	public static IReadOnlyList<string> ValidateTruncateWithinMax(string input, int maxLength, string expectedResult)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(maxLength, nameof(maxLength));
		ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
		ArgumentException.ThrowIfNullOrEmpty(expectedResult, nameof(expectedResult));

		var problems = new List<string>();

		if (maxLength < input.Length)
		{
			problems.Add("maxLength should be greater than or equal to input length for TruncateWithinMaxLength test");
		}

		if (expectedResult != input)
		{
			problems.Add("Expected result should equal input for TruncateWithinMaxLength test");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates test data for <see cref="StringExtensions.MaskSensitive(string?, int)"/> method.
	/// </summary>
	/// <param name="input">The input string (API key) to validate.</param>
	/// <param name="showChars">Number of characters to show at start and end (must be non-negative).</param>
	/// <param name="expectedResult">The expected masked result.</param>
	/// <returns>A list of human-readable validation problems, or empty if valid.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="showChars"/> is negative</exception>
	public static IReadOnlyList<string> ValidateMaskSensitive(string input, int showChars, string expectedResult)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(showChars, nameof(showChars));
		ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));
		ArgumentException.ThrowIfNullOrEmpty(expectedResult, nameof(expectedResult));

		var problems = new List<string>();

		if (showChars * 2 >= input.Length)
		{
			problems.Add("showChars should be small enough to actually mask some characters for MaskSensitive test");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates test data for <see cref="StringExtensions.SplitTrimmed(string, params char[])"/> method.
	/// </summary>
	/// <param name="input">The input string with whitespace-padded parts.</param>
	/// <param name="separator">The separator character.</param>
	/// <param name="expectedResult">The expected split and trimmed result.</param>
	/// <returns>A list of human-readable validation problems, or empty if valid.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="expectedResult"/> is <see langword="null"/></exception>
	public static IReadOnlyList<string> ValidateSplitTrimmed(string input, char separator, string[] expectedResult)
	{
		ArgumentNullException.ThrowIfNull(expectedResult, nameof(expectedResult));
		ArgumentException.ThrowIfNullOrEmpty(input, nameof(input));

		var problems = new List<string>();

		if (expectedResult.Length == 0)
		{
			problems.Add("Expected result array cannot be empty for SplitTrimmed test");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the ToPascalCase test data is valid.
	/// </summary>
	/// <param name="input">The input string to check.</param>
	/// <param name="expectedResult">The expected PascalCase result.</param>
	/// <returns>True if the test data is valid; otherwise, false.</returns>
	public static bool IsValidToPascalCase(string input, string expectedResult)
	{
		return ValidateToPascalCase(input, expectedResult).Count == 0;
	}

	/// <summary>
	/// Determines whether the Truncate test data (exceeds max length) is valid.
	/// </summary>
	/// <param name="input">The input string to check.</param>
	/// <param name="maxLength">The maximum allowed length.</param>
	/// <param name="expectedResult">The expected truncated result.</param>
	/// <returns>True if the test data is valid; otherwise, false.</returns>
	public static bool IsValidTruncateExceedsMax(string input, int maxLength, string expectedResult)
	{
		return ValidateTruncateExceedsMax(input, maxLength, expectedResult).Count == 0;
	}

	/// <summary>
	/// Determines whether the Truncate test data (within max length) is valid.
	/// </summary>
	/// <param name="input">The input string to check.</param>
	/// <param name="maxLength">The maximum allowed length.</param>
	/// <param name="expectedResult">The expected result.</param>
	/// <returns>True if the test data is valid; otherwise, false.</returns>
	public static bool IsValidTruncateWithinMax(string input, int maxLength, string expectedResult)
	{
		return ValidateTruncateWithinMax(input, maxLength, expectedResult).Count == 0;
	}

	/// <summary>
	/// Determines whether the MaskSensitive test data is valid.
	/// </summary>
	/// <param name="input">The input string (API key) to check.</param>
	/// <param name="showChars">Number of characters to show at start and end.</param>
	/// <param name="expectedResult">The expected masked result.</param>
	/// <returns>True if the test data is valid; otherwise, false.</returns>
	public static bool IsValidMaskSensitive(string input, int showChars, string expectedResult)
	{
		return ValidateMaskSensitive(input, showChars, expectedResult).Count == 0;
	}

	/// <summary>
	/// Determines whether the SplitTrimmed test data is valid.
	/// </summary>
	/// <param name="input">The input string to check.</param>
	/// <param name="separator">The separator character.</param>
	/// <param name="expectedResult">The expected split and trimmed result.</param>
	/// <returns>True if the test data is valid; otherwise, false.</returns>
	public static bool IsValidSplitTrimmed(string input, char separator, string[] expectedResult)
	{
		return ValidateSplitTrimmed(input, separator, expectedResult).Count == 0;
	}

	/// <summary>
	/// Ensures that the ToPascalCase test data is valid.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="expectedResult">The expected PascalCase result.</param>
	/// <exception cref="ArgumentException">Thrown if test data is invalid with a list of problems.</exception>
	public static void EnsureValidToPascalCase(string input, string expectedResult)
	{
		var problems = ValidateToPascalCase(input, expectedResult);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"ToPascalCase test data is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
		}
	}

	/// <summary>
	/// Ensures that the Truncate test data (exceeds max length) is valid.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="maxLength">The maximum allowed length.</param>
	/// <param name="expectedResult">The expected truncated result.</param>
	/// <exception cref="ArgumentException">Thrown if test data is invalid with a list of problems.</exception>
	public static void EnsureValidTruncateExceedsMax(string input, int maxLength, string expectedResult)
	{
		var problems = ValidateTruncateExceedsMax(input, maxLength, expectedResult);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"Truncate test data (exceeds max length) is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
		}
	}

	/// <summary>
	/// Ensures that the Truncate test data (within max length) is valid.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="maxLength">The maximum allowed length.</param>
	/// <param name="expectedResult">The expected result.</param>
	/// <exception cref="ArgumentException">Thrown if test data is invalid with a list of problems.</exception>
	public static void EnsureValidTruncateWithinMax(string input, int maxLength, string expectedResult)
	{
		var problems = ValidateTruncateWithinMax(input, maxLength, expectedResult);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"Truncate test data (within max length) is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
		}
	}

	/// <summary>
	/// Ensures that the MaskSensitive test data is valid.
	/// </summary>
	/// <param name="input">The input string (API key) to validate.</param>
	/// <param name="showChars">Number of characters to show at start and end.</param>
	/// <param name="expectedResult">The expected masked result.</param>
	/// <exception cref="ArgumentException">Thrown if test data is invalid with a list of problems.</exception>
	public static void EnsureValidMaskSensitive(string input, int showChars, string expectedResult)
	{
		var problems = ValidateMaskSensitive(input, showChars, expectedResult);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"MaskSensitive test data is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
		}
	}

	/// <summary>
	/// Ensures that the SplitTrimmed test data is valid.
	/// </summary>
	/// <param name="input">The input string to validate.</param>
	/// <param name="separator">The separator character.</param>
	/// <param name="expectedResult">The expected split and trimmed result.</param>
	/// <exception cref="ArgumentException">Thrown if test data is invalid with a list of problems.</exception>
	public static void EnsureValidSplitTrimmed(string input, char separator, string[] expectedResult)
	{
		var problems = ValidateSplitTrimmed(input, separator, expectedResult);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"SplitTrimmed test data is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
		}
	}
}