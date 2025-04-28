**GetAllScans** is a tool that gets the list of all the scans in the hub, and marks if they are mapped to a project version, or unmapped, adds a Codelocation name, version URL and codelocation scanSize

/api-doc/public.html#codelocation-list  

The command line options parsing is done with the beta System.CommandLine library  
 
Usage: GetAllScans [options]  

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BD URL   
  --not-secure                 | Allways trust server certificate  
  -f, --filepath <filepath>    | Output filepath. If not present in options, the tool will print the output to console  
 
Example usage:  
GetAllScans.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>` -f `c:\temp\codelocations.txt` --not-secure

Linux:  
On Linux dotnet-runtime-8.0 package is required: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install
dotnet GetAllScans.dll --token `<your-bd-token>` --bdurl `<your-bd-server-url>`
