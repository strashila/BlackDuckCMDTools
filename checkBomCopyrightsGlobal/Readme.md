**checkBomCopyrightsGlobal** is a tool that checks the existence of copyrights for the whole BlackDuck hub.  


The tool builds a CSV-like output with component name, version, origin external ID and number of available copyrights. 

Usage: checkBomCopyrightsGlobal [options]

Options:
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BD URL  
--not-secure | Allways trust server certificate  
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console  
--version | Show version information  
-?, -h, --help | Show help and usage information

On Windows .NET core 3.1 is requred  
Usage:  
.\checkBomCopyrightsGlobal.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  -f `<filePath>` --not-secure  

on Linux "dotnet-sdk" package is required: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
Usage: dotnet checkBomCopyrights.dll

