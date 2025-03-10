using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.TestAPI.Helper.CustomDataAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public class RequiredValidGuidAttribute : ValidationAttribute
{
    public RequiredValidGuidAttribute() : base(() => "Valid Guid field is required. Can not be guid.empty or null.") {}
    
    public override bool IsValid(object value)
    {
        return value is not null && !value.Equals(Guid.Empty);
    }
}

