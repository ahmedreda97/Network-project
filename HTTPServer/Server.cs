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
                }
            }

            // TODO: close client socket
            clientSock.Close();
        }

        Response HandleRequest(Request request)
        {
            //throw new NotImplementedException();
            string content;
            try
            {
                //TODO: check for bad request 
                bool status=request.ParseRequest();
                //TODO: map the relativeURI in request to get the physical path of the resource.
                string relativePath = request.relativeURI; //maybe wrong
                //TODO: check for redirect
                string path = GetRedirectionPagePathIFExist(relativePath);
                //TODO: check file exists
                if (!File.Exists(path))
                {
                    string fileName = Configuration.NotFoundDefaultPageName;
                    return new Response(StatusCode.NotFound, "text/html",File.ReadAllText(fileName), fileName);
                }
                //TODO: read the physical file
                 content = File.ReadAllText(path);
                // Create OK response
                
                return new Response(StatusCode.OK, "text/html", content, path);
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                string fileName = Configuration.InternalErrorDefaultPageName;
                // TODO: in case of exception, return Internal Server Error. 
                return new Response(StatusCode.InternalServerError, "text/html", File.ReadAllText(fileName),fileName);
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
