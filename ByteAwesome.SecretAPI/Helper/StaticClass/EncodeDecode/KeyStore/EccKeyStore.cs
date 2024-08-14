namespace ByteAwesome.SecretAPI.Helper
{
    public static class KeyStore
    {
        private static readonly Dictionary<string, string> keys = new Dictionary<string, string>();

        public static bool Exists(string publicKey)
        {
            return keys.ContainsKey(publicKey);
        }

        public static void Add(string publicKey, string privateKey)
        {
            if (!Exists(publicKey))
            {
                keys[publicKey] = privateKey;
            }
            else
            {
                throw new ArgumentException("A key with the same public key already exists.");
            }
        }
    }
}