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

There are two primary templating options that can be used:
1. `@title` If this token is starts the first line in a content file, it will get stripped out and used in the title replacement tag. Example: `@title Page Title`
1. `|token|` If a token is surrounded by pipes, it represents a replacement value. There are currently three supported replacement tokens:
    1. `|title|` Replaced with value represented by `@title`
    1. `|header|` Replaced with template/header.htm
    1. `|footer|` Replaced with template/footer.htm