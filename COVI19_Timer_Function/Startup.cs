using Common.Services;
using COVI19_Timer_Function;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;


[assembly: FunctionsStartup(typeof(Startup))]
namespace COVI19_Timer_Function
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
