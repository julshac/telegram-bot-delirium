using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace pj2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            /*
            if (env.EnvironmentName == "Local")
            {
                builder
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local\\secrets.json");
            }
            */
            builder.AddEnvironmentVariables("APPSETTINGS_");

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            string token = Configuration["TELEBOT_TOKEN"];
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "post",
                    template: "{contoller=api}/{action=p}/" + token
                );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=api}/{action=Index}/{id?}"
                );
            });

            Program.Token = token;
            Program.WebhookAddress = Configuration["SITE"];
        }
    }
}
