#tool nuget:?package=NUnit.ConsoleRunner

var target = Argument("target", "Default");

Task("Default")
  .IsDependentOn("Test");

Task("Build")
  .Does(() =>
{
  MSBuild("./QuartzFlow.sln");
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        NUnit3("./QuartzFlow.Tests/bin/Debug/QuartzFlow.Tests.dll");
		NUnit3("./QuartzFlowHost.Tests/bin/Debug/QuartzFlowHost.Tests.dll");
    });

RunTarget(target);