#:project ../src
//#:package FUnit@*

using FUnitImpl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#warning THIS WARNING IS EMITTED BY PREPROCESSOR DIRECTIVE

return FUnit.Run(args, describe =>
{
    describe("Must.BeEqual", it =>
    {
        it("should assert equality for value types", () =>
        {
            Must.BeEqual(1, 1);
            Must.BeEqual("hello", "hello");
            Must.BeEqual(true, true);
        });

        it("should throw FUnitException for inequality of value types", () =>
        {
            string actualString = "world";
            Must.Throw<FUnitException>("Expected \"hello\", but was \"world\". (actualString)", () => Must.BeEqual("hello", actualString));
            Must.Throw<FUnitException>("Expected '1', but was '2'. (1 + 1)", () => Must.BeEqual(1, 1 + 1));
            Must.Throw<FUnitException>("Expected 'True', but was 'False'. (0 == 1)", () => Must.BeEqual(true, 0 == 1));
        });

        it("should throw FUnitException with generic message for string inequality when newlines are present", () =>
        {
            Must.Throw<FUnitException>("Expected and actual strings are not equal.", () => Must.BeEqual("hello\nworld", "foo bar"));
            Must.Throw<FUnitException>("Expected and actual strings are not equal.", () => Must.BeEqual("hello world", "foo\nbar"));
            Must.Throw<FUnitException>("Expected and actual strings are not equal.", () => Must.BeEqual("hello\nworld", "foo\nbar"));
        });

        it("should throw Exception for ambiguous comparisons of IEnumerable", () =>
        {
            Must.Throw<FUnitException>("Ambiguous comparisons are not permitted in tests. Use 'HaveSameSequence' or 'BeSameReference' instead.", () => Must.BeEqual(new List<int> { 1 }, new List<int> { 1 }));
        });
    });

    describe("Must.NotBeEqual", it =>
    {
        it("should assert inequality for value types", () =>
        {
            Must.NotBeEqual(1, 2);
            Must.NotBeEqual("hello", "world");
            Must.NotBeEqual(true, false);
        });

        it("should throw FUnitException for equality of value types", () =>
        {
            string actualString = "hello";
            Must.Throw<FUnitException>("Expected not to be \"hello\", but was \"hello\". (actualString)", () => Must.NotBeEqual("hello", actualString));
            Must.Throw<FUnitException>("Expected not to be '1', but was '1'. (1)", () => Must.NotBeEqual(1, 1));
            Must.Throw<FUnitException>("Expected not to be 'True', but was 'True'. (true)", () => Must.NotBeEqual(true, true));
        });

        it("should throw FUnitException with generic message for string equality when newlines are present", () =>
        {
            Must.Throw<FUnitException>("Expected and actual strings are equal.", () => Must.NotBeEqual("hello\nworld", "hello\nworld"));
        });

        it("should throw Exception for ambiguous comparisons of IEnumerable", () =>
        {
            Must.Throw<FUnitException>("Ambiguous comparisons are not permitted in tests. Use 'NotHaveSameSequence' or 'NotBeSameReference' instead.", () => Must.NotBeEqual(new List<int> { 1 }, new List<int> { 2 }));
        });
    });

    describe("Must.BeSameReference", it =>
    {
        it("should assert reference equality", () =>
        {
            object obj1 = new object();
            object obj2 = obj1;
            Must.BeSameReference(obj1, obj2);
        });

        it("should throw FUnitException for reference inequality", () =>
        {
            object obj1 = new object();
            object obj2 = new object();
            Must.Throw<FUnitException>("Expected both references to point to the same object, but found different instances.", () => Must.BeSameReference(obj1, obj2));
        });
    });

    describe("Must.NotBeSameReference", it =>
    {
        it("should assert reference inequality", () =>
        {
            object obj1 = new object();
            object obj2 = new object();
            Must.NotBeSameReference(obj1, obj2);
        });

        it("should throw FUnitException for reference equality", () =>
        {
            object obj1 = new object();
            Must.Throw<FUnitException>("Expected references to point to different objects, but both pointed to the same instance.", () => Must.NotBeSameReference(obj1, obj1));
        });
    });

    describe("Must.BeTrue", it =>
    {
        it("should assert that a condition is true", () =>
        {
            Must.BeTrue(true);
        });

        it("should throw FUnitException if the condition is false", () =>
        {
            Must.Throw<FUnitException>("Expected condition 'false' to be met, but it was not.", () => Must.BeTrue(false));
            Must.Throw<FUnitException>("Expected condition '0 == 1' to be met, but it was not.", () => Must.BeTrue(0 == 1));
        });
    });

    describe("Must.HaveSameSequence", it =>
    {
        it("should assert sequence equality", () =>
        {
            Must.HaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 3 });
            Must.HaveSameSequence(new string[] { "a", "b" }, new string[] { "a", "b" });
        });

        it("should throw FUnitException for sequence inequality", () =>
        {
            Must.Throw<FUnitException>("Expected collections to be equal in order.", () => Must.HaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 3, 2 }));
            Must.Throw<FUnitException>("Expected collections to be equal in order.", () => Must.HaveSameSequence(new List<int> { 1, 3, 2 }, new List<int> { 1, 2, 3 }));
            Must.Throw<FUnitException>("Expected collections to be equal in order.", () => Must.HaveSameSequence(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }));
            Must.Throw<FUnitException>("Expected collections to be equal in order.", () => Must.HaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 2 }));
        });
    });

    describe("Must.NotHaveSameSequence", it =>
    {
        it("should assert sequence inequality", () =>
        {
            Must.NotHaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 3, 2 });
            Must.NotHaveSameSequence(new string[] { "a", "b" }, new string[] { "b", "a" });
            Must.NotHaveSameSequence(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 });
        });

        it("should throw FUnitException for sequence equality", () =>
        {
            Must.Throw<FUnitException>("Expected collections to not be equal in order.", () => Must.NotHaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 3 }));
            Must.Throw<FUnitException>("Expected collections to not be equal in order.", () => Must.NotHaveSameSequence(new string[] { "a", "b" }, new string[] { "a", "b" }));
        });
    });

    describe("Must.HaveSameUnorderedElements", it =>
    {
        it("should assert unordered sequence equality", () =>
        {
            Must.HaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 3, 1, 2 });
            Must.HaveSameUnorderedElements(new string[] { "a", "b" }, new string[] { "b", "a" });
        });

        it("should throw FUnitException for unordered sequence inequality", () =>
        {
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 4 }));
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1, 2, 4 }, new List<int> { 1, 2, 3 }));
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }));
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 1, 2 }));
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1 }, new List<int> { 1, 1 }));
            Must.Throw<FUnitException>("Expected collections to be equal ignoring order.", () => Must.HaveSameUnorderedElements(new List<int> { 1, 1 }, new List<int> { 1 }));
        });
    });

    describe("Must.NotHaveSameUnorderedElements", it =>
    {
        it("should assert unordered sequence inequality", () =>
        {
            Must.NotHaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 4 });
            Must.NotHaveSameUnorderedElements(new string[] { "a", "b" }, new string[] { "a", "c" });
            Must.NotHaveSameUnorderedElements(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 });
        });

        it("should throw FUnitException for unordered sequence equality", () =>
        {
            Must.Throw<FUnitException>("Expected collections to not be equal ignoring order.", () => Must.NotHaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 3, 1, 2 }));
            Must.Throw<FUnitException>("Expected collections to not be equal ignoring order.", () => Must.NotHaveSameUnorderedElements(new string[] { "a", "b" }, new string[] { "b", "a" }));
        });
    });

    describe("Must.ContainText", it =>
    {
        it("should assert that text contains substring", () =>
        {
            Must.ContainText("hello world", "world");
        });

        it("should throw FUnitException if text does not contain substring", () =>
        {
            Must.Throw<FUnitException>("Expected \"hello world\" to contain \"foo\", but it did not.", () => Must.ContainText("hello world", "foo"));
        });

        it("should throw FUnitException with generic message if text or substring contain newlines and substring is not found", () =>
        {
            Must.Throw<FUnitException>("Expected substring to be contained in source text, but it was not.", () => Must.ContainText("hello world", "foo\nbar"));
            Must.Throw<FUnitException>("Expected substring to be contained in source text, but it was not.", () => Must.ContainText("hello\nworld", "foo bar"));
            Must.Throw<FUnitException>("Expected substring to be contained in source text, but it was not.", () => Must.ContainText("hello\nworld", "foo\nbar"));
        });
    });

    describe("Must.NotContainText", it =>
    {
        it("should assert that text does not contain substring", () =>
        {
            Must.NotContainText("hello world", "foo");
        });

        it("should throw FUnitException if text contains substring", () =>
        {
            Must.Throw<FUnitException>("Expected \"hello world\" not to contain \"world\", but it did.", () => Must.NotContainText("hello world", "world"));
        });

        it("should throw FUnitException with generic message if text or substring contain newlines and substring is found", () =>
        {
            Must.Throw<FUnitException>("Expected substring not to be contained in source text, but it was.", () => Must.NotContainText("hello\nworld", "\nworld"));
            Must.Throw<FUnitException>("Expected substring not to be contained in source text, but it was.", () => Must.NotContainText("hello\nworld", "world"));
        });
    });

    describe("Must.Throw<T>", it =>
    {
        it("should assert that a specific exception is thrown", () =>
        {
            Must.Throw<InvalidOperationException>("Test exception", () => throw new InvalidOperationException("Test exception"));
        });

        it("should throw FUnitException if no exception is thrown", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw<ArgumentNullException>(null, () => { /* no exception */ }));
        });

        it("should throw FUnitException if a different exception type is thrown", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw<ArgumentNullException>(null, () => throw new InvalidOperationException()));
        });

        it("should throw FUnitException if the error message does not match", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw<InvalidOperationException>("Expected message", () => throw new InvalidOperationException("Wrong message")));
        });

        it("should throw FUnitException when a general Exception type is specified for Throw<T>", () =>
        {
            Must.Throw<FUnitException>("An explicit exception type must be specified.", () => Must.Throw<Exception>("General exception", () => throw new Exception("General exception")));
        });
    });

    describe("Must.Throw", it =>
    {
        it("should assert that a specific exception is thrown using non-generic Throw", () =>
        {
            Must.Throw("System.InvalidOperationException", "Test exception", () => throw new InvalidOperationException("Test exception"));
        });

        it("should throw FUnitException if type name is invalid for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Could not find exception type 'Invalid.Type.Name'.", () => Must.Throw("Invalid.Type.Name", null, () => { }));
        });

        it("should throw FUnitException if type name is not an exception for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Type 'System.String' is not an exception type.", () => Must.Throw("System.String", null, () => { }));
        });

        it("should throw FUnitException if no exception is thrown for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw("System.ArgumentNullException", null, () => { /* no exception */ }));
        });

        it("should throw FUnitException if a different exception type is thrown for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw("System.ArgumentNullException", null, () => throw new InvalidOperationException()));
        });

        it("should throw FUnitException if the error message does not match for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw("System.InvalidOperationException", "Expected message", () => throw new InvalidOperationException("Wrong message")));
        });
    });

    const int DELAY = 100;

    describe("Must.Throw<T> (async)", it =>
    {
        it("should assert that a specific exception is thrown by an async ValueTask", () =>
        {
            Must.Throw<InvalidOperationException>("Test exception", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Test exception"); });
        });

        it("should assert that a specific exception is thrown by an async Task", () =>
        {
            Must.Throw<InvalidOperationException>("Test exception", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Test exception"); });
        });

        it("should throw FUnitException if no exception is thrown by async ValueTask", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw<ArgumentNullException>(null, async () => { await Task.Delay(DELAY); /* no exception */ }));
        });

        it("should throw FUnitException if no exception is thrown by async Task", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw<ArgumentNullException>(null, async () => { await Task.Delay(DELAY); /* no exception */ }));
        });

        it("should throw FUnitException if a different exception type is thrown by async ValueTask", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw<ArgumentNullException>(null, async () => { await Task.Delay(DELAY); throw new InvalidOperationException(); }));
        });

        it("should throw FUnitException if a different exception type is thrown by async Task", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw<ArgumentNullException>(null, async () => { await Task.Delay(DELAY); throw new InvalidOperationException(); }));
        });

        it("should throw FUnitException if the error message does not match for async ValueTask", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw<InvalidOperationException>("Expected message", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Wrong message"); }));
        });

        it("should throw FUnitException if the error message does not match for async Task", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw<InvalidOperationException>("Expected message", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Wrong message"); }));
        });
    });

    describe("Must.Throw (async, non-generic)", it =>
    {
        it("should assert that a specific exception is thrown by an async ValueTask using non-generic Throw", () =>
        {
            Must.Throw("System.InvalidOperationException", "Test exception", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Test exception"); });
        });

        it("should assert that a specific exception is thrown by an async Task using non-generic Throw", () =>
        {
            Must.Throw("System.InvalidOperationException", "Test exception", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Test exception"); });
        });

        it("should throw FUnitException if no exception is thrown by async ValueTask for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw("System.ArgumentNullException", null, async () => { await Task.Delay(DELAY); /* no exception */ }));
        });

        it("should throw FUnitException if no exception is thrown by async Task for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got none.", () => Must.Throw("System.ArgumentNullException", null, async () => { await Task.Delay(DELAY); /* no exception */ }));
        });

        it("should throw FUnitException if a different exception type is thrown by async ValueTask for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw("System.ArgumentNullException", null, async () => { await Task.Delay(DELAY); throw new InvalidOperationException(); }));
        });

        it("should throw FUnitException if a different exception type is thrown by async Task for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.", () => Must.Throw("System.ArgumentNullException", null, async () => { await Task.Delay(DELAY); throw new InvalidOperationException(); }));
        });

        it("should throw FUnitException if the error message does not match for async ValueTask for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw("System.InvalidOperationException", "Expected message", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Wrong message"); }));
        });

        it("should throw FUnitException if the error message does not match for async Task for non-generic Throw", () =>
        {
            Must.Throw<FUnitException>("Expected error message to be \"Expected message\", but was \"Wrong message\".", () => Must.Throw("System.InvalidOperationException", "Expected message", async () => { await Task.Delay(DELAY); throw new InvalidOperationException("Wrong message"); }));
        });
    });

    describe("Must Collection Assertion Overload Ambiguity", it =>
    {
        it("HaveSameSequence should work with various combinations (Array, IEnumerable, Span, ReadOnlySpan)", () =>
        {
            int[] arr = new[] { 1, 2, 3 };
            List<int> list = new() { 1, 2, 3 };
            ReadOnlySpan<int> ros = new[] { 1, 2, 3 };
            Span<int> span = new int[] { 1, 2, 3 };

            Must.HaveSameSequence(arr, arr);
            Must.HaveSameSequence(arr, list);
            Must.HaveSameSequence(list, arr);
            Must.HaveSameSequence(arr, ros);
            Must.HaveSameSequence(ros, arr);
            Must.HaveSameSequence(ros, ros);
            Must.HaveSameSequence(span, ros);
        });

        it("NotHaveSameSequence should work with various combinations", () =>
        {
            int[] arr1 = new[] { 1, 2, 3 };
            int[] arr2 = new[] { 3, 2, 1 };
            List<int> list2 = new() { 3, 2, 1 };
            ReadOnlySpan<int> ros1 = new[] { 1, 2, 3 };
            ReadOnlySpan<int> ros2 = new[] { 3, 2, 1 };
            Span<int> span1 = new int[] { 1, 2, 3 };

            Must.NotHaveSameSequence(arr1, arr2);
            Must.NotHaveSameSequence(arr1, list2);
            Must.NotHaveSameSequence(list2, arr1);
            Must.NotHaveSameSequence(arr1, ros2);
            Must.NotHaveSameSequence(ros1, arr2);
            Must.NotHaveSameSequence(ros1, ros2);
            Must.NotHaveSameSequence(span1, ros2);
        });

        it("HaveSameUnorderedElements should work with various combinations", () =>
        {
            int[] arr1 = new[] { 1, 2, 3 };
            int[] arr2 = new[] { 3, 2, 1 };
            List<int> list2 = new() { 3, 2, 1 };
            ReadOnlySpan<int> ros1 = new[] { 1, 2, 3 };
            ReadOnlySpan<int> ros2 = new[] { 3, 2, 1 };
            Span<int> span1 = new int[] { 1, 2, 3 };

            Must.HaveSameUnorderedElements(arr1, arr2);
            Must.HaveSameUnorderedElements(arr1, list2);
            Must.HaveSameUnorderedElements(list2, arr1);
            Must.HaveSameUnorderedElements(arr1, ros2);
            Must.HaveSameUnorderedElements(ros1, arr2);
            Must.HaveSameUnorderedElements(ros1, ros2);
            Must.HaveSameUnorderedElements(span1, ros2);
        });

        it("NotHaveSameUnorderedElements should work with various combinations", () =>
        {
            int[] arr1 = new[] { 1, 2, 3 };
            int[] arr2 = new[] { 1, 2, 4 };
            List<int> list2 = new() { 1, 2, 4 };
            ReadOnlySpan<int> ros1 = new[] { 1, 2, 3 };
            ReadOnlySpan<int> ros2 = new[] { 1, 2, 4 };
            Span<int> span1 = new int[] { 1, 2, 3 };

            Must.NotHaveSameUnorderedElements(arr1, arr2);
            Must.NotHaveSameUnorderedElements(arr1, list2);
            Must.NotHaveSameUnorderedElements(list2, arr1);
            Must.NotHaveSameUnorderedElements(arr1, ros2);
            Must.NotHaveSameUnorderedElements(ros1, arr2);
            Must.NotHaveSameUnorderedElements(ros1, ros2);
            Must.NotHaveSameUnorderedElements(span1, ros2);
        });
    });
});
