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
#warning funit include Sandbox.cs
#warning funit include Sandbox.cs
#warning funit include ./Sandbox.cs
#warning funit include ./Sandbox.cs

// IGNORED: directive must be placed at line beginning
//   #warning
/#warning
// leading space is not allowed
    #warning

// ERRORS
#warning funit include
#warning funit include  NotFound.cs
// unknown keyword is ignored for #warning
#warning funit unknown
#warning
#warning funit include  file not supported

*/


// IGNORED: in multiline comment
/*
#warning
*/
