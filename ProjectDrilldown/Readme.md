**ProjectDrilldown** is a tool that takes a project, and creates a complete drilldown - it gets all versions, all codelocations (scans), the total scan size, anf matches from latest scan summary

The command line options parsing is done with the beta System.CommandLine library  
 
Usage: ProjectDrilldow [options]  

Options:  

 --token <token>               | REQUIRED: BD Token  
 --bdurl <bdurl>               | REQUIRED: BD URL 
 --projectname <projectname>   | REQUIRED: Project Name  
  --not-secure                 | Allways trust server certificate  
  -f, --filepath <filepath>    | Output filepath. If not present in options, the tool will print the output to a default file: projectName + _drilldown.csv
 
Example usage:  
ProjectDrilldown.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>` --projectname `MyProject` -f `c:\temp\myproject.txt` --not-secure

Linux:  
dotnet GetAllScans.dll --token `<your-bd-token>` --bdurl `<your-bd-server-url>` --projectname `MyProject`
