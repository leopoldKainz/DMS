namespace DMS.Business.Models.Validation;

public static class ValidationMessages
{
    public static class User
    {
        public const string UsernameRequired = "Username is required.";
        public const string UsernameMaxLength = "Username must not exceed 20 characters.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email address is required.";
        public const string EmailMaxLength = "Email must not exceed 50 characters.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordMinLength = "Password must be at least 6 characters long.";
        public const string UsernameOrEmailRequired = "Username or email is required.";
        public const string UsernameOrEmailMaxLength = "Username or email cannot exceed 50 characters.";
        public const string UserIdRequired = "User ID is required.";
    }

    public static class Document
    {
        public const string DocumentIdRequired = "Document ID is required.";
        public const string NameRequired = "Document name is required.";
        public const string NameMaxLength = "Document name cannot exceed 255 characters.";
        public const string TypeRequired = "Document type is required.";
        public const string TypeMaxLength = "Document type cannot exceed 100 characters.";
        public const string SizePositive = "Document size must be greater than 0 bytes.";
        public const string SizeNonNegative = "Document size must be non-negative.";
    }

    public static class ShareRequest
    {
        public const string IdRequired = "Share request ID is required.";
        public const string OwnerIdRequired = "Owner ID is required.";
        public const string ReceiverIdRequired = "Receiver ID is required.";
        public const string ReceiverCannotBeOwner = "Receiver cannot be the same as the owner.";
        public const string OwnerRequired = "Owner is required.";
        public const string ReceiverRequired = "Receiver is required.";
        public const string DocumentRequired = "Document is required.";
        public const string PermissionRequired = "A valid permission must be specified.";
        public const string StatusRequired = "A valid share request status must be specified.";
    }

    public static class Common
    {
        public const string CreatedAtRequired = "Creation timestamp is required.";
    }
}
