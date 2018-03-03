/*
 * Copyright (c) 2013 avatar29A
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Hqub.MusicBrainz.API
{
    /// <summary>
    /// This exception class can be used to display message in Windows Service Apps
    /// ShowMessageAsync requires a tile and description
    /// </summary>
    public class ReadableException : Exception
    {
        protected string _errorMessage;
        protected string _errorDescription;

        public ReadableException() { }

        public ReadableException(string message, string description)
            : base(message + ": " + description)
        {
            _errorMessage = message;
            _errorDescription = description;
        }

        public ReadableException(string message, string description, Exception inner)
            : base(message + ": " + description, inner)
        {
            _errorMessage = message;
            _errorDescription = description;
        }

        public string Error { get { return _errorMessage; } }
        public string Description { get { return _errorDescription; } }
    }
}