using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Tool
{
#if NETCOREAPP
#else
    /// <summary>HTTP/2に対応してないHttpClientを対応させるやつ</summary>
    public class Http2CustomHandler : System.Net.Http.WinHttpHandler //4.6.1はnugetでSystem.Net.Http.WinHttpHandlerを参照
    {
        public Http2CustomHandler()
        {

        }
        protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Version = new Version("2.0");
            return base.SendAsync(request, cancellationToken);
        }
    }
#endif
}
