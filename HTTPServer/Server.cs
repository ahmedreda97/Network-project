using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, portNumber);
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.serverSocket.Bind(iep);

        }

        public void StartServer()
        {
            this.serverSocket.Listen(100);
            
            // TODO: Listen to connections, with large backlog.

            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket clientSocket = serverSocket.Accept();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                clientThread.Start(clientSocket);
            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket clientSock = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSock.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] buffer = new byte[1024];
                    int length = clientSock.Receive(buffer);
                    buffer = buffer.Take(length).ToArray();
                    // TODO: break the while loop if receivedLen==0
                    if(length == 0)
                    {
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    string requestString = ASCIIEncoding.ASCII.GetString(buffer);
                    Request req = new Request(requestString);
                    // TODO: Call HandleRequest Method that returns the response
                   Response res= HandleRequest(req);
                    // TODO: Send Response back to client
                    buffer = new byte[res.ResponseString.Length];
                    buffer = Encoding.ASCII.GetBytes(res.ResponseString);
                    clientSock.Send(buffer);

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    break;
                }
            }

            // TODO: close client socket
            clientSock.Close();
        }

        Response HandleRequest(Request request)
        {
            //throw new NotImplementedException();
            string content;
            string fileName;
            try
            {
                //TODO: check for bad request 
                bool status = request.ParseRequest();
                Console.Write(request.method.ToString());
                if (request.method.ToString() == "HEAD" && status == false)
                {
                    return new Response(StatusCode.BadRequest, "text/html", "", "");
                }
                else if (status == false)
                {
                    fileName = Configuration.BadRequestDefaultPageName;
                    return new Response(StatusCode.BadRequest, "text/html", File.ReadAllText(Configuration.RootPath + '\\' + fileName), fileName);
                }
                //TODO: map the relativeURI in request to get the physical path of the resource.
                string relativePath = request.relativeURI;
                //TODO: check for redirect
                string redPath = GetRedirectionPagePathIFExist(relativePath);
                if (request.method.ToString() == "HEAD" && redPath != string.Empty)
                {
                    return new Response(StatusCode.Redirect, "text/html", "", "");
                }
                else if (redPath != string.Empty)
                {
                    fileName = Configuration.RedirectionDefaultPageName;

                    return new Response(StatusCode.Redirect, "text/html", File.ReadAllText(Configuration.RootPath + '\\' + fileName), redPath);
                }
                if (request.method.ToString() == "HEAD" && relativePath == "/")
                {
                    return new Response(StatusCode.OK, "text/html", "", "");
                }
                else if (relativePath == "/")
                {
                    fileName = "main.html";
                    return new Response(StatusCode.OK, "text/html", File.ReadAllText(Configuration.RootPath + '\\' + fileName), null);
                }
                //TODO: check file exists
                if (request.method.ToString() == "HEAD" && !File.Exists(Configuration.RootPath + '\\' + relativePath))
                {
                    return new Response(StatusCode.NotFound, "text/html", "", "");
                }
                else if (!File.Exists(Configuration.RootPath+'\\'+ relativePath))
                {
                    //aa
                    fileName = Configuration.NotFoundDefaultPageName;
                    return new Response(StatusCode.NotFound, "text/html",File.ReadAllText(Configuration.RootPath + '\\' + fileName), null);
                }

                if (request.method.ToString() == "HEAD" )
                {
                    return new Response(StatusCode.OK, "text/html", "", "");
                }
                else
                {
                    //TODO: read the physical file
                    content = File.ReadAllText(Configuration.RootPath + '\\' + relativePath);
                    // Create OK response
                    return new Response(StatusCode.OK, "text/html", content, relativePath);
                }
                
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                fileName = Configuration.InternalErrorDefaultPageName;
                // TODO: in case of exception, return Internal Server Error. 
                return new Response(StatusCode.InternalServerError, "text/html", File.ReadAllText(Configuration.RootPath + '\\' + fileName),null);
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
            {
                return Configuration.RedirectionRules[relativePath];
            }
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            try
            {
                return File.ReadAllText(filePath);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
                return string.Empty;
            }
            // else read file and return its content
            
        }

        private void LoadRedirectionRules(string filePath)
        {
            string line;
            try
            {
                StreamReader sr = new StreamReader(filePath);
                // TODO: using the filepath paramter read the redirection rules from file 
                while((line = sr.ReadLine()) != null)
                {
                    string[] splited = line.Split(',');
                    Configuration.RedirectionRules.Add(splited[0], splited[1]);
                }
                sr.Close();
                // then fill Configuration.RedirectionRules dictionary 
                
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
