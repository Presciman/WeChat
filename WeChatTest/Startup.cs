using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WeChatTest.Startup))]
namespace WeChatTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
