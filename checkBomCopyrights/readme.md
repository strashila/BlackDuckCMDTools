**checkBomCopyrights** is a tool that checks the existence of copyrights for a specific project version BOM.  


The tool builds a CSV-like output with Project Name, Project Version, Component name, Component version, Origin ID, Origin released date, and the number of available copyrights.  
There are 2 ways to use it: with "versionname" specified, and without. If versionname is not specified, the tool will iterate over all the versions of the project.   


Usage: checkBomCopyrights [options]

Options:
--token | REQUIRED: BlackDuck Token  
--bdurl | REQUIRED: BlackDuck URL  
--projectname | REQUIRED: Project name  
--versionname | Project version name. If not specified, the tool will iterate over all the versions of the project.  
--not-secure | Allways trust server certificate  
-f, --filename | Output filename. If not present in options, the tool will create a default filename, {projectName}_copyrights.txt  
--version | Show version information  
-?, -h, --help | Show help and usage information

On Windows .NET core 3.1 is requred  
Usage:  
 .\checkBomCopyrights.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --projectname `<projectName>` --versionname `<projectVersionName>` -f `<filePath>` --not-secure  

on Linux dotnet-runtime-8.0 package is required: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install
Usage: dotnet checkBomCopyrights.dll

