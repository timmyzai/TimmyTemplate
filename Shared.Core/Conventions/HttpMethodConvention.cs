using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AwesomeProject
{
    public class HttpMethodConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            HttpMethodAttribute attribute = DetermineHttpMethod(action.ActionMethod.Name);

            // Apply the HTTP method constraint to all selectors, adding a selector if none exist
            if (!action.Selectors.Any())
            {
                action.Selectors.Add(new SelectorModel());
            }
            foreach (var selector in action.Selectors)
            {
                if (selector.ActionConstraints.OfType<HttpMethodActionConstraint>().Any())
                {
                    continue; // Skip if an HTTP method constraint is already present
                }
                selector.ActionConstraints.Add(new HttpMethodActionConstraint(attribute.HttpMethods));
            }
        }
        private HttpMethodAttribute DetermineHttpMethod(string methodName)
        {
            if (methodName.StartsWith("Get") || methodName.StartsWith("Send"))
                return new HttpGetAttribute();
            if (methodName.StartsWith("Update") || methodName.StartsWith("Change") ||
                methodName.StartsWith("Enable") || methodName.StartsWith("Disable") ||
                methodName.StartsWith("Verify"))
                return new HttpPutAttribute();
            if (methodName.StartsWith("Delete") || methodName.StartsWith("Remove"))
                return new HttpDeleteAttribute();
            return new HttpPostAttribute();
        }
    }
}
