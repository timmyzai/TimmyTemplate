using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ByteAwesome
{
    public class NormalizeControllerNameConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var name = controller.ControllerName;
            if (name.StartsWith("Base"))
                return;
            int underscoreIndex = name.IndexOf("_V");
            if (underscoreIndex > -1)
            {
                name = name[..underscoreIndex];
                if(name.EndsWith("Controller"))
                {
                    name = name[..^10];
                }
                controller.ControllerName = name;
            }
        }
    }

}