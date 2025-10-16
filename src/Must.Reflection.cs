using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FUnitImpl;

#pragma warning disable CA1050 // Declare types in namespaces
partial class Must
#pragma warning restore CA1050
{
    /// <summary>
    /// Asserts that two objects have equal properties, performing a deep comparison.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="propertyNamesToSkip">Optional. An array of property names to skip during the comparison.</param>
    /// <param name="logger">Optional. A logger action to output comparison details.</param>
    public static void HaveEqualProperties<T>(
        T expected,
        T actual,
        string[]? propertyNamesToSkip = null,
        Action<string>? logger = null
    )
    {
        DeepEquals(expected, actual, depth: 0, propertyOrFieldName: "$", compareByProperty: true, propertyNamesToSkip, logger);
    }

    /// <summary>
    /// Asserts that two objects have equal fields, performing a deep comparison.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="fieldNamesToSkip">Optional. An array of field names to skip during the comparison.</param>
    /// <param name="logger">Optional. A logger action to output comparison details.</param>
    public static void HaveEqualFields<T>(
        T expected,
        T actual,
        string[]? fieldNamesToSkip = null,
        Action<string>? logger = null
    )
    {
        DeepEquals(expected, actual, depth: 0, propertyOrFieldName: "$", compareByProperty: false, fieldNamesToSkip, logger);
    }


    static void DeepEquals<T>(
        T expected,
        T actual,
        int depth,
        string propertyOrFieldName,
        bool compareByProperty,
        string[]? propertyOrFieldNamesToSkip,
        Action<string>? logger
    )
    {
        var indent = new string(' ', (depth * 2) + 2);

        if (expected == null || actual == null)
        {
            BeTrue(expected is null && actual is null, $"{propertyOrFieldName} == null");

            logger?.Invoke($"{indent}--> null == null");
            return;
        }

        // DO NOT check by typeof(T). it is always 'object'.
        var typedef = expected.GetType();
        if (typedef.IsPrimitive ||
            typedef.IsEnum ||
            expected is string ||
            (typedef.IsValueType && typedef.Namespace == "System"))
        {
            BeEqual(expected, actual, propertyOrFieldName);

            logger?.Invoke($"{indent}--> {typedef}: '{expected}' == '{actual}'");
            return;
        }

        if (depth > byte.MaxValue)
        {
            (logger ?? Console.Error.WriteLine).Invoke($"  [SKIP] Traversal depth limit exceeded: {depth}");
            return;
        }
        ++depth;

        int checkedValueCount = 0;
        foreach (MemberInfo member in compareByProperty
            ? typedef.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty)
            : typedef.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OfType<MemberInfo>())
        {
            if (propertyOrFieldNamesToSkip?.Contains(member.Name) == true)
            {
                continue;
            }

            // for record types
            if (member.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
            {
                logger?.Invoke($"{indent}[SKIP] compiler generated member: {member.Name} ({typedef})");
                continue;
            }

            // indexer (this[]) is always skipped
            if (member is PropertyInfo prop && prop.GetIndexParameters().Length > 0)
            {
                logger?.Invoke($"{indent}[SKIP] indexer: {member.Name} ({typedef})");
                continue;
            }

            object? E;
            object? A;
            try
            {
                E = compareByProperty ? ((PropertyInfo)member).GetValue(expected) : ((FieldInfo)member).GetValue(expected);
                A = compareByProperty ? ((PropertyInfo)member).GetValue(actual) : ((FieldInfo)member).GetValue(actual);
            }
            catch (Exception error)
            {
                throw new FUnitException($"{member.Name} ({typedef}): {error.Message}", error);
            }

            // SyncRoot may be pointing itself
            if (ReferenceEquals(E, expected) ||
                ReferenceEquals(A, actual))
            {
                logger?.Invoke($"{indent}[SKIP] reference value is pointing itself: {member.Name} ({typedef})");
                continue;
            }

            checkedValueCount++;
            logger?.Invoke($"{indent}{member.Name}");

            var memberFullPath = $"{propertyOrFieldName}.{member.Name}";

            // main
            if (E is IDictionary expectedMapUntyped &&
                A is IDictionary actualMapUntyped)
            {
                var expectedMap = new Dictionary<object, object?>(expectedMapUntyped.Count);
                foreach (DictionaryEntry kv in expectedMapUntyped)
                {
                    expectedMap.Add((((kv.Key!))), kv.Value);
                }

                var actualMap = new Dictionary<object, object?>(actualMapUntyped.Count);
                foreach (DictionaryEntry kv in actualMapUntyped)
                {
                    actualMap.Add((((kv.Key!))), kv.Value);
                }

                // check keys separately for better error message
                foreach (var key in expectedMap.Keys)
                {
                    BeTrue(actualMap.ContainsKey(key), $"{memberFullPath} has key: {key}");
                }
                // actual should not have key that is not in expected
                foreach (var key in actualMap.Keys)
                {
                    BeTrue(expectedMap.ContainsKey(key), $"{memberFullPath} doesn't have key: {key}");
                }

                // both dictionary keys are checked so don't need to check count.

                foreach (var key in expectedMap.Keys)
                {
                    // key existence has already been tested
                    expectedMap.Remove(key, out var expectedValue);
                    actualMap.Remove(key, out var actualValue);

                    DeepEquals(expectedValue, actualValue, depth, $"{memberFullPath}[{key}]", compareByProperty, propertyOrFieldNamesToSkip, logger);
                }

                continue;
            }
            else if (
                E is not string and IEnumerable expectedEnumerable &&
                A is not string and IEnumerable actualEnumerable)
            {
                var expectedList = new List<object?>();
                foreach (var x in expectedEnumerable)
                {
                    expectedList.Add(x);
                }

                var actualList = new List<object?>();
                foreach (var x in actualEnumerable)
                {
                    actualList.Add(x);
                }

                BeTrue(expectedList.Count == actualList.Count, $"{expectedList.Count} items in {memberFullPath}");

                for (int i = 0, count = Math.Min(expectedList.Count, actualList.Count); i < count; i++)
                {
                    DeepEquals(expectedList[i], actualList[i], depth, $"{memberFullPath}[{i}]", compareByProperty, propertyOrFieldNamesToSkip, logger);
                }

                continue;
            }
            else
            {
                DeepEquals(E, A, depth, memberFullPath, compareByProperty, propertyOrFieldNamesToSkip, logger);
                continue;
            }

            throw new FUnitException($"cannot compare object: expected: '{E}', actual: '{A}'");
        }

        // last check
        if (checkedValueCount == 0)
        {
            if (typedef.FullName is not string ||
                (
                    !typedef.FullName.StartsWith("System.Collections.Generic.GenericEqualityComparer`1", StringComparison.Ordinal) &&
                    !typedef.FullName.StartsWith("System.Collections.Generic.StringEqualityComparer", StringComparison.Ordinal)
                ))
            {
                (logger ?? Console.Error.WriteLine).Invoke($"  No property found: {typedef}");
            }
        }
    }
}
