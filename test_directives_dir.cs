#:project ../src
#:project ../directives
#warning funit include Directives_TestClass.cs
return FUnit.Run(args, describe => {
    describe("test", it => {
        it("should work", () => {
            Must.BeEqual(310, Tests.TestClass.TestMethod());
        });
    });
});
