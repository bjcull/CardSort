using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CardSort.Startup))]
namespace CardSort
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
