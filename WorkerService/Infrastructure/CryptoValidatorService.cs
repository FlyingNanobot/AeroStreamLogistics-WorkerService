using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WorkerService.Infrastructure
{
    /// <summary>
    /// Helper service providing cryptographic validation utilities used by the worker.
    /// </summary>
    /// <remarks>
    /// Contains simple helpers for JWT validation, HMAC, certificate and PEM-based signature
    /// verification. This class wraps BCL crypto primitives and writes to Console on errors.
    /// Consider replacing Console logging with ILogger for production use.
    /// </remarks>
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
        /// <summary>
        /// Validate a JWT using issuer, audience, and signing key.
        /// </summary>
        /// <returns>true when the token is valid and passes signature and lifetime checks.</returns>
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
        /// <summary>
        /// Validate an HMAC-SHA256 signature for the provided payload.
        /// </summary>
        /// <returns>true when the computed HMAC matches the provided signature.</returns>
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
        /// <summary>
        /// Validate a signature using the RSA public key from an X.509 certificate file.
        /// </summary>
        /// <returns>true when signature verification succeeds.</returns>
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
        /// <summary>
        /// Validate a signature using an RSA public key loaded from a PEM file path
        /// configured in the application settings (Crypto:PublicKeyPath).
        /// </summary>
        /// <returns>true when signature verification succeeds.</returns>
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
