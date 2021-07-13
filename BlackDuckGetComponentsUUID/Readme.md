**BlackDuckGetComponentsUUID** is a tool that lists all the component UUID for a specific project version.  
API endpoint at /api/projects/{projectId}/versions/{projectVersionId}/components.  

The tool builds a CSV-like output with component name, and UUID. 

Usage: BlackDuckGetComponentsUUID [options]

Options:
--token | REQUIRED: BD Token  
--bdurl | REQUIRED: BlackDuck URL  
--projectname | REQUIRED: Project name  
--versionname | REQUIRED: Version name  
--not-secure | Allways trust server certificate 
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console  
--filter | Supported Filters: /api-doc/public.html#_listing_bom_components  
--version | Show version information  
-?, -h, --help | Show help and usage information

Example usage:  

C:\dev\synopsys\BlackDuckCMDTools\BlackDuckGetComponentsUUID\bin\Debug\netcoreapp3.1>BlackDuckGetComponentsUUID.exe --token `<your-bd-token>` --bdurl `<your-bd-server-url>`  --projectname `<projectName>` --versionname `<projectVersionName>` -f c:\temp\components.txt --not-secure
