**BlackDuckGetBomMatchedFiles** is a tool that gets the list of all matched files from matched file API endpoint at 
/api/projects/{projectId}/versions/{projectVersionId}/components/{componentId}/matched-files


The tool builds a CSV-like output with uri or declaredComponentPath, matchType and componentId.  
The command line options parsing is done with the beta System.CommandLine library


 
Usage:
  BlackDuckGetBomMatchedFiles [options]

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BlackDuck URL   
  --projectname <projectname>  | REQUIRED: Project name   
  --versionname <versionname>  | REQUIRED: Version name   
  --not-secure                 | Allways trust server certificate  
  -f, --filepath <filepath>    | Output filepath. If not present in options, the tool will print the output to console  
  --filter <filter>            | Supported Filters: [bomMatchType]  
  --version                    | Show version information  
  -?, -h, --help               | Show help and usage information  
 
 Example usage:
C:\dev\synopsys\BlackDuckCMDTools\BlackDuckGetBomMatchedFiles\bin\Debug\netcoreapp3.1>BlackDuckGetBomMatchedFiles.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>` --projectname `<projectName>` --versionname `<projectVersionName>` -f c:\temp\components.txt --not-secure --filter filter=bomMatchType:FILE_DEPENDENCY_TRANSITIVE
