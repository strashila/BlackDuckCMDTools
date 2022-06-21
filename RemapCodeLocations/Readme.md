**RemapCodeLocations** does the following workflow:  
1. Takes a project and iterates over its versions to get the codelocations for each version.  
2. Parses all codelocations and creates a dictionary with key: full version URL and value: list of codelocation id's.  
3. Creates a json file with the dictionary, which functions as a backup.  
4. The Unmapping: the tool iterates over the dict, and for each key it iterates over all the codelocations in the value list, and set "mappedProjectVersion" to "" for each one.   
5. The Map again: it iterates over the dict again, and maps the key (fill version url) to "mappedProjectVersion" of each codelocation again.  
 
Usage: RemapCodeLocations [options]  

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BD URL  
 --projectname <projectname>   | REQUIRED: Project Name  
 --not-secure                  | Allways trust server certificate  
 --filename <filenameh>        | Output filename to save codelocations JSON. If not present in options, the tool will create a default output in the run folder
