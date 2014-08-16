// include Fake lib
#r @"tools/fake/FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile

let productDescription = "A job runner suited for running jobs in Microsoft Azure Worker Roles."
let productName = "Red Dog"

let version = environVarOrDefault "version" "1.0.0.0"
let buildDir = "./build/output/"
let packagingDir = "./build/packages/"


Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    CreateCSharpAssemblyInfo "./src/RedDog.Engine/Properties/AssemblyInfo.cs"
        [Attribute.Title "RedDog.Engine"
         Attribute.Description productDescription
         Attribute.Product productName
         Attribute.Version version
         Attribute.FileVersion version]

    // Build all projects.
    !! "./src/**/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Package" (fun _ ->
    let author = ["Sandrino Di Mattia"]
    
    // Prepare RedDog.Engine.
    let workingDir = packagingDir
    let net40Dir = workingDir @@ "lib/net40/"
    CleanDirs [workingDir; net40Dir]
    CopyFile net40Dir (buildDir @@ "RedDog.Engine.dll")
    
    // Package RedDog.Engine
    NuGet (fun p ->
        {p with
            Authors = author
            Project = "RedDog.Engine"
            Description = productDescription
            OutputPath = packagingDir
            Summary = productDescription
            WorkingDir = workingDir
            Version = version }) "./packaging/RedDog.Engine.nuspec"
)
    
// Default target
Target "Default" (fun _ ->
    let msg = "Building RedDog.Engine version: " + version
    trace msg
)

// Dependencies
"Clean"
   ==> "Build"
   ==> "Package"
   ==> "Default"
  
// Start Build
RunTargetOrDefault "Default"