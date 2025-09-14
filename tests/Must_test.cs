#:project ../src
//#:package FUnit@*

using FUnitImpl;

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
            Must.Throw<FUnitException>(() => Must.BeEqual("hello", actualString), "Expected \"hello\", but was \"world\". (actualString)");
            Must.Throw<FUnitException>(() => Must.BeEqual(1, 1 + 1), "Expected '1', but was '2'. (1 + 1)");
            Must.Throw<FUnitException>(() => Must.BeEqual(true, 0 == 1), "Expected 'True', but was 'False'. (0 == 1)");
        });

        it("should throw FUnitException with generic message for string inequality when newlines are present", () =>
        {
            Must.Throw<FUnitException>(() => Must.BeEqual("hello\nworld", "foo bar"), "Expected and actual strings are not equal.");
            Must.Throw<FUnitException>(() => Must.BeEqual("hello world", "foo\nbar"), "Expected and actual strings are not equal.");
            Must.Throw<FUnitException>(() => Must.BeEqual("hello\nworld", "foo\nbar"), "Expected and actual strings are not equal.");
        });

        it("should throw Exception for ambiguous comparisons of IEnumerable", () =>
        {
            Must.Throw<FUnitException>(() => Must.BeEqual(new List<int> { 1 }, new List<int> { 1 }), "Ambiguous comparisons are not permitted in tests. Use 'HaveSameSequence' or 'BeSameReference' instead.");
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
            Must.Throw<FUnitException>(() => Must.BeSameReference(obj1, obj2), "Expected both references to point to the same object, but found different instances.");
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
            Must.Throw<FUnitException>(() => Must.HaveSameSequence(new List<int> { 1, 2, 3 }, new List<int> { 1, 3, 2 }), "Expected collections to be equal in order.");
            Must.Throw<FUnitException>(() => Must.HaveSameSequence(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }), "Expected collections to be equal in order.");
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
            Must.Throw<FUnitException>(() => Must.HaveSameUnorderedElements(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 4 }), "Expected collections to be equal ignoring order.");
            Must.Throw<FUnitException>(() => Must.HaveSameUnorderedElements(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }), "Expected collections to be equal ignoring order.");
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
            Must.Throw<FUnitException>(() => Must.ContainText("hello world", "foo"), "Expected \"hello world\" to contain \"foo\", but it did not.");
        });

        it("should throw FUnitException with generic message if text or substring contain newlines and substring is not found", () =>
        {
            Must.Throw<FUnitException>(() => Must.ContainText("hello world", "foo\nbar"), "Expected substring to be contained in source text, but it was not.");
            Must.Throw<FUnitException>(() => Must.ContainText("hello\nworld", "foo bar"), "Expected substring to be contained in source text, but it was not.");
            Must.Throw<FUnitException>(() => Must.ContainText("hello\nworld", "foo\nbar"), "Expected substring to be contained in source text, but it was not.");
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
            Must.Throw<FUnitException>(() => Must.NotContainText("hello world", "world"), "Expected \"hello world\" not to contain \"world\", but it did.");
        });

        it("should throw FUnitException with generic message if text or substring contain newlines and substring is found", () =>
        {
            Must.Throw<FUnitException>(() => Must.NotContainText("hello\nworld", "\nworld"), "Expected substring not to be contained in source text, but it was.");
            Must.Throw<FUnitException>(() => Must.NotContainText("hello\nworld", "world"), "Expected substring not to be contained in source text, but it was.");
        });
    });

    describe("Must.Throw", it =>
    {
        it("should assert that a specific exception is thrown", () =>
        {
            Must.Throw<InvalidOperationException>(() => throw new InvalidOperationException("Test exception"), "Test exception");
        });

        it("should throw FUnitException if no exception is thrown", () =>
        {
            Must.Throw<FUnitException>(() => Must.Throw<ArgumentNullException>(() => { /* no exception */ }, null), "Expected exception of type 'ArgumentNullException', but got none.");
        });

        it("should throw FUnitException if a different exception type is thrown", () =>
        {
            Must.Throw<FUnitException>(() => Must.Throw<ArgumentNullException>(() => throw new InvalidOperationException(), null), "Expected exception of type 'ArgumentNullException', but got 'InvalidOperationException'.");
        });

        it("should throw FUnitException if the error message does not match", () =>
        {
            Must.Throw<FUnitException>(() => Must.Throw<InvalidOperationException>(() => throw new InvalidOperationException("Wrong message"), "Expected message"), "Expected error message to be \"Expected message\", but was \"Wrong message\".");
        });

        it("should throw FUnitException when a general Exception type is specified for Throw<T>", () =>
        {
            Must.Throw<FUnitException>(() => Must.Throw<Exception>(() => throw new Exception("General exception"), "General exception"), "An explicit exception type must be specified.");
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
            Must.Throw<FUnitException>(() => Must.BeTrue(false), "Expected condition 'false' to be met, but it was not.");
            Must.Throw<FUnitException>(() => Must.BeTrue(0 == 1), "Expected condition '0 == 1' to be met, but it was not.");
        });
    });

    describe("Must.BeFalse", it =>
    {
        it("should assert that a condition is false", () =>
        {
            Must.BeFalse(false);
        });

        it("should throw FUnitException if the condition is true", () =>
        {
            Must.Throw<FUnitException>(() => Must.BeFalse(true), "Expected condition 'true' not to be met, but it was.");
            Must.Throw<FUnitException>(() => Must.BeFalse(0 == 0), "Expected condition '0 == 0' not to be met, but it was.");
        });
    });
});
