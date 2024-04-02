namespace ByteAwesome.Services
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Clients = "Clients";
    }
    public static class UserNames
    {
        public const string AdminUserName = "Admin";
        public const string ClientsUserName = "Clients";
    }
    public class CurrencyConst
    {
        public const string BasedSymbol = "USDT";
        public const string BasedCurrency = "USD";
    }
    public class ConfigSettingsConst
    {
        public const string CustoWalletType = "CustoWalletType";
        public const string AdminWalletGroupId = "AdminWalletGroupId";
        public const string CreateWalletGroupFee = "CreateWalletGroupFee";
        public const string CreateWalletGroupFreeCount = "CreateWalletGroupFreeCount";
        public const string CreateMultiSignWalletGroupFee = "CreateMultiSignWalletGroupFee";
        public const string CreateWalletPolicyFee = "CreateWalletPolicyFee";
        public const string AddSobFee = "AddSobFee";
        public const string AddSobFreeCount = "AddSobFreeCount";
        public const string AddMspFee = "AddMspFee";
        public const string AddMspFreeCount = "AddMspFreeCount";
        public const string VaultFee = "VaultFee";
        public const string VaultMinFee = "VaultMinFee";
        public const string VaultMaxFee = "VaultMaxFee";
        public const string VaultFeeType = "VaultFeeType";
        public const string UpRankFromFreeToTeam = "UpRankFromFreeToTeam";
        public const string UpRankFromFreeToEnterprise = "UpRankFromFreeToEnterprise";
        public const string UpRankFromTeamToEnterprise = "UpRankFromTeamToEnterprise";
        public const string TransferDayLimit = "TransferDayLimit";
        public const string HugeAmountHoldingValue = "HugeAmountHoldingValue";
    }
    public static class NotifyEventNames
    {
        public const string TransferOwnerApprovalNeeded = "TransferOwnerApprovalNeeded";
        public const string TransferCompleteInformOwner = "TransferCompleteInformOwner";
        public const string TransferCompleteInformReceiver = "TransferCompleteInformReceiver";
        public const string VaultTransferCompleteInformOwner = "VaultTransferCompleteInformOwner";
        public const string CreateMspInformMspUsers = "CreateMspInformMspUsers";
        public const string MspTriggerInformOtherMsps = "MspTriggerInformOtherMsps";
        public const string MspApprovedInformOtherMsps = "MspApprovedInformOtherMsps";
        public const string MspRejectedInformOtherMsps = "MspRejectedInformOtherMsps";
        public const string SobTriggerInformMsp = "SobTriggerInformMsp";
        public const string SobTriggerInformAllSobs = "SobTriggerInformAllSobs";
        public const string SobApprovedInformMsp = "SobApprovedInformMsp";
        public const string SobRejectedInformMsp = "SobRejectedInformMsp";
        public const string TransferCompleteInformMspOwner = "TransferCompleteInformMspOwner";
        public const string DelegateAddedInformDelegateAgree = "DelegateAddedInformDelegateAgree";
        public const string DelegateApproveorRejectInformOwner = "DelegateApproveorRejectInformOwner";
        public const string DelegateActiveInformDelegate = "DelegateActiveInformDelegate";
        public const string DepositPendingInformReceiver = "DepositPendingInformReceiver";
        public const string DepositCompleteInformReceiver = "DepositCompleteInformReceiver";
        public const string ClaimRewardInformUser = "ClaimRewardInformUser";
        public const string WithdrawalSucceeded = "WithdrawalSucceeded";
        public const string WithdrawalFailed = "WithdrawalFailed";
    }
    public static class MessageContent
    {
        public const string VerifyPhoneNumber = "VerifyPhoneNumber";
        public const string VerifyEmail = "VerifyEmail";
        public const string LoginOtpEmail = "LoginOtpEmail";
        public const string PasswordResetEmail = "PasswordResetEmail";
        public const string OTPMessage = "OTPMessage";
    }
}
