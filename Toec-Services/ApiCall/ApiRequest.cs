using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using RestSharp;
using Toec_Common.Dto;
using Toec_Services.Crypto;
using Toec_Services.Entity;

namespace Toec_Services.ApiCall
{
    public class ApiRequest
    {
        private readonly RestClient _client;
        private readonly ILog _log = LogManager.GetLogger(typeof (ApiRequest));

        public ApiRequest()
        {
            _client = new RestClient();
            _client.BaseUrl = new Uri(DtoGobalSettings.ComServer);
            _client.Timeout = 120000; //120 seconds
        }

        public ApiRequest(int timeOut)
        {
            _client = new RestClient();
            _client.BaseUrl = new Uri(DtoGobalSettings.ComServer);
            _client.Timeout = timeOut;
        }

        public ApiRequest(string baseUrl)
        {
            _client = new RestClient();
            _client.BaseUrl = new Uri(baseUrl);
            _client.Timeout = 120000; //120 seconds
        }

        public TClass Execute<TClass>(RestRequest request) where TClass : new()
        {
            return SubmitRequest<TClass>(request);
        }

        public TClass ExecuteHMAC<TClass>(RestRequest request, string computerName) where TClass : new()
        {
            //Calculate UNIX time
            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - epochStart;
            var requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            var nonce = Guid.NewGuid().ToString("N");

            var url =
                HttpUtility.UrlEncode(_client.BaseUrl + request.Resource).ToLower();

            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            var requestContentBase64String = string.Empty;
            if (body != null)
            {
                var content = Encoding.ASCII.GetBytes(body.Value.ToString());
                var md5 = MD5.Create();
                var requestContentHash = md5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            var signatureRawData = string.Format("{0}{1}{2}{3}{4}{5}", computerName, request.Method, url,
                requestTimeStamp, nonce, requestContentBase64String);
            var serviceSetting = new ServiceSetting();
            var serverKeyEntropy = serviceSetting.GetSetting("server_key_entropy");
            var encryptedServerKey = serviceSetting.GetSetting("server_key");
            var decryptedServerKey = ServiceDP.DecryptData(Convert.FromBase64String(encryptedServerKey.Value), true,
                Convert.FromBase64String(serverKeyEntropy.Value));

            var signature = Encoding.UTF8.GetBytes(signatureRawData);
            string requestSignatureBase64String;
            using (var hmac = new HMACSHA256(decryptedServerKey))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
            }

            request.AddHeader("Authorization",
                "amx " +
                string.Format("{0}:{1}:{2}:{3}", computerName, requestSignatureBase64String, nonce, requestTimeStamp));
            return SubmitRequest<TClass>(request);
        }

        public bool DownloadFile(RestRequest request, string body, string destination)
        {
            if (string.IsNullOrEmpty(body))
                throw new ArgumentException("body");

            request.AddHeader("client", DtoGobalSettings.ClientIdentity.Name);
            request.AddHeader("identifier", DtoGobalSettings.ClientIdentity.Guid);
            var serviceSetting = new ServiceSetting();
            var entropy = serviceSetting.GetSetting("entropy");
            var encryptedKey = serviceSetting.GetSetting("encryption_key");
            var decryptedKey = ServiceDP.DecryptData(Convert.FromBase64String(encryptedKey.Value), true,
                Convert.FromBase64String(entropy.Value));

            var encryptedContent = new ServiceSymmetricEncryption().EncryptData(decryptedKey, body);
            request.AddParameter("text/xml", encryptedContent, ParameterType.RequestBody);

            var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null) return false;
            var encryptedCert = new ServiceSymmetricEncryption().EncryptData(decryptedKey,
                Convert.ToBase64String(deviceCert.RawData));
            request.AddHeader("device_cert", Convert.ToBase64String(encryptedCert));

