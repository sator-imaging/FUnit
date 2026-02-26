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

// IGNORED: prefix must be #warning funit and placed at line beginning
//   #warning funit
//#warning funit
// leading space is not allowed
    #warning funit

// ERRORS
#warning funit include
#warning funit include  NotFound.cs
#warning funit unknown
#warning funit
#warning funit include  file not supported

*/


// IGNORED: in multiline comment
/*
#warning funit
*/
