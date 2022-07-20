**checkBomCopyrights** is a tool that checks the existence of copyrights for a specific project version BOM.  


The tool builds a CSV-like output with component name, version, origin external ID and number of available copyrights.  
There are 2 users: with "versionname" specified, and without. If versionname is not specified, the tool will iterate over all the versions of the project.   


Usage: checkBomCopyrights [options]

Options:
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BD URL  
--projectname | REQUIRED: Project name  
--versionname | Project version name. If not specified, the tool will iterate over all the versions of the project.  
--not-secure | Allways trust server certificate  
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console  
--version | Show version information  
-?, -h, --help | Show help and usage information

On Windows .NET core 3.1 is requred  
Usage:  
PS C:\dev\synopsys\BlackDuckCMDTools\checkBomCopyrights\bin\Debug\netcoreapp3.1> .\checkBomCopyrights.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --projectname `<projectName>` --versionname `<projectVersionName>` -f `<filePath>` --not-secure  

on Linux "dotnet-sdk" package is required: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
Usage: dotnet checkBomCopyrights.dll

