using Brotli;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace GautoWebLogin.WebAuth
{
    public class BindableRequest : HttpRequest
    {
        public HttpWebRequest request;

        public RestRequest restRequest;
        public RestClient restClient;

        public bool bindEndPointProblem;

        public virtual void create(string url)
        {
            this.request = (HttpWebRequest)WebRequest.Create(url);
            this.request.Timeout = 30000;

            this.restRequest = new RestRequest();
            this.restClient = new RestClient(url)
            {
                UserAgent = "",
                AutomaticDecompression = false
            };
        }

        public virtual void create(Uri url)
        {
            this.request = (HttpWebRequest)WebRequest.Create(url);
            this.request.Timeout = 30000;

            this.restRequest = new RestRequest();
            this.restClient = new RestClient(url.ToString())
            {
                UserAgent = "",
                AutomaticDecompression = false
            };
        }

        public void setBindAddress(string ipaddr)
        {
            if (string.IsNullOrEmpty(ipaddr))
                return;

            #region HttpWebRequest
            ServicePointManager.MaxServicePoints = 200;
            request.ServicePoint.MaxIdleTime = 0;
            request.ServicePoint.ConnectionLimit = 1;

            request.ServicePoint.BindIPEndPointDelegate = delegate (ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
            {
                if (retryCount >= 5)
                {
                    bindEndPointProblem = true;
                    throw new InvalidDataException("BindIPEndPoint");
                }

                try
                {
                    if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        return new IPEndPoint(IPAddress.IPv6Any, 0);
                    else
                        return new IPEndPoint(IPAddress.Parse(ipaddr), 0);
                }
                catch (Exception)
                {
                    bindEndPointProblem = true;
                    throw new InvalidDataException("BindIPEndPoint");
                }
            };

            request.ServicePoint.ConnectionLeaseTimeout = 0;
            request.KeepAlive = false;
            #endregion

            #region RestClient
            restClient.ConfigureWebRequest(delegate (HttpWebRequest request)
            {
                ServicePointManager.MaxServicePoints = 200;
                request.ServicePoint.MaxIdleTime = 0;
                request.ServicePoint.ConnectionLimit = 1;
                request.ServicePoint.BindIPEndPointDelegate = delegate (ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
                {
                    if (retryCount >= 5)
                    {
                        throw new BindEndPointException();
                    }
                    return new IPEndPoint(IPAddress.Parse(ipaddr), 0);
                };
                request.ServicePoint.ConnectionLeaseTimeout = 0;
                request.KeepAlive = false;
            });
            #endregion
        }

        public string getResponseString(string postData)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (postData != null)
                {
                    byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
                    request.Method = "POST";
                    request.ContentLength = sendData.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(sendData, 0, sendData.Length);
                    requestStream.Close();
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    return null;

                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public string getResponseString(string postData, out WebHeaderCollection headers)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (postData != null)
                {
                    byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
                    request.Method = "POST";
                    request.ContentLength = sendData.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(sendData, 0, sendData.Length);
                    requestStream.Close();
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    headers = response.Headers;
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            if (response.Headers["Content-Encoding"] == null)
                            {
                                return reader.ReadToEnd();
                            }
                            else if (response.Headers["Content-Encoding"].Contains("gzip"))
                            {
                                var memoryStream = new MemoryStream();
                                stream.CopyTo(memoryStream);
                                var gzStream = new GZipStream(new MemoryStream(memoryStream.ToArray()), CompressionMode.Decompress);

                                MemoryStream outStream = new MemoryStream();
                                byte[] buffer = new byte[1024];
                                int nRead;
                                while ((nRead = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    outStream.Write(buffer, 0, nRead);
                                }

                                var array = outStream.ToArray();
                                return Encoding.UTF8.GetString(array);
                            }
                            else if (response.Headers["Content-Encoding"].Contains("br"))
                            {
                                using (BrotliStream bs = new BrotliStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))
                                {
                                    using (System.IO.MemoryStream msOutput = new System.IO.MemoryStream())
                                    {
                                        bs.CopyTo(msOutput);
                                        msOutput.Seek(0, System.IO.SeekOrigin.Begin);
                                        using (StreamReader rr = new StreamReader(msOutput))
                                        {
                                            return rr.ReadToEnd();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return reader.ReadToEnd();
                            }

                        }
                    }
                }
            }
            catch (WebException ex)
            {
                headers = null;
                if (ex.Response == null)
                    return null;

                headers = ex.Response.Headers;
                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public byte[] getResponseBytes(byte[] sendData)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (sendData != null)
                {
                    request.Method = "POST";
                    request.ContentLength = sendData.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(sendData, 0, sendData.Length);
                    requestStream.Close();
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (response.Headers["Content-Encoding"] == null)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                return ms.ToArray();
                            }
                        }
                        else if (response.Headers["Content-Encoding"].Contains("gzip"))
                        {
                            var memoryStream = new MemoryStream();
                            stream.CopyTo(memoryStream);
                            var gzStream = new GZipStream(new MemoryStream(memoryStream.ToArray()), CompressionMode.Decompress);

                            MemoryStream outStream = new MemoryStream();
                            byte[] buffer = new byte[1024];
                            int nRead;
                            while ((nRead = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outStream.Write(buffer, 0, nRead);
                            }

                            return outStream.ToArray();
                        }
                        else
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                return ms.ToArray();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    return null;

                using (var stream = ex.Response.GetResponseStream())
                {
                    if (ex.Response.Headers["Content-Encoding"] != null && ex.Response.Headers["Content-Encoding"].Contains("gzip"))
                    {
                        var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        var gzStream = new GZipStream(new MemoryStream(memoryStream.ToArray()), CompressionMode.Decompress);

                        MemoryStream outStream = new MemoryStream();
                        byte[] buffer = new byte[1024];
                        int nRead;
                        while ((nRead = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, nRead);
                        }

                        return outStream.ToArray();
                    }
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

        public string getRestResponse(byte[] postData)
        {
            try
            {
                if (postData != null)
                {
                    restRequest.Method = Method.POST;
                    restRequest.AddParameter(string.Empty, postData, ParameterType.RequestBody);
                }

                //RestClientExtensions.RemoveDefaultParameter(restClient, "Accept-Encoding");
                //RestClientExtensions.RemoveDefaultParameter(restClient, "Accept");
                restClient.ClearHandlers();
                IRestResponse restResponse = restClient.Execute(restRequest);

                var responseBytes = restResponse.RawBytes;
                if (restResponse.ContentEncoding.Contains("gzip"))
                {
                    var memoryStream = new MemoryStream(restResponse.RawBytes);
                    var gzStream = new GZipStream(new MemoryStream(memoryStream.ToArray()), CompressionMode.Decompress);

                    MemoryStream outStream = new MemoryStream();
                    byte[] buffer = new byte[1024];
                    int nRead;
                    while ((nRead = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, nRead);
                    }
                    responseBytes = outStream.ToArray();
                }

                var result = Encoding.UTF8.GetString(responseBytes);
                return result;
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    return ex.ToString();

                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
