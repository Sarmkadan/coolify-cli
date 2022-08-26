using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoolifyCli.Tests
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="ValidationHelperTests"/>.
    /// </summary>
    public static class ValidationHelperTestsExtensions
    {
        /// <summary>
        /// Returns the names of all public instance test methods on <see cref="ValidationHelperTests"/>
        /// that follow the naming convention used in this project (methods that start with "IsValid").
        /// </summary>
        /// <param name="helper">The test instance to inspect.</param>
        /// <returns>An enumerable of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helper"/> is null.</exception>
        public static IEnumerable<string> GetTestMethodNames(this ValidationHelperTests helper)
        {
            ArgumentNullException.ThrowIfNull(helper);

            return helper.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("IsValid", StringComparison.Ordinal))
                .Select(m => m.Name);
        }

        /// <summary>
        /// Executes all validation test methods on the supplied <see cref="ValidationHelperTests"/> instance.
        /// Any exception thrown by an individual test is collected and re‑thrown as an <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="helper">The test instance whose methods should be executed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helper"/> is null.</exception>
        /// <exception cref="AggregateException">Thrown when one or more tests fail, containing the collected exceptions.</exception>
        public static void RunAllValidationTests(this ValidationHelperTests helper)
        {
            ArgumentNullException.ThrowIfNull(helper);

            var exceptions = new List<Exception>();

            foreach (var method in helper.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.StartsWith("IsValid", StringComparison.Ordinal) && m.GetParameters().Length == 0))
            {
                try
                {
                    method.Invoke(helper, null);
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    // Capture the actual exception thrown by the test method.
                    exceptions.Add(tie.InnerException);
                }
                catch (Exception ex) when (ex is not null)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("One or more validation tests failed.", exceptions);
            }
        }

        /// <summary>
        /// Determines whether a test method with the specified name exists on <see cref="ValidationHelperTests"/>.
        /// </summary>
        /// <param name="helper">The test instance to inspect.</param>
        /// <param name="methodName">The name of the method to find.</param>
        /// <returns>True if the method exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helper"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="methodName"/> is null or whitespace.</exception>
        public static bool HasTestMethod(this ValidationHelperTests helper, string methodName)
        {
            ArgumentNullException.ThrowIfNull(helper);
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));

            return helper.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                is not null;
        }
    }
}