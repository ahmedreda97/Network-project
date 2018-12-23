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
        List<string> requestLines = new List<string>();
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        string requestString = null;
        List<string> contentLines = new List<string>();
        List<string> r_lines = new List<string>();
        List<string> header_lines = new List<string>();
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
            //int header_count = 0;
            bool stop = false;
            string request_line = "";
            string content = "";
            int content_size = 0;
            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            for (int i = 0; i < requestString.Count(); i++)
            {

                if (requestString[i] == '\r' && requestString[i + 1] == '\n')
                {
                    if (crlf_count >= 1 && stop == false)
                    {

                        header_lines.Add(request_line);
                        request_line = null;
                    }
                    r_lines.Add(request_line);
                    crlf_count++;
                    i++;
                    request_line = null;
                    if (requestString[i] == '\r' && requestString[i + 1] == '\n')
                    {
                        stop = true;
                    }
                }
                request_line += requestString[i];
            }
            if (!(crlf_count >= 3))
            {
                return false;
            }
            // Parse Request line
            string[] split = r_lines[0].Split(' ');
            foreach (var x in split)
            {
                //requestLines[count] = x;
                requestLines.Add(x);
            }
            bool parserequestline = ParseRequestLine();
            if (parserequestline == false)
            {
                return false;
            }

            if (method == RequestMethod.POST)
            {
                // get the length of the content in case of POST request
                foreach (var header in header_lines)
                {
                    if (header.Contains("Content-Length") || header.ToLower().Contains("content-length"))
                    {
                        string[] selectedHeader = header.Split(':');
                        string temp = selectedHeader[selectedHeader.Count() - 1].Trim().ToString();
                        content_size = int.Parse(temp);
                    }
                }

                string newRequestString = string.Empty;
                for (int s = 0; s < requestString.Count(); s++)
                {
                    if (s < requestString.Count() - content_size)
                    {
                        newRequestString += requestString[s];
                    }
                    else
                    {
                        content += requestString[s];
                    }
                }

                this.requestString = newRequestString;
            }

            // Validate blank line exists
            Console.Write(content);
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
                if (!containsHead())
                {
                    return false;
                }
            }
            headerLines = new Dictionary<string, string>();
            string[] Separators = new string[] { ": " };
            for (int i = 0; i < header_lines.Count() - 1; i++)
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
        private bool containsHead()
        {
            
            foreach(string val in header_lines)
            {
                if (val.Contains("Host"))
                {
                    return true;
                }

            }
            return false;

        }

    }
}
