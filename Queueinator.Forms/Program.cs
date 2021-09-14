using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Queueinator.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using App = System.Windows.Forms.Application;

namespace Queueinator.Forms
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            App.SetHighDpiMode(HighDpiMode.SystemAware);
            App.EnableVisualStyles();
            App.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();

            ConfigureServices(services);

            using (ServiceProvider sp = services.BuildServiceProvider())
            {
                var form1 = sp.GetRequiredService<MainScreen>();
                App.Run(form1);
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddMediatR(typeof(AppModule));
            services.AddScoped<MainScreen>();
            services.AddScoped<NewServerForm>();
        }
    }
}
