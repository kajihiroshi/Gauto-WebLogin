using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GautoWebLogin.WebAuth
{
    public interface HttpRequest
    {
        void create(string url);

        string getResponseString(string postData);

        byte[] getResponseBytes(byte[] postData);

        //string getResponse(byte[] postData, string method);
    }
}
