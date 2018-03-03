/*
 * Copyright (c) 2013 avatar29A
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hqub.MusicBrainz.API
{
    public class HttpClientException : ReadableException
    {
        public HttpClientException(HttpStatusCode statusCode)
        {
            // if more than 1 request per second per ip (on average)
            if (statusCode == HttpStatusCode.ServiceUnavailable)
            {
                _errorMessage = "Please try again";
                _errorDescription = "The server is temporarily unavailable, usually due to high load or maintenance.";
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                _errorMessage = "Error 404";
                _errorDescription = "The requested resource does not exist on the server.";
            }
            else
            {
                _errorMessage = "Network Error";
                _errorDescription = statusCode.ToString();
            }
        }

        public HttpClientException(int hresult)
        {
            uint errorCode = (uint)hresult;
            // 0x80072F30 -> when not connected to wifi/3g
            // 0x80072EE7 -> When invalid domain name couldn't be found
            // 0x80190194 -> When ensureSuccessCode fails -> error: 404

            _errorMessage = "Network Problem";
            _errorDescription = "You are not connected to the Internet.";
        }
    }
}
