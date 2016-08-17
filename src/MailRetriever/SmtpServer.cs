using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MailRetriever
{
    public class SmtpServer : IDisposable
    {
        private bool _isDisposed;
        private SocketAsyncEventArgs _acceptEvtArgs;
        private Socket _listener;
        private int _port;

        public IList<EmailMessage> EmailMessages { get; set; }

        public SmtpServer(int port)
        {
            _port = port;
            _acceptEvtArgs = new SocketAsyncEventArgs();
            EmailMessages = new List<EmailMessage>();

            Start();
        }

        private void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _listener.Bind(localEndPoint);
            _listener.Listen(20);

            _acceptEvtArgs.Completed += AcceptCompleted;
            ProcessAccept(_acceptEvtArgs);
        }

        public async Task<EmailMessage> WaitForEmailFromAsync(string emailFrom, int secondsTimeout)
        {
            return await Task.Run(() => SearchForEmail(emailFrom, secondsTimeout));
        }

        private EmailMessage SearchForEmail(string emailFrom, int secondsTimeout)
        {
            int i = 0;
            while (i <= (secondsTimeout * 20))
            {
                Thread.Sleep(50);
                foreach (var emailMessages in EmailMessages.ToList())
                {
                    if (emailMessages.To.Contains(emailFrom))
                    {
                        return emailMessages;
                    }
                }

                i++;
            }

            return null;
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                e.UserToken = new AsyncServerState();
                Send(e, ServerCommands.Welcome);
            }

            ProcessAccept(e);
        }

        private void Send(SocketAsyncEventArgs e, string command, bool finish = false)
        {
            Socket client = e.AcceptSocket;
            SocketAsyncEventArgs handshakeEvent = new SocketAsyncEventArgs();
            handshakeEvent.UserToken = e.UserToken;

            byte[] buffer = GetMessageBytes(command);
            handshakeEvent.SetBuffer(buffer, 0, buffer.Length);
            handshakeEvent.AcceptSocket = client;

            if (!finish)
            {
                handshakeEvent.Completed += Read;
            }

            client.SendAsync(handshakeEvent);
        }

        private void Read(object sender, SocketAsyncEventArgs e)
        {
            Socket client = e.AcceptSocket;
            SocketAsyncEventArgs receiveEvent = new SocketAsyncEventArgs();
            receiveEvent.AcceptSocket = client;
            receiveEvent.Completed += ReadCompleted;
            receiveEvent.UserToken = e.UserToken;
            byte[] buffer = new byte[1024];
            receiveEvent.SetBuffer(buffer, 0, buffer.Length);

            client.ReceiveAsync(receiveEvent);
        }

        private void ReadCompleted(object sender, SocketAsyncEventArgs e)
        {
            var command = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);
            AsyncServerState asyncServerState = e.UserToken as AsyncServerState;

            if (command.Length > 0)
            {
                if (command.StartsWith(ClientCommands.Quit))
                {
                    ProcessQuitCommand(e, asyncServerState);
                }
                else if (command.StartsWith(ClientCommands.Ehlo))
                {
                    ProcessEhloCommand(e);
                }
                else if (command.StartsWith(ClientCommands.Hello))
                {
                    ProcessHeloCommand(e);
                }
                else if (command.StartsWith(ClientCommands.ReceipientTo))
                {
                    ProcessReceipientToCommand(e, command, asyncServerState);
                }
                else if (command.StartsWith(ClientCommands.MailFrom))
                {
                    ProcessMailFromCommand(e, command, asyncServerState);
                }
                else if (command.StartsWith(ClientCommands.PlainAuthentication))
                {
                    ProcessAuthenticationCommand(e);
                }
                else if (command.StartsWith(ClientCommands.Data))
                {
                    ProcessDataCommand(e, asyncServerState);
                }
                else if (command.StartsWith(ClientCommands.EndData))
                {
                    ProcessEndDataCommand(e, asyncServerState);
                }
                else if (asyncServerState.IsProcessingData)
                {
                    ProcessReadingDataCommand(sender, e, command, asyncServerState);
                }
            }
        }

        private void ProcessReadingDataCommand(object sender, SocketAsyncEventArgs e, string command, AsyncServerState asyncServerState)
        {
            if (asyncServerState.EmailMessage.Header == null)
            {
                asyncServerState.EmailMessage.Header = HeaderParser.Parse(command);
            }
            else
            {
                asyncServerState.EmailContet.AppendLine(command);
            }

            if (command.Contains(ClientCommands.EndData))
            {
                asyncServerState.IsProcessingData = false;
                Send(e, ServerCommands.Ok);
            }
            else
            {
                Read(sender, e);
            }
        }

        private void ProcessEndDataCommand(SocketAsyncEventArgs e, AsyncServerState asyncServerState)
        {
            FinishProcessingData(asyncServerState);
            Send(e, ServerCommands.Ok);
        }

        private void ProcessDataCommand(SocketAsyncEventArgs e, AsyncServerState asyncServerState)
        {
            asyncServerState.IsProcessingData = true;
            Send(e, ServerCommands.StartData);
        }

        private void ProcessAuthenticationCommand(SocketAsyncEventArgs e)
        {
            Send(e, ServerCommands.AuthenticationSuccessful);
        }

        private void ProcessMailFromCommand(SocketAsyncEventArgs e, string command, AsyncServerState asyncServerState)
        {
            var email = ClearEmail(ClientCommands.MailFrom, command);
            asyncServerState.EmailMessage.From = email;
            Send(e, ServerCommands.Ok);
        }

        private void ProcessReceipientToCommand(SocketAsyncEventArgs e, string command, AsyncServerState asyncServerState)
        {
            var email = ClearEmail(ClientCommands.ReceipientTo, command);
            asyncServerState.EmailMessage.To.Add(email);
            Send(e, ServerCommands.Ok);
        }

        private void ProcessHeloCommand(SocketAsyncEventArgs e)
        {
            Send(e, ServerCommands.AuthenticationPlainLogin);
        }

        private void ProcessEhloCommand(SocketAsyncEventArgs e)
        {
            Send(e, ServerCommands.AuthenticationPlainLogin);
        }

        private void ProcessQuitCommand(SocketAsyncEventArgs e, AsyncServerState asyncServerState)
        {
            FinishProcessingData(asyncServerState);
            EmailMessages.Add(asyncServerState.EmailMessage);
            Send(e, ServerCommands.Bye, true);
        }

        private void FinishProcessingData(AsyncServerState asyncServerState)
        {
            asyncServerState.IsProcessingData = false;
            if (asyncServerState.EmailMessage.Body == null)
            {
                asyncServerState.EmailMessage.Body = asyncServerState.EmailContet.ToString();
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            e.AcceptSocket = null;
            if (_listener != null && !_listener.AcceptAsync(_acceptEvtArgs))
            {
                AcceptCompleted(null, _acceptEvtArgs);
            }
        }

        private byte[] GetMessageBytes(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message + "\r\n");
            return messageBytes;
        }

        private string ClearEmail(string prefix, string command)
        {
            string email = command.Replace(prefix, string.Empty);
            email = email.Replace("\r\n", string.Empty);
            email = email.Replace(">", string.Empty);
            email = email.Replace("<", string.Empty);

            return email;
        }

        ~SmtpServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                if (_listener != null)
                {
                    _listener.Dispose();
                    _listener = null;
                }
            }

            _isDisposed = true;
        }
    }
}
