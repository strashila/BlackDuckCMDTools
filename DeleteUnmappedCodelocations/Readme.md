**DeleteUnmappedCodelocations** is a tool that deletes all unmapped codelocations (not mapped to any project versions) from the hub    

Usage: DeleteUnmappedCodelocations [options]

Options:
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BD URL  
--not-secure | Allways trust server certificate  

Example usage:  
DeleteUnmappedCodelocations.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  

Linux:  
dotnet DeleteUnmappedCodelocations.dll
