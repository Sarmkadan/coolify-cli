#nullable enable

using CoolifyCli.Extensions;
using CoolifyCli.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoolifyCli.Tests
{
    /// <summary>
    /// Extension methods for testing enum extension methods.
    /// </summary>
    public static class EnumExtensionsTestsExtensions
    {
        /// <summary>
        /// Creates a test enum value from a string using ParseEnum with error handling.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The parsed enum value.</returns>
        /// <exception cref="ArgumentException">Thrown when parsing fails.</exception>
        public static TEnum ParseTestEnum<TEnum>(this string value) where TEnum : struct, Enum
        {
            return value.ParseEnum<TEnum>();
        }

        /// <summary>
        /// Safely tries to parse an enum value from a string.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The parsed enum value or null if parsing fails.</returns>
        public static TEnum? TryParseTestEnum<TEnum>(this string? value) where TEnum : struct, Enum
        {
            return value.TryParseEnum<TEnum>();
        }

        /// <summary>
        /// Gets all enum values and their display strings for testing.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>A dictionary mapping enum values to their display strings.</returns>
        public static Dictionary<TEnum, string> GetEnumValueDisplayMap<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            var map = new Dictionary<TEnum, string>();

            foreach (var value in values)
            {
                map[value] = value.ToDisplayString();
            }

            return map;
        }

        /// <summary>
        /// Verifies that all enum values have non-empty display strings.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>True if all display strings are non-empty; otherwise false.</returns>
        public static bool AllDisplayStringsNonEmpty<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            return values.All(v => !string.IsNullOrEmpty(v.ToDisplayString()));
        }

        /// <summary>
        /// Gets the underlying integer values for all enum members.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>A dictionary mapping enum values to their integer values.</returns>
        public static Dictionary<TEnum, int> GetEnumValueIntMap<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            var map = new Dictionary<TEnum, int>();

            foreach (var value in values)
            {
                map[value] = value.ToInt();
            }

            return map;
        }

        /// <summary>
        /// Gets the underlying long values for all enum members.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>A dictionary mapping enum values to their long values.</returns>
        public static Dictionary<TEnum, long> GetEnumValueLongMap<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            var map = new Dictionary<TEnum, long>();

            foreach (var value in values)
            {
                map[value] = value.ToLong();
            }

            return map;
        }

        /// <summary>
        /// Creates a test case for each enum value with its display string.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>An enumerable of tuples containing the enum value and its display string.</returns>
        public static IEnumerable<(TEnum Value, string Display)> GetEnumTestCases<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();

            foreach (var value in values)
            {
                yield return (value, value.ToDisplayString());
            }
        }

        /// <summary>
        /// Tests that enum values are in a specific order by their underlying integer values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="expectedOrder">The expected order of enum values.</param>
        /// <returns>True if the enum values are in the expected order; otherwise false.</returns>
        public static bool AreEnumValuesInOrder<TEnum>(this IEnumerable<TEnum> expectedOrder) where TEnum : struct, Enum
        {
            var actualOrder = EnumExtensions.GetAllValues<TEnum>();
            var actualValues = actualOrder.Select(v => v.ToInt()).ToList();
            var expectedValues = expectedOrder.Select(v => v.ToInt()).ToList();

            if (actualValues.Count != expectedValues.Count)
            {
                return false;
            }

            for (int i = 0; i < actualValues.Count; i++)
            {
                if (actualValues[i] != expectedValues[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the CLI format for all enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>A dictionary mapping enum values to their CLI format strings.</returns>
        public static Dictionary<TEnum, string> GetEnumCliFormatMap<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            var map = new Dictionary<TEnum, string>();

            foreach (var value in values)
            {
                map[value] = value.ToCliFormat();
            }

            return map;
        }

        /// <summary>
        /// Verifies that all enum values have unique CLI format strings.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <returns>True if all CLI format strings are unique; otherwise false.</returns>
        public static bool AllCliFormatsUnique<TEnum>() where TEnum : struct, Enum
        {
            var values = EnumExtensions.GetAllValues<TEnum>();
            var formats = values.Select(v => v.ToCliFormat()).ToList();

            return formats.Distinct().Count() == formats.Count;
        }
    }
}