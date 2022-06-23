**RemapCodeLocations** follows the following workflow:  
1. Take a project and iterate over its versions to get the codelocations for each version.  
2. Parse all codelocations and create a dictionary with key: full version URL, and value: list of codelocation id's.  
3. Create a json file with the dictionary, which functions as a backup.  
4. The Unmapping: iterate over the dict, for each key iterate over the list of codelocations, and set "mappedProjectVersion" to "" for each codelocation.  
5. The Re-map: iterate over the dict again, iterate over the list of codelocations, and set "mappedProjectVersion" to the key (full version url) for each codelocation.  

The tool uses "?offset=0&limit=1000" for all API calls, no pagination.   

Usage: RemapCodeLocations [options]  

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BD URL  
 --projectname <projectname>   | REQUIRED: Project Name  
 --not-secure                  | Allways trust server certificate  
 --filename <filename>         | Output filename to save codelocations JSON. If not present in options, the tool will create a default json in the run folder

 
On Windows .NET core 3.1 is requred  
Usage:  
PS C:\dev\synopsys\BlackDuckCMDTools\RemapCodeLocations\bin\Debug\netcoreapp3.1> .\RemapCodeLocations.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --projectname `<projectName>` -f `<filePath>` --not-secure  

on Linux "dotnet-sdk" package is required: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
Usage: dotnet RemapCodeLocations.dll
