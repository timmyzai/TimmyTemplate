using System.ComponentModel;

namespace ByteAwesome.WalletAPI.ExternalApis.AwsKmsServices.Dto
{
    public enum EncodeMethod
    {
        [Description("Single Key")]
        Symmetric,
        [Description("Public and Private Key Pair")]
        Asymmetric
    }
}