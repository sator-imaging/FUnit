#:project ../src
#:package FUnit.Directives@*

// cannot...? --> #:project ../directives

// [TEST] allow multiple include directives scattered in project
//:funit:include Directives_TestClass.cs
//:funit:include Directives_TestClass.cs

// [TEST] no duplicate even if same file is specified in different way
//:funit:include ./Directives_TestClass.cs
//:funit:include ./Directives_TestClass.cs

#warning THIS WARNING IS EMITTED BY PREPROCESSOR DIRECTIVE

return FUnit.Run(args, describe =>
{
    describe("FUnit.Directives", it =>
    {
        it("should work (:funit:include)", () =>
        {
            Must.BeEqual(310, Tests.TestClass.TestMethod());
        });
    });
});
