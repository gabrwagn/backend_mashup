# MusicGenie (backend_mashup)

Small proof of concept for home assignment which I nicknamed MusicGenie.

Quick summary: Mash together apis that serve Album Information, Cover Art and Wikipedia summary into a neat JSON package.

## Requirements: 

1. Visual Studio 2017 (earlier versions not tested).
2. ASP.NET Web Application (.Net Framework)

If any package issues are present, instructions are in the **Packages** section in the bottom of the *README*.

## Installation

Load the MusicGenie/MusicGenie.sln in Visual Studio.

Run the server with F5 (or the green arrow).
Your default browser should open at an index page (http://localhost:55217/index.html) used for demonstrating the service.

By typing in a MBID in the text box at the top and then pressing the "Get Info"-button the service is started.
After a few seconds (due to subsequent requests to MusicBrainz, mostly due to cover art) the data will be loaded in the page, provided that the MBID was valid.

The shown data is:

1. Artist Name
2. Artist summary (from Wikipedia)
3. Albums with art (list)

Additionally, the website prints the full JSON in the browser's  console, which can be opened by pressing F12 in most modern browsers.

## API (making a request and so on)

The REST-api url for making (GET only) requests is:

```
http://HOSTNAME:PORT/genie/artist/MBID
```

By default hostname is "localhost" and port "55217" which can be verified on the index page url.
Naturally MBID should be replaced by some valid artist MBID.

Example:

```
http://localhost:55217/genie/artist/83d91898-7763-47d7-b03b-b92132375c47
```

Failed requests (invalid MBID, artist not found on MusicBrainz etc.) will result in a 404 Not Found status code.

## Features

Subsequent requests made from this service use caching and retries with exponential backoff.

This is to minimize load on MusicBrainz servers which only allow a set amount of requests per second.

I created a simple page for running a minor load test hosted at http://localhost:55217/benchmark.html.
By specifying the number of requests to be made the page will show how many return successfully.
The page generates requests drawn from a pool of 9 unique MBIDs (hardcoded), meaning caching will quickly kick in and solve the load problem. To properly test the load more mbids should be added or caching turned off, however this risks throttling from MusicBrainz.

## Packages 

These should already be included and working in the project.
No installation SHOULD be required under "normal" circumstances.
However since Visual Studio can be installed with different modules (C++, C#, Python) and so on I chose to specify them below.
Again, they should already be installed or be automatically restored if the previously requirements are met (ASP.NET Web Api).

1. Microsoft.AspNet.WebApi
2. Microsoft.AspNet.WebApi.Client
3. Microsoft.AspNet.WebApi.WebHost
4. Microsoft.CodeDom.Providers.DotNetCompilerPlatform
5. Microsoft.Net.Compilers
6. Newtonsoft.Json

If for some reason the project says a package is missing it can be downloaded in Visual Studio:

```
Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution
```

There should be a button for downloading any missing packages with the text "Restore".
If that does not work, or "restore" is not found the packages should be installable manually:

```
Tools -> NuGet Package Manager -> Package Manager Console
In the console type: Install-Package [PACKAGE NAME]
```
