#tool "nuget:?package=JetBrains.dotCover.CommandLineTools&version=2019.1.2"
#tool "nuget:?package=ReportGenerator&version=4.0.5"
#addin "nuget:?package=Cake.Docker&version=0.10.0"

var target = Argument("Target", "OpenReport");

Setup<BuildState>(_ => 
{
    var state = new BuildState
    {
        Paths = new BuildPaths
        {
            SolutionFile = MakeAbsolute(File("./Translator.sln")),
            HostProjectFile = MakeAbsolute(File("./src/QueueProcessor/QueueProcessor.csproj"))
        }
    };

    CleanDirectory(state.Paths.OutputFolder);

    return state;
});


Task("Restore")
    .Does<BuildState>(state =>
{
    var settings = new DotNetCoreRestoreSettings();

    DotNetCoreRestore(state.Paths.SolutionFile.ToString(), settings);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does<BuildState>(state => 
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug",
        NoRestore = true
    };

    DotNetCoreBuild(state.Paths.SolutionFile.ToString(), settings);
});


Task("RunTests")
    .IsDependentOn("Build")
    .Does<BuildState>(state => 
{
    var projectFiles = GetFiles($"{state.Paths.TestFolder}/**/Tests.*.csproj");

    bool success = true;

    foreach (var file in projectFiles)
    {
        var targetFrameworks = GetTargetFrameworks(file);

        foreach (var framework in targetFrameworks)
        {
            var frameworkFriendlyName = framework.Replace(".", "-");

            try
            {
                Information($"Testing {file.GetFilenameWithoutExtension()} ({framework})");

                var testResultFile = state.Paths.TestOutputFolder.CombineWithFilePath($"{file.GetFilenameWithoutExtension()}-{frameworkFriendlyName}.trx");
                var coverageResultFile = state.Paths.TestOutputFolder.CombineWithFilePath($"{file.GetFilenameWithoutExtension()}-{frameworkFriendlyName}.dcvr");

                var projectFile = MakeAbsolute(file).ToString();

                var dotCoverSettings = new DotCoverCoverSettings()
                                        .WithFilter("+:QueueProcessor*")
                                        .WithFilter("-:Tests*")
                                        .WithFilter("-:TestUtils");

                var settings = new DotNetCoreTestSettings
                {
                    NoBuild = true,
                    NoRestore = true,
                    Logger = $"trx;LogFileName={testResultFile.FullPath}",
                    Filter = "TestCategory!=External",
                    Framework = framework
                };

                DotCoverCover(c => c.DotNetCoreTest(projectFile, settings), coverageResultFile, dotCoverSettings);
            }
            catch (Exception ex)
            {
                Error($"There was an error while executing the tests: {file.GetFilenameWithoutExtension()}", ex);
                success = false;
            }

            Information("");
        }
    }
    
    if (!success)
    {
        throw new CakeException("There was an error while executing the tests");
    }

    string[] GetTargetFrameworks(FilePath file)
    {
        XmlPeekSettings settings = new XmlPeekSettings
        {
            SuppressWarning = true
        };

        return (XmlPeek(file, "/Project/PropertyGroup/TargetFrameworks", settings) ?? XmlPeek(file, "/Project/PropertyGroup/TargetFramework", settings)).Split(";");
    }
});

Task("MergeCoverageResults")
    .IsDependentOn("RunTests")
    .Does<BuildState>(state =>
{
    Information("Merging coverage files");
    var coverageFiles = GetFiles($"{state.Paths.TestOutputFolder}/*.dcvr");
    DotCoverMerge(coverageFiles, state.Paths.DotCoverOutputFile);
    DeleteFiles(coverageFiles);
});

Task("GenerateXmlReport")
    .IsDependentOn("MergeCoverageResults")
    .Does<BuildState>(state =>
{
    Information("Generating dotCover XML report");
    DotCoverReport(state.Paths.DotCoverOutputFile, state.Paths.DotCoverOutputFileXml, new DotCoverReportSettings 
    {
        ReportType = DotCoverReportType.DetailedXML
    });
});

Task("ExportReport")
    .IsDependentOn("GenerateXmlReport")
    .Does<BuildState>(state =>
{
    Information("Executing ReportGenerator to generate HTML report");
    ReportGenerator(state.Paths.DotCoverOutputFileXml, state.Paths.ReportFolder, new ReportGeneratorSettings {
            ReportTypes = new[]{ReportGeneratorReportType.Html, ReportGeneratorReportType.Xml}
    });
});

Task("Test")
    .IsDependentOn("RunTests")
    .IsDependentOn("MergeCoverageResults")
    .IsDependentOn("GenerateXmlReport")
    .IsDependentOn("ExportReport");

Task("OpenReport")
    .IsDependentOn("Test")
    .Does<BuildState>(state => 
{
    FilePath reportPath = File($"{state.Paths.ReportFolder}/index.htm");
    Information($"Opening {reportPath}");
    
    if (IsRunningOnWindows())
    {
        StartProcess("cmd", new ProcessSettings {
            Arguments = $"/C start \"\" {reportPath}"
        });
    }
});

Task("Publish")
    .IsDependentOn("Test")
    .Does<BuildState>(state => 
{
    DotNetCorePublishSettings settings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        NoRestore = true,
        OutputDirectory = state.Paths.PublishFolder,
        WorkingDirectory = state.Paths.HostProjectFile.GetDirectory()
    };

    DotNetCorePublish(state.Paths.HostProjectFile.ToString(), settings);
});

RunTarget(target);

public class BuildState
{
    public BuildPaths Paths { get; set; }
}

public class BuildPaths
{
    public FilePath SolutionFile { get; set; }

    public FilePath HostProjectFile { get; set; }

    public DirectoryPath SolutionFolder => SolutionFile.GetDirectory();

    public DirectoryPath TestFolder => SolutionFolder.Combine("tests");

    public DirectoryPath OutputFolder => SolutionFolder.Combine("outputs");

    public DirectoryPath PublishFolder => OutputFolder.Combine("publish");

    public DirectoryPath TestOutputFolder => OutputFolder.Combine("tests");

    public FilePath DotCoverOutputFile => TestOutputFolder.CombineWithFilePath("coverage.dcvr");

    public FilePath DotCoverOutputFileXml => TestOutputFolder.CombineWithFilePath("coverage.xml");

    public FilePath OpenCoverResultFile => OutputFolder.CombineWithFilePath("OpenCover.xml");

    public DirectoryPath ReportFolder => TestOutputFolder.Combine("report");
}