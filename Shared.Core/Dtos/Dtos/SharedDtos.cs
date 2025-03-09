using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace AwesomeProject
{
    public interface IBaseUserDto
    {
        Guid UserId { get; set; }
        string UserName { get; set; }
    }
    public class RequiredTwoFactorPin
    {
        [Required]
        public string TwoFactorPin { get; set; }
    }
    public class OptionalTwoFactorPin
    {
        public string TwoFactorPin { get; set; }
    }
    public class OptionalPasskeyAndTfa
    {
        [RequiredAtLeastOne(nameof(PendingVerifyCredential))]
        public string TwoFactorPin { get; set; }
        [RequiredAtLeastOne(nameof(TwoFactorPin))]
        public AuthenticatorAssertionRawResponse PendingVerifyCredential { get; set; }
    }
    public class SortParameter
    {
        public string PropertyName { get; set; }
        public EntitySortOrder Order { get; set; }

        private static readonly List<SortParameter> _createdTimeDesc =
            [new SortParameter(nameof(IAuditedEntity.CreatedTime), EntitySortOrder.Desc)];

        public SortParameter(string propertyName, EntitySortOrder order = EntitySortOrder.Asc)
        {
            PropertyName = propertyName;
            Order = order;
        }
        public static List<SortParameter> Create(string propertyName, EntitySortOrder order = EntitySortOrder.Asc)
        {
            return [new SortParameter(propertyName, order)];
        }
        public static List<SortParameter> Create(params (string PropertyName, EntitySortOrder Order)[] sortParams)
        {
            var sortParameters = new List<SortParameter>();
            foreach (var (propertyName, order) in sortParams)
            {
                sortParameters.Add(new SortParameter(propertyName, order));
            }
            return sortParameters;
        }
        public static List<SortParameter> CreatedTimeDesc()
        {
            return _createdTimeDesc;
        }
    }
    public class FilterParameter // For BO Usage only
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public FilterOperator Operator { get; set; }

        public FilterParameter(string propertyName, object value, FilterOperator filterOperator = FilterOperator.Equal)
        {
            PropertyName = propertyName;
            Value = value;
            Operator = filterOperator;
        }
        public static List<FilterParameter> Create(string propertyName, object value, FilterOperator filterOperator = FilterOperator.Equal)
        {
            return [new FilterParameter(propertyName, value, filterOperator)];
        }
        public static List<FilterParameter> Create(params (string PropertyName, object Value, FilterOperator Operator)[] filterParams)
        {
            var filterParameters = new List<FilterParameter>();
            foreach (var (propertyName, value, filterOperator) in filterParams)
            {
                filterParameters.Add(new FilterParameter(propertyName, value, filterOperator));
            }
            return filterParameters;
        }
    }
    public enum FilterOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        In,
        NotIn
    }
}