**LicenseUpdater** gets csv with license names, urls and a new status, and updates the licenses in accordance with the new status

The command line options parsing is done with the beta System.CommandLine library. https://github.com/dotnet/command-line-api/issues/1537  
 
 
Usage: LicenseUpdater [options]  

 
Example usage:  
LicenseUpdater.exe --bdurl `<your-bd-server-url>` --token `<your-bd-token>` --csvfile `c:\temp\licenses_reviewed.csv` --not-secure

Linux:  
dotnet LicenseUpdater.dll --bdurl `<your-bd-server-url>` --token `<your-bd-token>` --csvfile `/home/user/licenses_reviewed.csv` --not-secure
