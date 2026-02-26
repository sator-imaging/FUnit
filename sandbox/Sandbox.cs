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
#warning include Sandbox.cs
#warning include Sandbox.cs
#warning include ./Sandbox.cs
#warning include ./Sandbox.cs

// IGNORED: directive must be placed at line beginning
//   #warning
/#warning
// leading space is not allowed
    #warning

// ERRORS
#warning include
#warning include  NotFound.cs
#warning unknown
#warning
#warning include  file not supported

*/


// IGNORED: in multiline comment
/*
#warning
*/
