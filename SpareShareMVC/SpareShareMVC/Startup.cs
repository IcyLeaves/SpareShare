using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SpareShareMVC.Startup))]
namespace SpareShareMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
