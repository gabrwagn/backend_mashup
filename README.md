# MusicGenie (backend_mashup)

Small proof of concept for home assignment which I nicknamed MusicGenie.

Quick summary: Mash together apis that serve Album Information, Cover Art and Wikipedia summary into a neat JSON package.

## Requirements: 

Visual Studio 2017 (earlier versions not tested).

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

## Making a request

To make requests from other applications etc. the url to make the GET-request to is:

```
http://HOSTNAME:PORT/genie/artist/MBID
```

By default hostname is "localhost" and port "55217" which can be verified on the index page url.
Naturally MBID should be replaced by some valid artist MBID.

Example:

```
http://localhost:55217/genie/artist/83d91898-7763-47d7-b03b-b92132375c47
```

## Features

Subsequent requests made from this service use caching and retries with exponential backoff.

This is to minimize load on MusicBrainz servers which only allow a set amount of requests per second.

I created a simple page for running a minor load test hosted at http://localhost:55217/benchmark.html.
By specifying the number of requests to be made the page will show how many return successfully.
The page generates requests drawn from a pool of 5 unique MBIDs, meaning caching will quickly kick in and solve the load problem.
To properly test the load more mbids should be added.

# Left out of proof of concept

I chose to have in memory caching instead of storing anything in a database since this is a proof of concept.
Additionally a larger range of tests should be performed in a real solution. Such as generating heavy load etc.
