using System;
using System.Collections.Generic;

return FUnit.Run(args, describe =>
{
    describe("Must Collection Assertion Overload Ambiguity", it =>
    {
        it("HaveSameSequence should work with arrays (T[] vs T[])", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            int[] actual = new[] { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameSequence should work with array vs List (T[] vs IEnumerable<T>)", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            List<int> actual = new() { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameSequence should work with List vs array (IEnumerable<T> vs T[])", () =>
        {
            List<int> expected = new() { 1, 2, 3 };
            int[] actual = new[] { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameSequence should work with array vs ReadOnlySpan (T[] vs ReadOnlySpan<T>)", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            ReadOnlySpan<int> actual = new[] { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameSequence should work with ReadOnlySpan vs array (ReadOnlySpan<T> vs T[])", () =>
        {
            ReadOnlySpan<int> expected = new[] { 1, 2, 3 };
            int[] actual = new[] { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameSequence should work with ReadOnlySpan vs ReadOnlySpan", () =>
        {
            ReadOnlySpan<int> expected = new[] { 1, 2, 3 };
            ReadOnlySpan<int> actual = new[] { 1, 2, 3 };
            Must.HaveSameSequence(expected, actual);
        });

        it("HaveSameUnorderedElements should work with arrays", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            int[] actual = new[] { 3, 2, 1 };
            Must.HaveSameUnorderedElements(expected, actual);
        });

        it("NotHaveSameSequence should work with arrays", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            int[] actual = new[] { 3, 2, 1 };
            Must.NotHaveSameSequence(expected, actual);
        });

        it("NotHaveSameUnorderedElements should work with arrays", () =>
        {
            int[] expected = new[] { 1, 2, 3 };
            int[] actual = new[] { 1, 2, 4 };
            Must.NotHaveSameUnorderedElements(expected, actual);
        });
    });
});
