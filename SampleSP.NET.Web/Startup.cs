using System;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleSP.NET.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2Configuration>((config) => {
                config.AllowedAudienceUris.Add(config.Issuer);
                X509Certificate2 idpCertificate = LoadCertificate(Configuration["Saml2:IdPCertificate"]);
                config.SignatureValidationCertificates.Add(idpCertificate);
            });
            services.AddSaml2(slidingExpiration: true);
            services.AddControllersWithViews();
        }

        public X509Certificate2 LoadCertificate(string certificate)
        {
            if (string.IsNullOrWhiteSpace(certificate))
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            return new X509Certificate2(Convert.FromBase64String(certificate));
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
