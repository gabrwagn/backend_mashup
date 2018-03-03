# backend_mashup

Small proof of concept for home assignment.

## Requirements: 

Visual Studio 2017 (earlier versions not tested).

## Installation

Load the MusicGenie/MusicGenie.sln in Visual Studio.

Run the server with F5 (or the green arrow).
Your default browser should open at an index page used for demonstrating the service.

By typing in a MBID in the text box at the top and then pressing the "Get Info"-button the service is started.
After a few seconds (due to subsequent requests to MusicBrainz, mostly due to cover art) the data will be loaded in the page, provided that the MBID was valid.

The shown data is:

1. Artist Name
2. Artist summary (from Wikipedia)
3. Albums with art (list)

## Making a request

To make requests from other applications etc. the url to make the GET-request to is:

```
http://HOSTNAME:PORT/api/artist/MBID
```
By default hostname is "localhost" and port "55217" which can be verified on the index page url.
Naturally MBID should be replaced by the corresponding MBID of the artist retrieve info about.
