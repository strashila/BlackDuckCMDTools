**GetComponentUsedProjects** gets the list of all the Projects and Project Versions where a component name is used  
Usage: GetComponentUsedProjects [options]

Options:

--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BlackDuck URL  
--component | REQUIRED: Component name  
--not-secure | Allways trust server certificate  
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console  
--version | Show version information  
-?, -h, --help | Show help and usage information  

Example looking for any log4j components (Linux with dotnet package) 

On Linux dotnet-runtime-8.0 package is required: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install


$ dotnet GetComponentUsedProjects.dll --bdurl={url} --token={token} --component=log4j --not-secure


