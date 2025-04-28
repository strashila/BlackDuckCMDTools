**GetComponentVersions** is a tool that lists all the versions for passed component UUID  
API endpoint at /api/components/{componentId}/versions  

The tool builds a CSV-like output with component UUID, version name, and version UUID. 

Usage: GetComponentVersions [options]

Options:  
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BD URL  
--component | REQUIRED: component UUID  
--not-secure | Allways trust server certificate  
-f, --filename | Output filename. If not present in options, the tool will print the output to console  
--version | Show version information  
-?, -h, --help | Show help and usage information

Example usage:  

GetComponentVersions.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --component `<componentId>` -f `<filename>` --not-secure  

On Linux dotnet-runtime-8.0 package is required: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install
dotnet GetComponentVersions.dll
