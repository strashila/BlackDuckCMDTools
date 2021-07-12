BlackDuckGetComponentsUUID is a tool that lists all the component UUID for a specific project version.  
API endpoint at /api/projects/{projectId}/versions/{projectVersionId}/components.  

The tool builds a CSV-like output with component name, and UUID. 

Usage: BlackDuckGetBomMatchedFiles [options]

Options:
--token | REQUIRED: BD Token
--bdurl | REQUIRED: BlackDuck URL
--projectname | REQUIRED: Project name
--versionname | REQUIRED: Version name
--not-secure | Disable secure connection to BlackDuck server
-f, --filepath | Output filepath. If not present in options, the tool will print the output to console
--filter | Supported Filters: [bomMatchType]
--version | Show version information
-?, -h, --help | Show help and usage information
