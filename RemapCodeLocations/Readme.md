**RemapCodeLocations** follows the following workflow:  
1. Take a BlackDuck Project Version, and get all the Code locations for the Version.  
2. Parse all codelocations and create a dictionary with key: Version object, and value: list of codelocation id's.  
3. Print the dictionary as Json to console.  
4. The Unmapping: iterate over the dict, for each key iterate over the list of codelocations, and set "mappedProjectVersion" to "" for each codelocation.  
5. The Re-mapping: iterate over the dict again, iterate over the list of codelocations, and set "mappedProjectVersion" to the full version url for each codelocation.  

The tool uses "?offset=0&limit=1000" for all API calls, no pagination.   

Usage: RemapCodeLocations [options]  

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BD URL  
 --projectname <projectname>   | REQUIRED: Project Name    
 --versionname <versionname>   | Optional: Project Version Name. If this is not passed, the tool will try to iterate over ALL the versions of the project   
 --not-secure                  | Allways trust server certificate  

 If no versionname is provided, the tool will ask for confirmation to run over the entire project. 

 
On Windows .NET 8.0 is requred  


Usage:  
.\RemapCodeLocations.exe --token=`<your-bd-token>` --bdurl=`<your-bd-server-url>`  --projectname=`<projectName>` --versionname=`<versionname>` --not-secure  


On Linux "dotnet-runtime-8.0" package is required: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
Usage: dotnet RemapCodeLocations.dll
