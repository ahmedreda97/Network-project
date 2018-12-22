using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
            //throw new NotImplementedException();
            // TODO: Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])
            headerLines.Add("Content-Type:" + "text/html");
            headerLines.Add("Content-Length:" + (content.Length).ToString());
            headerLines.Add("Date:" + (DateTime.UtcNow).ToString("F"));
            headerLines.Add(redirectoinPath);

            // TODO: Create the request string
            responseString = string.Empty;
            responseString = GetStatusLine(code);
            responseString += "\r\n";
            foreach (string s in headerLines)
            {
                responseString += s;
                responseString += "\r\n";
            }
            responseString += "\r\n";
            responseString += content;

        }

        private string GetStatusLine(StatusCode code)
        {
            // TODO: Create the response status line and return it
            string statusLine = string.Empty;

            statusLine += Configuration.ServerHTTPVersion;
            statusLine += " ";
            statusLine += code.ToString();
            statusLine += " ";

            if (code == StatusCode.OK)
                statusLine += "OK";
            else if (code == StatusCode.NotFound)
                statusLine += "Not Found";
            else if (code == StatusCode.InternalServerError)
                statusLine += "Internal Server Error";
            else if (code == StatusCode.BadRequest)
                statusLine += "Bad Request";
            else
                statusLine += "Redirect";

            return statusLine;
        }
    }
}
