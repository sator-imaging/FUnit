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
