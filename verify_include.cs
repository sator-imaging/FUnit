#warning funit include test/Directives_TestClass.cs
return FUnit.Run(args, d => d("test", it => it("ok", () => Must.BeEqual(310, Tests.TestClass.TestMethod()))));
