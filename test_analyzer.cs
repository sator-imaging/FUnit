#:project ./directives/FUnit.Directives.csproj
#warning funit include test/Directives_TestClass.cs
class C { void M() { _ = Tests.TestClass.TestMethod(); } }
