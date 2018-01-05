using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TImerXml.Startup))]
namespace TImerXml
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
