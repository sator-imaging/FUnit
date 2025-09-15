#:package FUnit@*
#:package FUnit.Directives@0.2.0-rc.1

// [TEST] allow multiple include directives scattered in project
//:funit:include Directives_TestClass.cs
//:funit:include Directives_TestClass.cs

// [TEST] no duplicate even if same file is specified in different way
//:funit:include ./Directives_TestClass.cs
//:funit:include ./Directives_TestClass.cs

return FUnit.Run(args, describe =>
{
    describe("FUnit.Directives", it =>
    {
        it("should work (:funit:include)", () =>
        {
            Must.BeEqual(310, Foo.TestClass.TestMethod());
        });
    });
});
