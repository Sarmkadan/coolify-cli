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
        public static IEnumerable<string> GetTestMethodNames(this ValidationHelperTests helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));

            return helper.GetType()
                         .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                         .Where(m => m.Name.StartsWith("IsValid", StringComparison.Ordinal))
                         .Select(m => m.Name);
        }

        /// <summary>
        /// Executes all validation test methods on the supplied <see cref="ValidationHelperTests"/> instance.
        /// Any exception thrown by an individual test is collected and re‑thrown as an <see cref="AggregateException"/>.
        /// </summary>
        public static void RunAllValidationTests(this ValidationHelperTests helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));

            var exceptions = new List<Exception>();

            foreach (var method in helper.GetType()
                                         .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                         .Where(m => m.Name.StartsWith("IsValid", StringComparison.Ordinal) && m.GetParameters().Length == 0))
            {
                try
                {
                    method.Invoke(helper, null);
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    // Capture the actual exception thrown by the test method.
                    exceptions.Add(tie.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException("One or more validation tests failed.", exceptions);
        }

        /// <summary>
        /// Determines whether a test method with the specified name exists on <see cref="ValidationHelperTests"/>.
        /// </summary>
        public static bool HasTestMethod(this ValidationHelperTests helper, string methodName)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException("Method name must be provided.", nameof(methodName));

            return helper.GetType()
                         .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly) != null;
        }
    }
}
