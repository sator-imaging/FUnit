#:project ../src

using FUnitImpl;

#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable IDE0300 // Simplify collection initialization

return FUnit.Run(args, describe =>
{
    describe("Must.HaveEqualProperties", it =>
    {
        it("should pass when properties are equal", () =>
        {
            var expected = new Wrapper<TestClass>(new TestClass { Id = 1, Name = "Test" });
            var actual = new Wrapper<TestClass>(new TestClass { Id = 1, Name = "Test" });
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when properties are not equal", () =>
        {
            var expected = new Wrapper<TestClass>(new TestClass { Id = 1, Name = "Test" });
            var actual = new Wrapper<TestClass>(new TestClass { Id = 2, Name = "Test" });
            Must.Throw<FUnitException>("Expected '1', but was '2'. ($.Data.Id)", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when properties are equal and one is skipped", () =>
        {
            var expected = new Wrapper<TestClass>(new TestClass { Id = 1, Name = "Test" });
            var actual = new Wrapper<TestClass>(new TestClass { Id = 999, Name = "Test" });
            Must.HaveEqualProperties(expected, actual, new[] { "Id" });
        });

        it("should pass when null properties are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckPrp = null });
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckPrp = null });
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when null properties are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckPrp = null });
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckPrp = "not null" });
            Must.Throw<FUnitException>("Expected condition '$.Data.NullCheckPrp == null' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when MapPrp values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapPrp.Add("key1", 1);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapPrp.Add("key1", 1);
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when MapPrp values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapPrp.Add("key1", 1);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapPrp.Add("key1", 2);
            Must.Throw<FUnitException>("Expected '1', but was '2'. ($.Data.MapPrp[key1])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when MapPrp has unnecessary key", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapPrp.Add("key1", 1);
            expected.Data.MapPrp.Add("key2", 2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapPrp.Add("key1", 1);
            actual.Data.MapPrp.Add("key2", 2);
            actual.Data.MapPrp.Add("unnecessary", 310);
            Must.Throw<FUnitException>("Expected condition '$.Data.MapPrp doesn't have unnecessary key: unnecessary' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when MapPrp doesn't have necessary key", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapPrp.Add("key1", 1);
            expected.Data.MapPrp.Add("key2", 2);
            expected.Data.MapPrp.Add("required", 310);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapPrp.Add("key1", 1);
            actual.Data.MapPrp.Add("key2", 2);
            Must.Throw<FUnitException>("Expected condition '$.Data.MapPrp has required key: required' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when ListPrp values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListPrp.Add(1);
            expected.Data.ListPrp.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListPrp.Add(1);
            actual.Data.ListPrp.Add(2);
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when ListPrp values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListPrp.Add(1);
            expected.Data.ListPrp.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListPrp.Add(1);
            actual.Data.ListPrp.Add(3);
            Must.Throw<FUnitException>("Expected '2', but was '3'. ($.Data.ListPrp[1])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when ListPrp content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListPrp.Add(1);
            expected.Data.ListPrp.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListPrp.Add(1);
            Must.Throw<FUnitException>("Expected condition '$.Data.ListPrp has 2 items' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when ArrayPrp values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayPrp = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayPrp = new int[] { 1, 2 };
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when ArrayPrp values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayPrp = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayPrp = new int[] { 1, 3 };
            Must.Throw<FUnitException>("Expected '2', but was '3'. ($.Data.ArrayPrp[1])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when ArrayPrp content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayPrp = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayPrp = new int[] { 1 };
            Must.Throw<FUnitException>("Expected condition '$.Data.ArrayPrp has 2 items' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when JagArrayPrp values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when JagArrayPrp values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 99.9f } };
            Must.Throw<FUnitException>("Expected '2.2', but was '99.9'. ($.Data.JagArrayPrp[1][1])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when JagArrayPrp content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayPrp = new float[][] { new float[] { 1.1f, 1.2f } };
            Must.Throw<FUnitException>("Expected condition '$.Data.JagArrayPrp has 2 items' to be met, but it was not.", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should pass when direct array comparison is equal", () =>
        {
            var expected = new[] { 1, 2, 3 };
            var actual = new[] { 1, 2, 3 };
            Must.HaveEqualProperties(expected, actual);
        });

        it("should pass when direct list comparison is equal", () =>
        {
            var expected = new List<int> { 4, 5, 6 };
            var actual = new List<int> { 4, 5, 6 };
            Must.HaveEqualProperties(expected, actual);
        });

        it("should pass when direct dictionary comparison is equal", () =>
        {
            var expected = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            var actual = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            Must.HaveEqualProperties(expected, actual);
        });

        it("should throw when direct array comparison fails", () =>
        {
            var expected = new[] { 1, 2, 3 };
            var actual = new[] { 1, 2, 99 };
            Must.Throw<FUnitException>("Expected '3', but was '99'. ($[2])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when direct list comparison fails", () =>
        {
            var expected = new List<int> { 4, 5, 6 };
            var actual = new List<int> { 4, 5, 99 };
            Must.Throw<FUnitException>("Expected '6', but was '99'. ($[2])", () => Must.HaveEqualProperties(expected, actual));
        });

        it("should throw when direct dictionary comparison fails", () =>
        {
            var expected = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            var actual = new Dictionary<string, int> { { "a", 7 }, { "b", 99 } };
            Must.Throw<FUnitException>("Expected '8', but was '99'. ($[b])", () => Must.HaveEqualProperties(expected, actual));
        });
    });

    describe("Must.HaveEqualFields", it =>
    {
        it("should pass when fields are equal", () =>
        {
            var expected = new Wrapper<TestStruct>(new TestStruct { Value = 10 });
            var actual = new Wrapper<TestStruct>(new TestStruct { Value = 10 });
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when fields are not equal", () =>
        {
            var expected = new Wrapper<TestStruct>(new TestStruct { Value = 10 });
            var actual = new Wrapper<TestStruct>(new TestStruct { Value = 20 });
            Must.Throw<FUnitException>("Expected '10', but was '20'. ($.m_data.Value)", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when fields are equal and one is skipped", () =>
        {
            var expected = new Wrapper<TestStruct>(new TestStruct { Value = 10 });
            var actual = new Wrapper<TestStruct>(new TestStruct { Value = 9999 });
            Must.HaveEqualFields(expected, actual, new[] { "Value" });
        });

        it("should pass when null fields are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckFld = null });
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckFld = null });
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when null fields are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckFld = null });
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures { NullCheckFld = "not null" });
            Must.Throw<FUnitException>("Expected condition '$.m_data.NullCheckFld == null' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when MapFld values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapFld.Add("key1", 1);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapFld.Add("key1", 1);
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when MapFld values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapFld.Add("key1", 1);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapFld.Add("key1", 2);
            Must.Throw<FUnitException>("Expected '1', but was '2'. ($.m_data.MapFld[key1])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when MapFld has unnecessary key", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapFld.Add("key1", 1);
            expected.Data.MapFld.Add("key2", 2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapFld.Add("key1", 1);
            actual.Data.MapFld.Add("key2", 2);
            actual.Data.MapFld.Add("unnecessary", 310);
            Must.Throw<FUnitException>("Expected condition '$.m_data.MapFld doesn't have unnecessary key: unnecessary' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when MapFld doesn't have necessary key", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.MapFld.Add("key1", 1);
            expected.Data.MapFld.Add("key2", 2);
            expected.Data.MapFld.Add("required", 310);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.MapFld.Add("key1", 1);
            actual.Data.MapFld.Add("key2", 2);
            Must.Throw<FUnitException>("Expected condition '$.m_data.MapFld has required key: required' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when ListFld values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListFld.Add(1);
            expected.Data.ListFld.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListFld.Add(1);
            actual.Data.ListFld.Add(2);
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when ListFld values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListFld.Add(1);
            expected.Data.ListFld.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListFld.Add(1);
            actual.Data.ListFld.Add(3);
            Must.Throw<FUnitException>("Expected '2', but was '3'. ($.m_data.ListFld[1])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when ListFld content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ListFld.Add(1);
            expected.Data.ListFld.Add(2);
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ListFld.Add(1);
            Must.Throw<FUnitException>("Expected condition '$.m_data.ListFld has 2 items' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when ArrayFld values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayFld = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayFld = new int[] { 1, 2 };
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when ArrayFld values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayFld = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayFld = new int[] { 1, 3 };
            Must.Throw<FUnitException>("Expected '2', but was '3'. ($.m_data.ArrayFld[1])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when ArrayFld content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.ArrayFld = new int[] { 1, 2 };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.ArrayFld = new int[] { 1 };
            Must.Throw<FUnitException>("Expected condition '$.m_data.ArrayFld has 2 items' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when JagArrayFld values are equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when JagArrayFld values are not equal", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 99.9f } };
            Must.Throw<FUnitException>("Expected '2.2', but was '99.9'. ($.m_data.JagArrayFld[1][1])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when JagArrayFld content count is different", () =>
        {
            var expected = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            expected.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f }, new float[] { 2.1f, 2.2f } };
            var actual = new Wrapper<AllSupportedFeatures>(new AllSupportedFeatures());
            actual.Data.JagArrayFld = new float[][] { new float[] { 1.1f, 1.2f } };
            Must.Throw<FUnitException>("Expected condition '$.m_data.JagArrayFld has 2 items' to be met, but it was not.", () => Must.HaveEqualFields(expected, actual));
        });

        it("should pass when direct array comparison is equal", () =>
        {
            var expected = new[] { 1, 2, 3 };
            var actual = new[] { 1, 2, 3 };
            Must.HaveEqualFields(expected, actual);
        });

        it("should pass when direct list comparison is equal", () =>
        {
            var expected = new List<int> { 4, 5, 6 };
            var actual = new List<int> { 4, 5, 6 };
            Must.HaveEqualFields(expected, actual);
        });

        it("should pass when direct dictionary comparison is equal", () =>
        {
            var expected = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            var actual = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            Must.HaveEqualFields(expected, actual);
        });

        it("should throw when direct array comparison fails", () =>
        {
            var expected = new[] { 1, 2, 3 };
            var actual = new[] { 1, 2, 99 };
            Must.Throw<FUnitException>("Expected '3', but was '99'. ($[2])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when direct list comparison fails", () =>
        {
            var expected = new List<int> { 4, 5, 6 };
            var actual = new List<int> { 4, 5, 99 };
            Must.Throw<FUnitException>("Expected '6', but was '99'. ($[2])", () => Must.HaveEqualFields(expected, actual));
        });

        it("should throw when direct dictionary comparison fails", () =>
        {
            var expected = new Dictionary<string, int> { { "a", 7 }, { "b", 8 } };
            var actual = new Dictionary<string, int> { { "a", 7 }, { "b", 99 } };
            Must.Throw<FUnitException>("Expected '8', but was '99'. ($[b])", () => Must.HaveEqualFields(expected, actual));
        });
    });
});


#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable CA1825 // Avoid zero-length array allocations
#pragma warning disable IDE0032 // Use auto property

file sealed record Wrapper<T>
{
    private readonly T m_data;
    public T Data => m_data;
    public Wrapper(T data) => m_data = data;
}

file sealed class TestClass
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

file struct TestStruct
{
    public int Value;
}

file sealed class AllSupportedFeatures
{
    public string? NullCheckFld;
    public string? NullCheckPrp { get; set; }

    public int ValueCheckFld;
    public int ValueCheckPrp { get; set; }

    public Dictionary<string, int> MapFld = new();
    public Dictionary<string, int> MapPrp { get; set; } = new();

    public List<int> ListFld = new();
    public List<int> ListPrp { get; set; } = new();

    public int[] ArrayFld = new int[0];
    public int[] ArrayPrp { get; set; } = new int[0];

    public float[][] JagArrayFld = new float[0][];
    public float[][] JagArrayPrp { get; set; } = new float[0][];
}
