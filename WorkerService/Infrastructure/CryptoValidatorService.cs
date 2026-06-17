using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WorkerService.Infrastructure
{
    public class CryptoValidatorService
    {
        private readonly IConfiguration _config;

        public CryptoValidatorService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Validate a JWT using issuer, audience, and signing key.
        /// </summary>
        public bool ValidateJwt(string jwt, string issuer, string audience, string signingKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
            };

            try
            {
                tokenHandler.ValidateToken(jwt, validationParams, out _);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CryptoValidator] JWT validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate an HMAC signature against a payload.
        /// </summary>
        public bool ValidateHmac(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computed = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

            if (signature == computed)
                return true;

            Console.WriteLine("[CryptoValidator] HMAC validation failed.");
            return false;
        }

        /// <summary>
        /// Validate a payload against an X.509 certificate signature.
        /// </summary>
        public bool ValidateWithCertificate(byte[] payload, byte[] signature, string certPath)
        {
            try
            {
                var cert = new X509Certificate2(certPath);
                using var rsa = cert.GetRSAPublicKey();

                if (rsa == null)
                {
                    Console.WriteLine("[CryptoValidator] No RSA public key found in certificate.");
                    return false;
                }

                return rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CryptoValidator] Certificate validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate Signature using RSA public key from a PEM file.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool ValidateSignature(byte[] payload, byte[] signature)
        {
            var publicKeyPath = _config["Crypto:PublicKeyPath"];
            var cert = File.ReadAllText(publicKeyPath);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(cert);

            return rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

    }
}
