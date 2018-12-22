using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLines;
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        string requestString;
        string[] contentLines;
        string[] r_lines;
        string[] header_lines;
        public Request(string requestString)
        {
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            // throw new NotImplementedException();
            //TODO: parse the receivedRequest using the \r\n delimeter   
            int crlf_count = 0;
            int header_count = 0;
            bool stop = false;
            string request_line = "";
            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            for (int i = 0; i < requestString.Count(); i++)
            {
                request_line += requestString[i];
                if (requestString[i] == '\r' && requestString[i + 1] == '\n')
                {
                    if (crlf_count >= 1 && stop == false)
                    {
                        header_lines[header_count] = request_line;
                        header_count++;
                    }
                    r_lines[crlf_count] = request_line;
                    crlf_count++;
                    i++;
                    if (requestString[i] == '\r' && requestString[i + 1] == '\n')
                    {
                        stop = true;
                    }
                }
            }
            if (!(crlf_count >= 3))
            {
                return false;
            }
            // Parse Request line
            string[] split = r_lines[0].Split(' ');
            int count = 0;
            foreach (var x in split)
            {
                requestLines[count] = x;
                count++;
            }
            bool parserequestline = ParseRequestLine();
            if (parserequestline == false)
            {
                return false;
            }
            // Validate blank line exists
            bool blank_lines = ValidateBlankLine();
            if (blank_lines == false)
            {
                return false;
            }
            // Load header lines into HeaderLines dictionary
            bool load_headerlines = LoadHeaderLines();
            if (load_headerlines == false)
            {
                return false;
            }
            return true;
        }

        private bool ParseRequestLine()
        {
            //throw new NotImplementedException();

            if (requestLines[0] == "GET")
            {
                method = RequestMethod.GET;
            }
            else if (requestLines[0] == "HEAD")
            {
                method = RequestMethod.HEAD;
            }
            else if (requestLines[0] == "POST")
            {
                method = RequestMethod.POST;
            }
            else
            {
                return false;
            }

            bool valid_URI = ValidateIsURI(requestLines[1]);
            if (valid_URI == false)
            {
                return false;
            }
            else
            {
                relativeURI = requestLines[1];
            }

            if (requestLines[2] == "HTTP/1.0")
            {
                httpVersion = HTTPVersion.HTTP10;
            }
            else if (requestLines[2] == "HTTP/1.1")
            {
                httpVersion = HTTPVersion.HTTP11;
            }
            else if (requestLines[2] == "HTTP/0.9" || requestLines[2] == "HTTP")
            {
                httpVersion = HTTPVersion.HTTP09;
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {
            //  throw new NotImplementedException();
            if (httpVersion == HTTPVersion.HTTP11)
            {
                if (header_lines.Contains("Host") == false)
                {
                    return false;
                }
            }
            headerLines = new Dictionary<string, string>();
            string[] Separators = new string[] { ": " };
            for (int i = 0; i < header_lines.Count(); i++)
            {
                string[] split = header_lines[i].Split(Separators, StringSplitOptions.None);
                headerLines.Add(split[0], split[1]);
            }
            return true;
        }

        private bool ValidateBlankLine()
        {
            // throw new NotImplementedException();
            if (method == RequestMethod.GET || method == RequestMethod.HEAD)
            {

                if (requestString.EndsWith("\r\n\r\n") == false)
                {
                    return false;
                }
            }
            else if (method == RequestMethod.POST)
            {
                if (requestString.EndsWith("\r\n") == false)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
