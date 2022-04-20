**checkBomCopyrights** is a tool that checks the existence of copyrights for a specific project version BOM.  


The tool builds a CSV-like output with component name, version, origin external ID and number of available copyrights. 

Usage: checkBomCopyrights [options]

Options:
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BD URL  
--projectname | REQUIRED: Project name  
--versionname | REQUIRED: Version name  
--not-secure | Allways trust server certificate  
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console  
--version | Show version information  
-?, -h, --help | Show help and usage information

On Windows .NET core 3.1 is requred  
Usage:  
PS C:\dev\synopsys\BlackDuckCMDTools\checkBomCopyrights\bin\Debug\netcoreapp3.1> .\checkBomCopyrights.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --projectname `<projectName>` --versionname `<projectVersionName>` -f `<filePath>` --not-secure  

on Linux dotnet-sdk is required: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
Usage: dotnet checkBomCopyrights.dll

