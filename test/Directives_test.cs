#:project ../src
#:package FUnit.Directives@*

// [TEST] allow multiple include directives scattered in project
#warning funit include Directives_TestClass.cs
//:funit:include Directives_TestClass.cs

// [TEST] no duplicate even if same file is specified in different way
#warning funit include ./Directives_TestClass.cs
//:funit:include ./Directives_TestClass.cs

return FUnit.Run(args, describe =>
{
    describe("FUnit.Directives", it =>
    {
        it("should work", () =>
        {
            Must.BeEqual(310, Tests.TestClass.TestMethod());
        });
    });
});
