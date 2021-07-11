BlackDuckGetBomMatchedFiles is a tool that chains API calls to get the list of all matched files from matched file API endpoint at 
/api/projects/{projectId}/versions/{projectVersionId}/components/{componentId}/matched-files

The tool builds a CSV-like output with uri or declaredComponentPath, matchType and componentId
The command line options parsing is done with System.CommandLine library https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md

 
Usage:
  BlackDuckGetBomMatchedFiles [options]

Options:
  --token <token>              REQUIRED: BD Token
  --bdurl <bdurl>              REQUIRED: BlackDuck URL
  --projectname <projectname>  REQUIRED: Project name
  --versionname <versionname>  REQUIRED: Version name
  --not-secure                 disable secure connection
  -f, --filepath <filepath>    Output filepath. If not present in options, the tool will print the output to console
  --filter <filter>            Supported Filters: [bomMatchType]
  --version                    Show version information
  -?, -h, --help               Show help and usage information