            try
            {
                _log.Debug(request.Resource);
                using (var stream = File.Create(destination, 4096))
                {
                    request.ResponseWriter = (responseStream) => responseStream.CopyTo(stream);
                    _client.DownloadData(request);
                    if (stream.Length == 0)
                    {
                        //something went wrong, rest sharp can't display any other info with downloaddata, so we don't know why
                        return false;

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Could Not Save File: " + destination);
                _log.Error(ex.Message);
                return false;
            }
          
        }

        public byte[] ExecuteRawHmac(RestRequest request, string computerName)
        {
            //Calculate UNIX time
            var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - epochStart;
            var requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            var nonce = Guid.NewGuid().ToString("N");

            var url =
                HttpUtility.UrlEncode(_client.BaseUrl + request.Resource.ToLower());

            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            var requestContentBase64String = string.Empty;
            if (body != null)
            {
                var content = Encoding.ASCII.GetBytes(body.Value.ToString());
                var md5 = MD5.Create();
                var requestContentHash = md5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            var signatureRawData = string.Format("{0}{1}{2}{3}{4}{5}", computerName, request.Method, url,
                requestTimeStamp, nonce, requestContentBase64String);
            var serviceSetting = new ServiceSetting();
            var serverKeyEntropy = serviceSetting.GetSetting("server_key_entropy");
            var encryptedServerKey = serviceSetting.GetSetting("server_key");
            var decryptedServerKey = ServiceDP.DecryptData(Convert.FromBase64String(encryptedServerKey.Value), true,
                Convert.FromBase64String(serverKeyEntropy.Value));

            var signature = Encoding.UTF8.GetBytes(signatureRawData);
            string requestSignatureBase64String;
            using (var hmac = new HMACSHA256(decryptedServerKey))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
            }

            request.AddHeader("Authorization",
                "amx " +
                string.Format("{0}:{1}:{2}:{3}", computerName, requestSignatureBase64String, nonce, requestTimeStamp));
         
            var response = _client.DownloadData(request);

            if (response == null)
            {
                _log.Error("Could Not Complete Raw API Request.  The Response was empty." + request.Resource);
                return null;
            }

            if (response.Length == 0)
            {
                _log.Error("Could Not Complete Raw API Request.  The Response was empty." + request.Resource);
                return null;
            }

            _log.Debug(request.Resource);
            return response;
        }

        public TClass ExecuteSymKeyEncryption<TClass>(RestRequest request, string body) where TClass : new()
        {
            request.AddHeader("client", DtoGobalSettings.ClientIdentity.Name);
            request.AddHeader("identifier", DtoGobalSettings.ClientIdentity.Guid);
            var serviceSetting = new ServiceSetting();
            var entropy = serviceSetting.GetSetting("entropy");
            var encryptedKey = serviceSetting.GetSetting("encryption_key");
            var decryptedKey = ServiceDP.DecryptData(Convert.FromBase64String(encryptedKey.Value), true,
                Convert.FromBase64String(entropy.Value));

            if (!string.IsNullOrEmpty(body))
            {
                var encryptedContent = new ServiceSymmetricEncryption().EncryptData(decryptedKey, body);
                request.AddParameter("text/xml", encryptedContent, ParameterType.RequestBody);
            }

            var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
            var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
            if (deviceCert == null) return default(TClass);

            var encryptedCert = new ServiceSymmetricEncryption().EncryptData(decryptedKey,
                Convert.ToBase64String(deviceCert.RawData));
            request.AddHeader("device_cert", Convert.ToBase64String(encryptedCert));

            return SubmitRequest<TClass>(request, decryptedKey);
        }

        private TClass SubmitRequest<TClass>(RestRequest request, byte[] encKey = null) where TClass : new()
        {
            if (request == null)
            {
                _log.Error("Could Not Execute API Request.  The Request was empty." + new TClass().GetType());
                return default(TClass);
            }
            _log.Debug(request.Resource);


            var response = _client.Execute<TClass>(request);

            if (response == null)
            {
                _log.Error("Could Not Complete API Request.  The Response was empty." + request.Resource);
                return default(TClass);
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                _log.Error("Could Not Complete API Request.  The Response Produced An Error." + request.Resource);
                _log.Error(response.Content);

                try
                {
                    if (encKey != null)
                    {
                        var encryptedresponse = JsonConvert.DeserializeObject<DtoStringResponse>(response.Content);
                        var content = new ServiceSymmetricEncryption().Decrypt(encKey,
                            Convert.FromBase64String(encryptedresponse.Value));
                        _log.Error(content);
                    }
                }
                catch
                {
                    //ignore
                }

                return default(TClass);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _log.Error("The Request Was Unauthorized " + request.Resource);
                return default(TClass);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _log.Error("Error Retrieving API Response: Not Found " + request.Resource);
                return default(TClass);
            }

            if (response.ErrorException != null && encKey == null)
            {
                _log.Error("Error Retrieving API Response: " + response.ErrorException);

                return default(TClass);
            }

            if (response.Data == null && encKey == null)
            {
                _log.Error("Response Data Was Null For Resource: " + request.Resource);
                return default(TClass);
            }

            if (encKey != null)
            {
                if (response.Headers.Any(t => t.Name.Equals("client_signature")))
                {
                    var firstOrDefault = response.Headers.FirstOrDefault(t => t.Name.Equals("client_signature"));
                    if (firstOrDefault == null)
                    {
                        _log.Error("The Response Signature Is Not Valid For This Device: " + request.Resource);
                        return default(TClass);
                    }

                    var deviceThumbprint = new ServiceSetting().GetSetting("device_thumbprint");
                    var deviceCert = ServiceCertificate.GetCertificateFromStore(deviceThumbprint.Value, StoreName.My);
                    if (deviceCert == null)
                    {
                        _log.Error("Could Not Find The Device Certificate: " + request.Resource);
                        return default(TClass);
                    }
                    var signature = firstOrDefault.Value.ToString();
                    var encryptedresponse = JsonConvert.DeserializeObject<DtoStringResponse>(response.Content);
                    if (
                        !ServiceCertificate.VerifySignature(deviceCert, Convert.FromBase64String(signature),
                            encryptedresponse.Value))
                    {
                        _log.Error("Response Signature Verification Failed: " + request.Resource);
                        return default(TClass);
                    }
                    var content = new ServiceSymmetricEncryption().Decrypt(encKey,
                        Convert.FromBase64String(encryptedresponse.Value));
                    return JsonConvert.DeserializeObject<TClass>(content);
                }

                _log.Error("Invalid Reponse, Signature Missing: " + request.Resource);
                return default(TClass);
            }

            return response.Data;
        }
    }
}