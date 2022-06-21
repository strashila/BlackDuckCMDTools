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
