using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GautoWebLogin.WebAuth
{
    public class WebLoginException : Exception
    {
        public virtual string ErrorCode
        {
            get { return "WebLoginError"; }
        }
    }

    public class BindEndPointException : WebLoginException
    {
        public override string ErrorCode
        {
            get { return "IPProblem"; }
        }
    }
}
