using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Configuration;
using forgeSample.Controllers;

namespace forgeSample
{
    public class Startup
    {
        private Credentials Credentials { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UseMemoryStorage());
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson();
            services.AddSignalR().AddNewtonsoftJsonProtocol(opt => {
                opt.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseCors(options =>
                options.WithOrigins(Credentials.GetAppSetting("FORGE_WEBHOOK_URL")).AllowAnyMethod()
            //options.WithOrigins(Controllers.OAuthController.GetAppSetting("FORGE_WEBHOOK_URL")).AllowAnyMethod()
            );
            app.UseEndpoints(routes =>
            {
                routes.MapHub<Controllers.DataManagementHub>("/api/signalr/datamanagement");
            });

            app.UseMvc();

            // Hangfire
            GlobalConfiguration.Configuration.UseMemoryStorage();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
