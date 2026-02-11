return FUnit.Run(args, describe =>
{
    describe("Test subject", it =>
    {
        it("should be true", () =>
        {
            Must.BeTrue(true);
        });
    });
});


/* uncomment to test FUnit.Directives

// multiple include of same file should be allowed
//:funit:include Sandbox.cs
//:funit:include Sandbox.cs
//:funit:include ./Sandbox.cs
//:funit:include ./Sandbox.cs

// IGNORED: prefix must be single line comment and placed at line beginning
//   //:funit:
///:funit:
// leading space is not allowed
    //:funit:

// ERRORS
//:funit:include
//:funit:include  NotFound.cs
//:funit:unknown
//:funit:
//:funit:include  file not supported

*/


// IGNORED: in multiline comment
/*
//:funit:
*/
