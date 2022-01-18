using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SatispayGBusiness.Models;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SatispayGBusiness
{
    public class Api
    {
        private const string baseDomain = "authservices.satispay.com";
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string KeyId { get; set; }
        public string Version { get; private set; } = "1.2.0";
        public bool IsSandbox { get; private set; }

        private HttpClient httpClient;

        private AsymmetricCipherKeyPair ackp;

        public Api(HttpClient httpClient, bool isSandBox = false)
        {
            IsSandbox = isSandBox;
            this.httpClient = httpClient;
            httpClient.BaseAddress = isSandBox ?
            new Uri($"https://staging.{baseDomain}/g_business/v1/") :
            new Uri($"https://{baseDomain}/g_business/v1/");
        }

        public string GenerateRsaKeys()
        {
            RsaKeyPairGenerator rkpg = new RsaKeyPairGenerator();
            rkpg.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
            ackp = rkpg.GenerateKeyPair();

            PublicKey = GetPem(ackp.Public);

            PrivateKey = GetPem(ackp.Private);

            return PrivateKey;
        }

        private string GetPem(AsymmetricKeyParameter akp)
        {
            StringBuilder keyPem = new StringBuilder();
            PemWriter pemWriter = new PemWriter(new StringWriter(keyPem));
            pemWriter.WriteObject(akp);
            pemWriter.Writer.Flush();

            return keyPem.ToString().Replace("\r", string.Empty);
        }

        public void SetAsymmetricKeyParameter(string pemPrivateKey)
        {
            var pemReader = new PemReader(new StringReader(pemPrivateKey));
            ackp = (AsymmetricCipherKeyPair)pemReader.ReadObject();
        }

        public async Task<string> RequestKeyId(RequestKeyIdRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.token))
                throw new ArgumentNullException("Missing activationToken argument");

            request.public_key = request.public_key ?? PublicKey;

            if (string.IsNullOrWhiteSpace(request.public_key))
                throw new ArgumentNullException("Missing PublicKey");

            HttpResponseMessage response = null;

            try
            {
                response = await httpClient.PostAsJsonAsync("authentication_keys", request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RequestKeyIdResponse>();

                KeyId = result.key_id;

                return KeyId;
            }
            catch (HttpRequestException ex)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new ActivationTokenNotFoundException();
                    case HttpStatusCode.Forbidden:
                        throw new ActivationTokenAlreadyPairedException();
                    case HttpStatusCode.BadRequest:
                        throw new InvalidRsaKeyException();
                }

                throw ex;
            }
        }

        private async Task<T> SendJsonAsync<T>(HttpMethod method, string requestUri, object content = null, string idempotencyKey = null)
        {
            var requestJson = string.Empty;

            if (content != null)
                requestJson = JsonSerializer.Serialize(content, new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                });

            var httpRequestMessage = new HttpRequestMessage(method, requestUri)
            {
                Content = content == null ? null : new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            using (SHA256 sha256 = SHA256.Create())
            {
                var now = DateTime.Now;
                var date = now.ToString("ddd, d MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture) + " " + now.ToString("zzz").Replace(":", string.Empty);

                httpRequestMessage.Headers.Add("Date", date);

                var signature = new StringBuilder();

                signature.Append($"(request-target): {method.Method.ToLower()} {httpClient.BaseAddress.LocalPath}{requestUri}\n");
                signature.Append($"host: {httpClient.BaseAddress.Host}\n");
                signature.Append($"date: {((string[])httpRequestMessage.Headers.GetValues("Date"))[0]}\n");

                var digest = $"SHA-256={Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(requestJson)))}";

                signature.Append($"digest: {digest}");

                var sign = SignData(signature.ToString(), ackp.Private);

                httpRequestMessage.Headers.Add("Digest", digest);
                httpRequestMessage.Headers.Add("Authorization",
                    $"Signature keyId=\"{KeyId}\", algorithm=\"rsa-sha256\", headers=\"(request-target) host date digest\", signature=\"{sign}\"");

                if (idempotencyKey != null)
                    httpRequestMessage.Headers.Add("Idempotency-Key", idempotencyKey);

                httpRequestMessage.Headers.Add("x-satispay-appn", "Satispay.NET");

                HttpResponseMessage response = null;

                string stringContent = string.Empty;

                try
                {
                    response = await httpClient.SendAsync(httpRequestMessage);

                    stringContent = await response.Content.ReadAsStringAsync();

                    response.EnsureSuccessStatusCode();

                    return JsonSerializer.Deserialize<T>(stringContent);
                }
                catch (HttpRequestException)
                {
                    throw new SatispayException(stringContent, response.StatusCode);
                }
                catch (JsonException)
                {
                    throw new SatispayException(stringContent, HttpStatusCode.OK);
                }
            }
        }

        private string SignData(string msg, AsymmetricKeyParameter privKey)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);

            ISigner signer = SignerUtilities.GetSigner("SHA256WithRSA");
            signer.Init(true, privKey);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            byte[] sigBytes = signer.GenerateSignature();

            return Convert.ToBase64String(sigBytes);
        }

        private bool VerifySignature(AsymmetricKeyParameter pubKey, string signature, string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            byte[] sigBytes = Convert.FromBase64String(signature);

            ISigner signer = SignerUtilities.GetSigner("SHA256WithRSA");
            signer.Init(false, pubKey);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(sigBytes);
        }

        public async Task<CreatePaymentResponse<T>> CreatePayment<T>(CreatePaymentRequest<T> request, string idempotencyKey = null)
        {
            if (request.amount_unit == 0)
                throw new SatispayException("amount_unit must be greater than 0", HttpStatusCode.BadRequest);

            var response = await SendJsonAsync<CreatePaymentResponse<T>>(HttpMethod.Post, "payments", request, idempotencyKey);

            //TODO
            response.QrCodeUrl = IsSandbox ? $"https://staging.online.satispay.com/qrcode/{response.code_identifier}" : $"https://online.satispay.com/qrcode/{response.code_identifier}";

            return response;
        }
        public async Task<PaymentDetailsResponse<T>> GetPaymentDetails<T>(string paymentId)
        {
            return await SendJsonAsync<PaymentDetailsResponse<T>>(HttpMethod.Get, $"payments/{paymentId}");
        }
        public async Task<PaymentDetailsResponse<T>> UpdatePaymentDetails<T>(string paymentId, UpdatePaymentRequest<T> request)
        {
            return await SendJsonAsync<PaymentDetailsResponse<T>>(HttpMethod.Put, $"payments/{paymentId}", request);
        }
    }
}
