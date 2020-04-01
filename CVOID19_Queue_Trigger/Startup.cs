using Common.Services;
using CVOID19_Queue_Trigger;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: FunctionsStartup(typeof(Startup))]
namespace CVOID19_Queue_Trigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddSingleton<ICOVIDService>(x =>
            {
                return new COVIDService(new System.Net.Http.HttpClient());
            });
        }
    }
}
