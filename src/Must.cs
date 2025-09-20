using FUnitImpl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#pragma warning disable CA1050 // Declare types in namespaces
/// <summary>
/// Provides a set of assertion methods for FUnit tests.
/// </summary>
public static class Must
#pragma warning restore CA1050
{
    private const string INDENT = "  ";
    private const string NULL = "<null>";
    private static readonly char[] NewLineChars = new[] { '\n', '\r' };

    /// <summary>
    /// Asserts that two objects are equal. For collections, use <see cref="HaveSameSequence{T}"/> or <see cref="BeSameReference"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="actualExpr">The expression of the actual value, used for error reporting.</param>
    public static void BeEqual<T>(T expected, T actual, [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
    {
        if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T)))
        {
            throw new FUnitException(
                $"Ambiguous comparisons are not permitted in tests. Use '{nameof(HaveSameSequence)}' or '{nameof(BeSameReference)}' instead."
                );
        }

        actualExpr = string.IsNullOrWhiteSpace(actualExpr)
            ? null
            : $" ({actualExpr})";

        if (typeof(T) == typeof(string))
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                throw new FUnitException(
                    ((actual as string)?.Contains('\n') == true || (expected as string)?.Contains('\n') == true)
                        ? $"Expected and actual strings are not equal."
                        : $"Expected {(expected == null ? NULL : $"\"{expected}\"")}, but was {(actual == null ? NULL : $"\"{actual}\"")}.{actualExpr}"
                    );
            }
        }
        else
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                throw new FUnitException($"Expected '{expected?.ToString() ?? NULL}', but was '{actual?.ToString() ?? NULL}'.{actualExpr}");
            }
        }
    }

    /// <summary>
    /// Asserts that two object references point to the same instance.
    /// </summary>
    /// <param name="expected">The expected object instance.</param>
    /// <param name="actual">The actual object instance.</param>
    public static void BeSameReference(object expected, object actual)
    {
        if (!ReferenceEquals(actual, expected))
        {
            throw new FUnitException($"Expected both references to point to the same object, but found different instances.");
        }
    }


    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="conditionExpr">The expression of the condition, used for error reporting.</param>
    public static void BeTrue(bool condition, [CallerArgumentExpression(nameof(condition))] string? conditionExpr = null)
    {
        if (!condition)
        {
            throw new FUnitException(string.IsNullOrWhiteSpace(conditionExpr)
                ? "Expected condition to be true, but was false."
                : $"Expected condition '{conditionExpr}' to be met, but it was not."
            );
        }
    }


    /// <summary>
    /// Asserts that two sequences are equal and in the same order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequences.</typeparam>
    /// <param name="expected">The expected sequence.</param>
    /// <param name="actual">The actual sequence.</param>
    public static void HaveSameSequence<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new FUnitException(
$@"Expected collections to be equal in order.
{INDENT}Expected: [{string.Join(", ", expected.Select(x => x?.ToString() ?? NULL))}]
{INDENT}Actual:   [{string.Join(", ", actual.Select(x => x?.ToString() ?? NULL))}]"
                );
        }
    }

    /// <summary>
    /// Asserts that two collections contain the same elements, regardless of order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collections.</typeparam>
    /// <param name="expected">The expected collection.</param>
    /// <param name="actual">The actual collection.</param>
    public static void HaveSameUnorderedElements<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        if (expected.Count() != actual.Count())
        {
            goto THROW;
        }

        var checkList = new List<T>(expected);
        foreach (var x in actual)
        {
            if (!checkList.Remove(x))
            {
                goto THROW;
            }
        }

        if (checkList.Count != 0)
        {
            goto THROW;
        }

        return;

    THROW:
        throw new FUnitException(
$@"Expected collections to be equal ignoring order.
{INDENT}Expected: [{string.Join(", ", expected.Select(x => x?.ToString() ?? NULL))}]
{INDENT}Actual:   [{string.Join(", ", actual.Select(x => x?.ToString() ?? NULL))}]"
            );
    }


    /// <summary>
    /// Asserts that a string contains a specified substring.
    /// </summary>
    /// <param name="text">The string to search within.</param>
    /// <param name="substring">The substring to search for.</param>
    public static void ContainText(string text, string substring)
    {
        if (!text.Contains(substring, StringComparison.Ordinal))
        {
            throw new FUnitException(
                text.Contains('\n') || substring.Contains('\n')
                    ? $"Expected substring to be contained in source text, but it was not."
                    : $"Expected \"{text}\" to contain \"{substring}\", but it did not."
                );
        }
    }

    /// <summary>
    /// Asserts that a string does not contain a specified substring.
    /// </summary>
    /// <param name="text">The string to search within.</param>
    /// <param name="substring">The substring not to search for.</param>
    public static void NotContainText(string text, string substring)
    {
        if (text.Contains(substring, StringComparison.Ordinal))
        {
            throw new FUnitException(
                text.Contains('\n') || substring.Contains('\n')
                    ? $"Expected substring not to be contained in source text, but it was."
                    : $"Expected \"{text}\" not to contain \"{substring}\", but it did."
                );
        }
    }


    /// <summary>
    /// Asserts that a specific type of exception is thrown by an action.
    /// </summary>
    /// <typeparam name="TException">The type of the expected exception.</typeparam>
    /// <param name="test">The action to execute that is expected to throw an exception.</param>
    /// <param name="expectedMessage">The expected exception message. If null or empty, the message is not checked.</param>
    public static void Throw<TException>(string? expectedMessage, Action test)
        where TException : Exception
    {
        if (typeof(TException) == typeof(Exception))
        {
            throw new FUnitException("An explicit exception type must be specified.");
        }

        try
        {
            test.Invoke();
        }
        catch (Exception error)
        {
            string actualMessage = error.Message;

            if (error.GetType() != typeof(TException))
            {
                throw new FUnitException($@"Expected exception of type '{typeof(TException).Name}', but got '{error.GetType().Name}'.");
            }

            if (string.IsNullOrWhiteSpace(expectedMessage))
            {
                return;
            }

            if (actualMessage == expectedMessage)
            {
                return;
            }

            if (actualMessage.Contains('\n'))  // ummm.....
            {
                if (actualMessage.Split(NewLineChars)[0] == expectedMessage)
                {
                    return;
                }
            }

            throw new FUnitException($@"Expected error message to be ""{expectedMessage}"", but was ""{actualMessage}"".");
        }

        throw new FUnitException($@"Expected exception of type '{typeof(TException).Name}', but got none.");
    }
}
