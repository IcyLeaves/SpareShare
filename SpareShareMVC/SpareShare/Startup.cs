using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SpareShare.Startup))]
namespace SpareShare
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
