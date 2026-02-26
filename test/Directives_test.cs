#:project ../src
#:project ../directives/FUnit.Directives.csproj

// cannot...? --> #:project ../directives

// [TEST] allow multiple include directives scattered in project
#warning funit include Directives_TestClass.cs
#warning funit include Directives_TestClass.cs

// [TEST] no duplicate even if same file is specified in different way
#warning funit include ./Directives_TestClass.cs
#warning funit include ./Directives_TestClass.cs

#warning THIS WARNING IS EMITTED BY PREPROCESSOR DIRECTIVE

return FUnit.Run(args, describe =>
{
    describe("FUnit.Directives", it =>
    {
        it("should work (#warning funit include)", () =>
        {
            Must.BeEqual(310, Tests.TestClass.TestMethod());
        });
    });
});
