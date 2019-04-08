using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using log4net;
using Toec_Services;
using Toec_Services.Entity;

namespace Toec_RemoteApi.Controllers.Authorization
{
    public class SignatureAuth : Attribute, IAuthenticationFilter
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string authenticationScheme = "amx";
        private readonly ulong requestMaxAgeInSeconds = 600; //5 mins
        private string logId;

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            logId = Guid.NewGuid().ToString("n").Substring(0, 8);
            var req = context.Request;
            Logger.Debug($"ID: {logId} - Received remote api auth request");
            Logger.Debug($"ID: {logId} - Request URI {req.RequestUri} ");

            if (req.Headers.Authorization != null &&
                authenticationScheme.Equals(req.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var rawAuthzHeader = req.Headers.Authorization.Parameter;
                Logger.Debug($"ID: {logId} - {rawAuthzHeader}");
                var autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthzHeader);

                if (autherizationHeaderArray != null)
                {
                    var incomingBase64Signature = autherizationHeaderArray[0];
                    Logger.Debug($"ID: {logId} - Received Signature: " + incomingBase64Signature);
                    var nonce = autherizationHeaderArray[1];
                    var requestTimeStamp = autherizationHeaderArray[2];

                    var isValid = isValidRequest(req, incomingBase64Signature, nonce, requestTimeStamp);

                    if (isValid.Result)
                    {
                        var currentPrincipal = new GenericPrincipal(new GenericIdentity("server"), null);
                        context.Principal = currentPrincipal;
                    }
                    else
                    {
                        Logger.Debug($"ID: {logId} - Authorization failed validation");
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                    }
                }
                else
                {
                    Logger.Debug($"ID: {logId} - Authorization header was null");
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                }
            }
            else
            {
                Logger.Debug($"ID: {logId} - Authorization header was null");
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultWithChallenge(context.Result);
            return Task.FromResult(0);
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        private static async Task<byte[]> ComputeHash(HttpContent httpContent)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = null;
                var content = await httpContent.ReadAsByteArrayAsync();
                if (content.Length != 0)
                {
                    hash = md5.ComputeHash(content);
                }
                return hash;
            }
        }

        private string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            var credArray = rawAuthzHeader.Split(':');

            if (credArray.Length == 3)
            {
                return credArray;
            }
            return null;
        }

        private bool isReplayRequest(string nonce, string requestTimeStamp)
        {
            if (MemoryCache.Default.Contains(nonce))
            {
                Logger.Debug($"ID: {logId} - This nonce has already been used");
                return true;
            }

            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var currentTs = DateTime.UtcNow - epochStart;

            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            Logger.Debug($"ID: {logId} - Server Timestamp Seconds " + serverTotalSeconds);
            Logger.Debug($"ID: {logId} - Request Timestamp Seconds " + requestTimeStamp);

            if (requestTotalSeconds > serverTotalSeconds)
            {
                Logger.Debug($"ID: {logId} - Server time is behind client, allowing 5 minute discrepancy");
                //server time is behind client, give it a 5 min window
                serverTotalSeconds += 300;
            }

            var timeStampDifference = serverTotalSeconds - requestTotalSeconds;
            Logger.Debug($"ID: {logId} - Timestamp difference: " + timeStampDifference);
            if (timeStampDifference > requestMaxAgeInSeconds)
            {
                Logger.Debug($"ID: {logId} - Request has exceeded the maximum request age of 600 seconds");
                return true;
            }

            MemoryCache.Default.Add(nonce, requestTimeStamp,
                DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));

            return false;
        }

        private async Task<bool> isValidRequest(HttpRequestMessage req, string incomingBase64Signature, string nonce,
            string requestTimeStamp)
        {
            var requestContentBase64String = "";
            var requestUri = HttpUtility.UrlEncode(req.RequestUri.AbsoluteUri.ToLower());
            var requestHttpMethod = req.Method.Method;

            if (isReplayRequest(nonce, requestTimeStamp))
            {
                Logger.Debug($"ID: {logId} - Request appears to be a replay, denying {nonce} {requestTimeStamp}");
                return false;
            }

            var hash = await ComputeHash(req.Content);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            var data = string.Format("{0}{1}{2}{3}{4}", requestHttpMethod, requestUri, requestTimeStamp, nonce,
                requestContentBase64String);
            Logger.Debug($"ID: {logId} - Expected Signature Data " + data);
            var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null)
            {
                Logger.Error("Could Not Find The Device Certificate For Signature Verification.");
                return false;
            }

            if (!ServiceCertificate.VerifySignature(deviceCert, Convert.FromBase64String(incomingBase64Signature), data))
            {
                return false;
            }
            return true;
        }
    }

    public class ResultWithChallenge : IHttpActionResult
    {
        private readonly string authenticationScheme = "amx";
        private readonly IHttpActionResult next;

        public ResultWithChallenge(IHttpActionResult next)
        {
            this.next = next;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await next.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authenticationScheme));
            }

            return response;
        }
    }
}