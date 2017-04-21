#tool nuget:?package=NUnit.ConsoleRunner

var target = Argument("target", "Default");

Task("Default")
  .IsDependentOn("Test");

Task("Build")
  .Does(() =>
{
  MSBuild("./JobScheduler.sln");
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        NUnit3("./JobScheduler.Tests/bin/Debug/JobScheduler.Tests.dll");
		NUnit3("./JobSchedulerConsole.Tests/bin/Debug/JobSchedulerHost.Tests.dll");
    });

RunTarget(target);