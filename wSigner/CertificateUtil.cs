using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace wSigner
{
    /// <summary>
    /// Certificate utilities
    /// </summary>
    public static class CertificateUtil
    {
        #region Retrieval
        /// <summary>
        /// Gets the certificates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector">The selector.</param>
        /// <param name="useLocalMachine">if set to <c>true</c> [use local machine].</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>(Func<X509Certificate2, T> selector, bool useLocalMachine = false)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            X509Store store = null;
            try
            {
                store = OpenReadStore(useLocalMachine);
                return store.Certificates
                            .Cast<X509Certificate2>()
                            .Where(cert=>cert.HasPrivateKey)
                            .Select(selector)
                            .ToArray();
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
        }

        /// <summary>
        /// Gets a certificate with the specified serial number.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="useLocalMachine">if set to <c>true</c> [use local machine].</param>
        /// <param name="validOnly">if set to <c>true</c> [valid only].</param>
        /// <returns></returns>
        public static X509Certificate2 GetBySerial(string serial, bool useLocalMachine = false, bool validOnly = false)
        {
            if (String.IsNullOrEmpty(serial))
            {
                throw new ArgumentNullException("serial");
            }
            X509Store store = null;
            try
            {
                store = OpenReadStore(useLocalMachine);
                return store.Certificates
                            .Find(X509FindType.FindBySerialNumber, serial, validOnly)
                            .Cast<X509Certificate2>()
                            .FirstOrDefault();
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
        }

        /// <summary>
        /// Gets the certificate from file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static X509Certificate2 GetFromFile(string fileName, string password)
        {
            return new X509Certificate2(fileName, password);
        }

        /// <summary>
        /// Show a dialog and have the user select a certificate to sign.
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 GetByDialog(bool useLocalMachine = false, bool validOnly = true)
        {
            X509Store store = null;
            X509Certificate2 result;
            try
            {
                store = OpenReadStore(useLocalMachine);
                var filtered = store.Certificates
                    .Cast<X509Certificate2>()
                    .Where(cert => cert.HasPrivateKey);
                var cols = new X509Certificate2Collection(filtered.ToArray());
                var sel = X509Certificate2UI.SelectFromCollection(cols, "Certificates", "Select one to sign", X509SelectionFlag.SingleSelection);
                result = sel.Count == 0
                             ? null
                             : sel.Cast<X509Certificate2>()
                                   .FirstOrDefault();
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
            return result;
        } 
        #endregion

        #region Misc
        /// <summary>
        /// Convert from hexadecimal string to big integer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static BigInteger HexadecimalStringToBigInt(string input)
        {
            return BigInteger.Parse(NormalizeSerialString(input), System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Convert from big integer to hexadecimal string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string BigIntToHexadecimalString(BigInteger input)
        {
            return input.ToString("x");
        }

        /// <summary>
        /// Normalizes the serial string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string NormalizeSerialString(string input)
        {
            return input != null ? input.Replace(" ", "").ToUpperInvariant() : String.Empty;
        }
        #endregion

        #region Private
        private static X509Store OpenReadStore(bool useLocalMachine)
        {
            X509Store store;
            var storeLocation = useLocalMachine
                                    ? StoreLocation.LocalMachine
                                    : StoreLocation.CurrentUser;
            store = new X509Store(StoreName.My, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            return store;
        } 
        #endregion
    }
}