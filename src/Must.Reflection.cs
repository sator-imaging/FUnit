// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

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
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="propertyNamesToSkip">Optional. An array of property names to skip during the comparison.</param>
    /// <param name="logger">Optional. A logger action to output comparison details.</param>
    public static void HaveEqualProperties<TExpected, TActual>(
        TExpected expected,
        TActual actual,
        string[]? propertyNamesToSkip = null,
        Action<string>? logger = null
    )
        where TActual : TExpected
    {
        DeepEquals(
            expected, actual, depth: 0, typeof(TExpected).IsAbstract || typeof(TActual).IsAbstract,
            propertyOrFieldPath: "$", compareByProperty: true, propertyNamesToSkip, logger
        );
    }

    /// <summary>
    /// Asserts that two objects have equal fields, performing a deep comparison.
    /// </summary>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="fieldNamesToSkip">Optional. An array of field names to skip during the comparison.</param>
    /// <param name="logger">Optional. A logger action to output comparison details.</param>
    public static void HaveEqualFields<TExpected, TActual>(
        TExpected expected,
        TActual actual,
        string[]? fieldNamesToSkip = null,
        Action<string>? logger = null
    )
        where TActual : TExpected
    {
        DeepEquals(
            expected, actual, depth: 0, typeof(TExpected).IsAbstract || typeof(TActual).IsAbstract,
            propertyOrFieldPath: "$", compareByProperty: false, fieldNamesToSkip, logger
        );
    }


    static void DeepEquals<T>(
        T expected,
        T actual,
        int depth,
        bool isAbstractType,
        string propertyOrFieldPath,
        bool compareByProperty,
        string[]? propertyOrFieldNamesToSkip,
        Action<string>? logger
    )
    {
        const string IndentForSystemMessage = "  ";

        var indent = new string(' ', (depth * 2) + 2);

        if (expected == null || actual == null)
        {
            BeTrue(expected is null && actual is null, $"{propertyOrFieldPath} == null");

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
            BeEqual(expected, actual, propertyOrFieldPath);

            logger?.Invoke($"{indent}--> {typedef}: '{expected}' == '{actual}'");
            return;
        }

        // must check right after primitive value comparison
        if (depth > byte.MaxValue)
        {
            logger?.Invoke($"{IndentForSystemMessage}[SKIP] Traversal depth limit exceeded: {depth}");
            return;
        }
        ++depth;

        // collections (DO NOT return)
        if (expected is IDictionary expectedMapUntyped &&
            actual is IDictionary actualMapUntyped)
        {
            logger?.Invoke($"{indent}IDictionary ({propertyOrFieldPath})");

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
                BeTrue(actualMap.ContainsKey(key), $"{propertyOrFieldPath} has required key: {key}");
            }
            // actual should not have key that is not in expected
            foreach (var key in actualMap.Keys)
            {
                BeTrue(expectedMap.ContainsKey(key), $"{propertyOrFieldPath} doesn't have unnecessary key: {key}");
            }

            // both dictionary keys are checked so don't need to check count.

            foreach (var key in expectedMap.Keys)
            {
                // key existence has already been tested
                expectedMap.Remove(key, out var expectedValue);
                actualMap.Remove(key, out var actualValue);

                DeepEquals(expectedValue, actualValue, depth,
                    expectedValue?.GetType().IsAbstract == true || actualValue?.GetType().IsAbstract == true,
                    $"{propertyOrFieldPath}[{key}]", compareByProperty, propertyOrFieldNamesToSkip, logger
                );
            }
        }
        else if (
            expected is not string and IEnumerable expectedEnumerable &&
            actual is not string and IEnumerable actualEnumerable)
        {
            logger?.Invoke($"{indent}IEnumerable ({propertyOrFieldPath})");

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

            BeTrue(expectedList.Count == actualList.Count, $"{propertyOrFieldPath} has {expectedList.Count} items");

            for (int i = 0, count = Math.Min(expectedList.Count, actualList.Count); i < count; i++)
            {
                DeepEquals(expectedList[i], actualList[i], depth,
                    expectedList[i]?.GetType().IsAbstract == true || actualList[i]?.GetType().IsAbstract == true,
                    $"{propertyOrFieldPath}[{i}]", compareByProperty, propertyOrFieldNamesToSkip, logger
                );
            }
        }

        // properties or fields
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
                logger?.Invoke($"{indent}[SKIP] Compiler generated member: {member.Name} ({typedef})");
                continue;
            }

            // indexer (this[]) is always skipped
            if (member is PropertyInfo prop && prop.GetIndexParameters().Length > 0)
            {
                logger?.Invoke($"{indent}[SKIP] Indexer: {member.Name} ({typedef})");
                continue;
            }

            // ignore Capacity (internal buffer size doesn't matter)
            if (member.Name == "Capacity" && typedef.FullName != null)
            {
                if (typedef.FullName.StartsWith("System.Collections.Generic.Dictionary`2", StringComparison.Ordinal) ||
                    typedef.FullName.StartsWith("System.Collections.Generic.List`1", StringComparison.Ordinal))
                {
                    logger?.Invoke($"{indent}[SKIP] Capacity: {member.Name} ({typedef})");
                    continue;
                }
            }

            object? E;
            object? A;
            try
            {
                E = compareByProperty ? ((PropertyInfo)member).GetValue(expected) : ((FieldInfo)member).GetValue(expected);
                A = compareByProperty ? ((PropertyInfo)member).GetValue(actual) : ((FieldInfo)member).GetValue(actual);
            }
            catch (TargetException) when (isAbstractType)
            {
                logger?.Invoke($"{indent}[SKIP] Actual type of abstractions are different: {member.Name} ({typedef})");
                continue;
            }
            catch (Exception error)
            {
                throw new FUnitException($"{member.Name} ({typedef}): {error.Message}", error);
            }

            // reference may be pointing itself
            if (ReferenceEquals(E, expected) ||
                ReferenceEquals(A, actual) ||
                // explicitly ignore SyncRoot to avoid indefinitely loop
                // --> IDictionary -> IEnumerable -> IDictionary -> IEnumerable...
                member.Name == "System.Collections.ICollection.SyncRoot"
            )
            {
                logger?.Invoke($"{indent}[SKIP] Reference value is pointing itself: {member.Name} ({typedef})");
                continue;
            }

            checkedValueCount++;
            logger?.Invoke($"{indent}{member.Name}");

            var memberFullPath = $"{propertyOrFieldPath}.{member.Name}";

            // main
            DeepEquals(
                E, A, depth,
                (
                    (member as PropertyInfo)?.PropertyType.IsAbstract == true ||
                    (member as FieldInfo)?.FieldType.IsAbstract == true
                ),
                memberFullPath, compareByProperty, propertyOrFieldNamesToSkip, logger
            );

            continue;

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
                logger?.Invoke($"{IndentForSystemMessage}No property found: {typedef}");
            }
        }
    }
}
