namespace ByteAwesome
{
    public static class ErrorCodes
    {
        public static class General
        {
            // public const string HostTerminated = "1001";
            public const string UnhandledError = "1002";
            // public const string EntityGetById = "1003";
            // public const string EntityAdd = "1004";
            // public const string EntityUpdate = "1005";
            // public const string EntityDelete = "1006";
            public const string EntityGetAll = "1007";
            // public const string LoadLanguages = "1008";
            // public const string DecryptString = "1009";
            // public const string NotifyUser = "1010";
            public const string JsonDataParse = "1011";
            public const string PleaseLogin = "1012";
            public const string EntityNameNotFound = "1013";
            // public const string RedisConnection = "1014";
            public const string InvalidField = "1015";
            public const string Unauthorized = "1016";
            // public const string GetByUserId = "1017";
            public const string ComingSoon = "9999";
            public const string EntityAlreadyDeleted = "1018";
            public const string ConfigNotFound = "1019";
            // public const string EncryptString = "1009";
            public const string EntityDuplicate = "1020";
            public const string EntitiesNotFound = "1021";
        }
        public static class User
        {
            public const string RoleAlreadyExists = "U1001";
            public const string KycFinalRejected = "U1002";
            public const string KycApproved = "U1003";
            public const string KycLevelAlreadyApproved = "U1004";
            public const string UserAddFailed = "U1005";
            public const string UserNotExists = "U1006";
            public const string UserLoginFailed = "U1007";
            public const string TwoFactorAlreadyEnabled = "U1008";
            public const string EnableDisablePassKeyError = "U1009";
            // public const string UserGetAllFailed = "U1010";
            public const string SendConfirmationEmail = "U1011";
            public const string InvalidUserLoginIdentity = "U1012";
            public const string UserIsNotActive = "U1013";
            public const string UserLockedOut = "U1014";
            public const string InvalidPassword = "U1015";
            public const string RequireOtpToVerifyEmail = "U1016";
            public const string RequireTwoFactorPin = "U1017";
            public const string RequireLoginOtp = "U1018";
            public const string GetMyUserInfo = "U1019";
            public const string GetUserInfoByUserIdentity = "U1020";
            public const string UserNewPasswordNotMatch = "U1021";
            public const string ChangeUserPassword = "U1022";
            public const string UpdateUserProfile = "U1023";
            public const string UserLogoutFailed = "U1024";
            public const string UpdateUserPhoneNumber = "U1025";
            public const string GetTwoFactorAuthInfo = "U1026";
            public const string EnableOrDisableTwoFactorAuth = "U1027";
            public const string DeleteMyAccount = "U1028";
            public const string SendLoginOtpEmail = "U1029";
            public const string SendConfirmationPhone = "U1030";
            public const string VerifyPhoneNumber = "U1031";
            public const string SendForgotPasswordEmail = "U1032";
            public const string ChangeUserPasswordByEmail = "U1033";
            // public const string GenerateAccessToken = "U1034";
            // public const string DecryptString = "U1035";
            public const string GetMyUserLoginSessions = "U1036";
            public const string PassKeyNotEnabled = "U1037";
            public const string UserNameAlreadyExists = "U1038";
            public const string EmailAddressAlreadyExists = "U1039";
            public const string PhoneNumberAlreadyExists = "U1040";
            // public const string GetRoleByName = "U1041";
            public const string UserWithoutPhone = "U1042";
            public const string PendingVerifyCredential = "U1043";
            // public const string IdentityTypeAlreadyExists = "U1044";
            // public const string OTPError = "U1045";
            public const string PhoneAlreadyVerified = "U1046";
            // public const string GetUserRoleByUser = "U1047";
            // public const string UserGrpcError = "U1048";
            // public const string GetKycAccessTokenError = "U1049";
            // public const string RegisterPasskeyError = "U1050";
            // public const string AddPasskeyCredentialError = "U1051";
            // public const string PasskeyLoginError = "U1052";
            public const string PasskeyAssertionError = "U1053";
            public const string PassKeyAlreadyEnabled = "U1054";
            public const string PleaseCreatePassKey = "U1055";
            public const string PleaseEnableTwoFactorAuth = "U1056";
            public const string PasskeyAttestationError = "U1057";
            public const string EmailAlreadyVerified = "U1058";
            public const string PassKeyAlreadyDisabled = "U1059";
            public const string PassKeyNotRegisterOnPhone = "U1060";
        }
        public static class Secret
        {
            // public const string GenerateAccessToken = "S1001";
            // public const string CreateAccessToken = "S1002";
            // public const string CreateJwtClaims = "S1003";
            // public const string CreateOrUpdateOtp = "S1004";
            // public const string VerifyOTP = "S1005";
            // public const string OTPGetAll = "S1006";
            public const string InvalidOTP = "S1007";
            public const string ExpiredOTP = "S1008";
            public const string UsedOTP = "S1009";
            public const string CreateOrUpdateTFA = "S1010";
            // public const string CreateOrUpdateKmsKeys = "S1011";
            public const string InvalidTFPin = "S1012";
            public const string OTPNotFound = "S1013";
            // public const string GetValidOtpByValue = "S1014";
            // public const string VerifyDataAndDecrypt = "S1015";
            public const string OTPAlreadyRequested = "S1016";
            public const string TFANotFound = "S1017";
            public const string KmsKeysNotFound = "S1018";
        }
        public static class Wallet
        {
            public const string WalletGroupNotFound = "W1001";
            public const string WalletNotEnoughFunds = "W1002";
            public const string WalletUserNotFound = "W1003";
            public const string WalletUserNotExists = "W1004";
            public const string CurrencyConversionError = "W1005";
            public const string CurrencyCodeInvalid = "W1006";
            public const string CountryNotSupported = "W1007";
            public const string CountryOrCurrencyNotSupported = "W1008";
            public const string UserAlreadyHasWallet = "W1009";
            public const string RateRetrievalError = "W1010";
            public const string RateStorageError = "W1011";
            public const string CurrencyRetrievalError = "W1012";
        }
        public static class Notification
        {
            public const string GetMyNotifySettings = "N1001";
            public const string UpdateMyNotifySettings = "N1002";
            public const string SendEmail = "N1003";
            public const string SendSMS = "N1004";
            public const string RegisterUserSocket = "N1005";
            public const string DeleteUserSocket = "N1006";
            public const string GetMyNotifyInbox = "N1007";
            public const string AddSaveUserNotifySetting = "N1008";
            public const string UpdateMyMessage = "N1009";
            public const string NotisGrpcError = "N1010";
            public const string UserNotifySettingAlreadyExists = "N1011";
            public const string UserDeviceNotFound = "N1012";
            public const string UserMessageNotFound = "N1013";
        }
        public static class Campaign
        {
            public const string CampaignNotFound = "CP1001";
            public const string NotActive = "CP1002";
            public const string NotStarted = "CP1003";
            public const string Ended = "CP1004";
            public const string UserAlreadyClaimed = "CP1005";
            public const string GetCampaignClaimLink = "CP1006";
            public const string InvalidSession = "CP1007";
            public const string SessionExpired = "CP1008";
            public const string GetAllCampaigns = "CP1009";
            public const string MoneyPoolLessThanZero = "CP1010";
            public const string MaxAmountLessThanZero = "CP1011";
            public const string MinGreaterThanMax = "CP1012";
            public const string MinGreaterThanPool = "CP1013";
            public const string MoneyPoolLessThanMax = "CP1014";
            public const string DecimalPlacesLessthanZero = "CP1015";
            public const string MinAmountLessThanZero = "CP1016";
            public const string StartDateGreaterThanEndDate = "CP1017";
            public const string ReclaimCoolDownDaysLessThanZero = "CP1018";
            public const string ReclaimCoolDownDaysNotFound = "CP1019";
            public const string ReclaimCoolDownDaysMustNull = "CP1020";
            public const string RewardNotFound = "CP1021";
            public const string GetCampaignById = "CP1022";
            public const string RewardExceededDecimalPlaces = "CP1023";
        }
        public static class CryptoFinance
        {
            public const string FiatAndCryptoEmpty = "F1001";
            public const string FiatOrCryptoAmountInvalid = "F1002";
            public const string FiatAmountInvalid = "F1003";
            public const string PaymentActionInvalid = "F1004";
            public const string CryptoAmountInvalid = "F1005";
            public const string GetCountriesAndCurrenciesError = "F1006";
            public const string GetPaymentMethodsByFiatAndCryptoError = "F1007";
            public const string GetBestEstimatedPriceOrCryptoAmountError = "F1008";
            public const string GetOrdersError = "F1009";
            public const string CreateOrderError = "F1010";
            public const string SellTokenError = "F1011";
            public const string FiatCurrencyOrCountryInvalid = "F1012";
        }
    }
}
