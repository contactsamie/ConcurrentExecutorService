//http://fsharp.github.io/FAKE/apidocs/fake-filehelper.html
#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r "System.Xml.Linq"
#r "packages/ConfigJson/lib/ConfigJsonNET.dll"

open System
open System.IO
open Fake.TaskRunnerHelper
open Fake
open Fake.FileUtils
open System.Xml.Linq
open System.Collections.Generic
open Fake.Testing

let buildParam = getBuildParamOrDefault  "buildType" "release" 
// Directories
let root="./.build/build-"+buildParam
let buildDir  = root+"/app/"
let testDir   =root+ "/test"
let deployDir = root+"/deploy/"
let nugetWorkingDir =root+ "/packaging/"
let allPackageFiles = [
                        (buildDir+"ConcurrentExecutorServiceLib2.dll");
                        (buildDir+"ConcurrentExecutorService.Common.dll");
                        (buildDir+"ConcurrentExecutorService.Messages.dll");
                        (buildDir+"ConcurrentExecutorService.Reception.dll");
                        (buildDir+"ConcurrentExecutorService.ServiceWorker.dll");
                        (buildDir+"ConcurrentExecutorService.ActorSystemFactory.dll");
                        (buildDir+"readme.txt")
                    ]


let nugetDeployPath = getBuildParamOrDefault  "nugetDeployPath" deployDir 

let testOutput = FullName "./.build/TestResults"

//--------------------------------------------------------------------------------
// Information about the project for Nuget and Assembly info files
//--------------------------------------------------------------------------------

let product = "ConcurrentExecutorService"
let authors = [ "samuel" ]
let copyright = "©2017"
let company = ""
let description = "ConcurrentExecutorService"
let tags = []
let projectName="ConcurrentExecutorService"
// Read release notes and version

let BuildFn<'T>= match buildParam with
                  | "debug" -> MSBuildDebug
                  | _       ->MSBuildRelease


                  

let BuildVersionType= match buildParam with
                           | "release" -> ""
                           | _         -> "-"+buildParam

let NugetDeployPath= match nugetDeployPath with
                           | "release" -> ""
                           | _         -> "-"+buildParam

// version info
let version = sprintf "0.1.%s" buildVersion

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [root]    
    CreateDir deployDir
    CreateDir testDir
    CreateDir buildDir
    CreateDir nugetWorkingDir
    CreateDir testOutput
)

let serviceReferences  =  !! "./**/*.csproj"

Target "Build" (fun _ ->
     MSBuildDebug buildDir "Build" serviceReferences
        |> Log "AppBuild-Output: "
)

let testDlls = !! (buildDir + "/ConcurrentExecutorService.Tests.dll")

Target "Test" (fun _ ->  
    let xunitTestAssemblies = !! (testDir + "/ConcurrentExecutorService.Tests.dll")

    let xunitToolPath = findToolInSubPath "xunit.console.exe" "packages/FAKE/xunit.runner.console/tools"
    
    printfn "Using XUnit runner: %s" xunitToolPath
    let runSingleAssembly assembly =
        let assemblyName = Path.GetFileNameWithoutExtension(assembly)
        xUnit2
            (fun p -> { p with XmlOutputPath = Some (testOutput + @"\" + assemblyName + "_xunit_"+buildParam+".xml"); HtmlOutputPath = Some (testOutput + @"\" + assemblyName + "_xunit_"+buildParam+".HTML"); ToolPath = xunitToolPath; TimeOut = System.TimeSpan.FromMinutes 30.0; Parallel = ParallelMode.NoParallelization }) 
            (Seq.singleton assembly)

    xunitTestAssemblies |> Seq.iter (runSingleAssembly)
 
)

Target "CreateNuget" (fun _ ->
    // Copy all the package files into a package folder
    CopyFiles nugetWorkingDir allPackageFiles

    NuGet (fun p -> 
        { 
          p with
            Authors = authors
            Project = projectName
            Description = description                               
            OutputPath = deployDir
            Summary = description
            WorkingDir = nugetWorkingDir
            Version = version 
         })             
            "ConcurrentExecutorServiceLib.nuspec"
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*") 
        -- "*.zip" 
        |> Zip buildDir (deployDir + "ConcurrentExecutorService." + version + ".zip")
)



Target "RemotePublishNuGet" (fun _ ->     
    !! (deployDir + "*.nupkg") 
      |> Copy NugetDeployPath
)


// Build order
"Clean"
  ==> "Build"
  ==> "Test"
  ==> "Deploy"
  ==> "CreateNuget"
  ==> "RemotePublishNuGet"

// start build
RunTargetOrDefault "RemotePublishNuGet"
//RunTargetOrDefault "Test"