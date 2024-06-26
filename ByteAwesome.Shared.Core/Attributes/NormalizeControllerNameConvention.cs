using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ByteAwesome
{
    public class NormalizeControllerNameConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var name = controller.ControllerName;
            int underscoreIndex = name.IndexOf("_V");
            if (underscoreIndex > -1)
            {
                name = name.Substring(0, underscoreIndex);
                if(name.EndsWith("Controller"))
                {
                    name = name[..^10];
                }
                controller.ControllerName = name;
            }
        }
    }

}