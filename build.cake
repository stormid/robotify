// #tool "xunit.runner.console"
#tool "GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument("configuration", "Release");
var solutionPath            = MakeAbsolute(File(Argument("solutionPath", "./Robotify.sln")));
var nugetProjects           = Argument("nugetProjects", "Robotify");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var artifacts               = MakeAbsolute(Directory(Argument("artifactPath", "./artifacts")));
var buildOutput             = MakeAbsolute(Directory(artifacts +"/build/"));
var testResultsPath         = MakeAbsolute(Directory(artifacts + "./test-results"));
var versionAssemblyInfo     = MakeAbsolute(File(Argument("versionAssemblyInfo", "VersionAssemblyInfo.cs")));

IEnumerable<FilePath> nugetProjectPaths     = null;
SolutionParserResult solution               = null;
GitVersion versionInfo                      = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(() => {
    if(!FileExists(solutionPath)) throw new Exception(string.Format("Solution file not found - {0}", solutionPath.ToString()));
    solution = ParseSolution(solutionPath.ToString());

    var projects = solution.Projects.Where(x => nugetProjects.Contains(x.Name));
    if(projects == null || !projects.Any()) throw new Exception(string.Format("Unable to find projects '{0}' in solution '{1}'", nugetProjects, solutionPath.GetFilenameWithoutExtension()));
    
    Information("[Setup] Using Solution '{0}'", solutionPath.ToString());
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(artifacts.ToString());
    EnsureDirectoryExists(artifacts);
    EnsureDirectoryExists(buildOutput);
    
    var binDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\bin");
    var objDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\obj");
    CleanDirectories(binDirs);
    CleanDirectories(objDirs);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath, new NuGetRestoreSettings());
});

Task("Update-Version-Info")
    .IsDependentOn("CreateVersionAssemblyInfo")
    .Does(() => 
{
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
            UpdateAssemblyInfoFilePath = versionAssemblyInfo
        });

    if(versionInfo != null) {
        Information("Version: {0}", versionInfo.FullSemVer);
    } else {
        throw new Exception("Unable to determine version");
    }
});

Task("CreateVersionAssemblyInfo")
    .WithCriteria(() => !FileExists(versionAssemblyInfo))
    .Does(() =>
{
    Information("Creating version assembly info");
    CreateAssemblyInfo(versionAssemblyInfo, new AssemblyInfoSettings {
        Version = "0.0.0.0",
        FileVersion = "0.0.0.0",
        InformationalVersion = "",
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version-Info")
    .Does(() =>
{
    MSBuild(solutionPath, settings => settings
        .WithProperty("TreatWarningsAsErrors","true")
        .WithProperty("UseSharedCompilation", "false")
        .WithProperty("AutoParameterizationWebConfigConnectionStrings", "false")
        .SetVerbosity(Verbosity.Quiet)
        .SetConfiguration(configuration)
        .WithTarget("Clean;Build")
    );
});

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() => 
{
    EnsureDirectoryExists(buildOutput +"/Robotify");
    EnsureDirectoryExists(buildOutput +"/Robotify/lib/net45");
    CopyFile("./src/Robotify/bin/" +configuration +"/Robotify.dll", buildOutput +"/Robotify/lib/net45/Robotify.dll");
    CopyFile("./src/Robotify/bin/" +configuration +"/Robotify.pdb", buildOutput +"/Robotify/lib/net45//Robotify.pdb");
    CopyFile("./src/Robotify/bin/" +configuration +"/readme.txt", buildOutput +"/Robotify/readme.txt");
    CopyDirectory("./src/Robotify/content", buildOutput +"/Robotify/content");
});

Task("Create-NuGet-Package")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Files")
    .Does(() => 
{
    EnsureDirectoryExists(artifacts +"/packages/");

    var settings = new NuGetPackSettings {
        BasePath = buildOutput +"/Robotify",
        Properties = new Dictionary<string, string> { { "Configuration", configuration }},
        Symbols = false,
        NoPackageAnalysis = true,
        Version = versionInfo.NuGetVersionV2,
        OutputDirectory = artifacts +"/packages/",
        Files = new[] {
            new NuSpecContent { Source = "**/*", Target = "" },
        }
    };
    NuGetPack("./nuspec/Robotify.nuspec", settings);                     
});

Task("Update-AppVeyor-Build-Number")
    .IsDependentOn("Update-Version-Info")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(versionInfo.FullSemVer +"." +AppVeyor.Environment.Build.Number);
});

Task("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Package")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{

    var artifact = MakeAbsolute(File(artifacts.ToString() +"/packages/Robotify.nupkg"));
    AppVeyor.AddInformationalMessage("Uploading artifacts");
    AppVeyor.UploadArtifact(artifact, settings => settings
        .SetArtifactType(AppVeyorUploadArtifactType.NuGetPackage)
    );    
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Create-NuGet-Package");

Task("Default")
    .IsDependentOn("Update-Version-Info")
    .IsDependentOn("Package")
    ;

Task("CI")
    .IsDependentOn("Update-Version-Info")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Package")
    .IsDependentOn("Upload-AppVeyor-Artifacts");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
