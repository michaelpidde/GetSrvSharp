# GetSrvSharp
GetSrvSharp is a rudimentary web server that only handles GET requests. It is built on top of the [asyncronous socket server example](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example) provided by Microsoft.

This is not a production server in any sense. Don't use it for anything important if you stumble upon this. It is not highly fault tolerant because writing that sort of code is boring to me.

## Configuration
The server is configured via a simple JSON file:
```
{
    "host": "mysite",
    "port": 6660,
    "siteRoot": "D:/static_site",
    "defaultPage": "index.htm",
    "templateEngineEnabled": true,
    "logFile": "D:/static_site/srv.log"
}
```

## Running
Run the server by passing in the path to the configuration file:
`./GetSrvSharp --config-file D:\static_site\srv.json`

## Site Structure
There are three directories that are expected by convention:
* `content` Required. Contains .htm pages for the site. This can contain sub-directories.
* `template` Required if `templateEngineEnabled` is true. Contains .htm files that can get parsed and included in the content pages.
* `errors` Optional. Contains custom error .htm files named with HTTP status code numbers (e.g. 404.htm).

## Templating
Current templating capabilities are limited and work via convention.

The `templateEngineEnabled` configuration setting will control whether template parsing will occur.

A templating directive is prefixed with two backticks ` `` ` and ends with the same. The engine currently supports only one directive inside a set of backticks - that is, you cannot do something like ` ``include(header) include(header2)`` `. Each include would need to be in its own set of backticks.

There are two primary templating options that can be used:
1. Variables
    1. A variable is defined with an `@` followed by the name of the variable and the value in parentheses. For example: ` ``@title(Site Title)`` `
    1. The variable can then be used elsewhere via the `out` directive. ` ``out(title)`` ` would be used to output the title variable in the above example.
1. Includes
    1. The include directive is expectedly used to directly copy the content of a file into another. For example, ` ``include(header)`` ` would look in the `template` directory for a file named `header.htm` and replace the include directive with its content.
    1. There are no protections against including the same file multiple times.