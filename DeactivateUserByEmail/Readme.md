**DeactivateUsersByEmail** gets a user email, and provided that it is a user of type "EXTERNAL", deactivated the user in BD

The command line options parsing is done with the beta System.CommandLine library. https://github.com/dotnet/command-line-api/issues/1537  
The new command handler API
 
 
Usage: DeactivateUsersByEmail [options]  

 
Example usage:  
DeactivateUsersByEmail.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>` --email `user@email.com` --trust-cert

Linux:  
dotnet DeactivateUsersByEmail.dll --token `<your-bd-token>` --bdurl `<your-bd-server-url>` --email `user@email.com` --trust-cert
