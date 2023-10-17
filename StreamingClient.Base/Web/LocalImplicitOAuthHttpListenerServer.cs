using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace StreamingClient.Base.Web
{
    /// <summary>
    /// An Http listening server for processing OAuth requests.
    /// </summary>
    public class LocalImplicitOAuthHttpListenerServer : LocalHttpListenerServer
    {
        private const string defaultSuccessResponse = "<!DOCTYPE html><html><body><h1 style=\"text-align:center;\">Logged In Successfully</h1><p style=\"text-align:center;\">You have been logged in, you may now close this webpage</p></body></html>";
        
        private const string implicitFlowRedirect = @"
<!DOCTYPE html>
<html>
    <script language=""javascript"" type=""text/javascript"">
        function grabHash() {
            var hashForm = document.forms.hashForm;
            hashForm.hashField.value = document.location.hash;
            hashForm.submit();
        }
    </script>
    <body onload=""grabHash()"">
        <h1 style=""text-align:center;"">Please Wait...</h1>
        <p style=""text-align:center;"">Waiting for authentication from Twitch...</p>
        <form id=""hashForm"" method=""post"">
            <input type=""hidden"" id=""hash"" name=""hashField"">
        </form>
    </body>
</html>
";

        private string accessTokenParameterName = null;
        private string successResponse = null;

        private string accessToken = null;

        /// <summary>
        /// Creates a new instance of the LocalOAuthHttpListenerService with the specified address.
        /// </summary>
        /// <param name="accessTokenParameterName">The name of the parameter for the authorization code</param>
        public LocalImplicitOAuthHttpListenerServer(string accessTokenParameterName)
        {
            this.accessTokenParameterName = accessTokenParameterName;
        }

        /// <summary>
        /// Creates a new instance of the LocalOAuthHttpListenerService with the specified address &amp; login response.
        /// </summary>
        /// <param name="accessTokenParameterName">The name of the parameter for the authorization code</param>
        /// <param name="successResponse">The response to send upon successfully obtaining an authorization token</param>
        public LocalImplicitOAuthHttpListenerServer(string accessTokenParameterName, string successResponse)
            : this(accessTokenParameterName)
        {
            this.successResponse = successResponse;
        }

        /// <summary>
        /// Waits for a successful authorization response from the OAuth service.
        /// </summary>
        /// <param name="secondsToWait">The total number of seconds to wait</param>
        /// <returns>The authorization token from the OAuth service</returns>
        public async Task<string> WaitForAccessToken(int secondsToWait = 30)
        {
            for (int i = 0; i < secondsToWait; i++)
            {
                if (!string.IsNullOrEmpty(this.accessToken))
                {
                    break;
                }
                await Task.Delay(1000);
            }
            return this.accessToken;
        }

        /// <summary>
        /// Processes an http request.
        /// </summary>
        /// <param name="listenerContext">The context of the request</param>
        /// <returns>An awaitable task to process the request</returns>
        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string result = string.Empty;

            if (listenerContext.Request.HttpMethod == "GET")
            {
                statusCode = HttpStatusCode.OK;
                result = implicitFlowRedirect;
            }
            else if (listenerContext.Request.HttpMethod == "POST")
            {
                string postData;
                using (var reader = new StreamReader(listenerContext.Request.InputStream, listenerContext.Request.ContentEncoding))
                {
                    postData = reader.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(postData))
                {
                    postData = HttpUtility.UrlDecode(postData);

                    string token = this.GetParameterFromString(postData, this.accessTokenParameterName);
                    if (!string.IsNullOrEmpty(token))
                    {
                        statusCode = HttpStatusCode.OK;
                        result = defaultSuccessResponse;
                        if (!string.IsNullOrEmpty(this.successResponse))
                        {
                            result = successResponse;
                        }

                        this.accessToken = token;
                    }
                }
            }


            await this.CloseConnection(listenerContext, statusCode, result);
        }
    }
}
