using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace ChatServer
{
    partial class Application
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Chat Server ..");

            //ChatServer chatServer = new ChatServer(8080);
            //chatServer.StartAsync().Wait();

            //CreateHostBuilder(args).Build().Run();
            CreateInsecureHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<AppServer>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 443, listenOptions =>
                    {
                        listenOptions.UseHttps("path/to/certificate.pfx", "certificatePassword");
                    });
                });

        public static IWebHostBuilder CreateInsecureHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<AppServer>()
                .UseUrls("http://127.0.0.1:8080");
    }
}
