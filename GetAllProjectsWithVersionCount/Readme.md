**GetAllProjectsWithVersionCount** gets the list of all projects in the hub with version count.
 
Usage:
  GetAllProjectsWithVersionCount [options]

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BlackDuck URL   
 --not-secure                 | Allways trust server certificate  
 -f, --filepath <filepath>    | Output filepath. If not present in options, the tool will print the output to console  
 --version                    | Show version information  
 -?, -h, --help               | Show help and usage information  
 

On Linux dotnet-runtime-8.0 package is required: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install