using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(brownsberger.web.Startup))]
namespace brownsberger.web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
