using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CurrencyAPI
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    // ASP.NET Core 3.0+:
        //    // The UseServiceProviderFactory call attaches the
        //    // Autofac provider to the generic hosting mechanism.
        //    var host = Host.CreateDefaultBuilder(args)
        //        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        //        .ConfigureWebHostDefaults(webHostBuilder =>
        //        {
        //            webHostBuilder
        //                .UseContentRoot(Directory.GetCurrentDirectory())
        //                .UseIISIntegration()
        //                .UseStartup<Startup>();
        //        })
        //        .Build();

        //    host.Run();
        //}

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration(
                    ).UseStartup<Startup>();
                });
    }
}
