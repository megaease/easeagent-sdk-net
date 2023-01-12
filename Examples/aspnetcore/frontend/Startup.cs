/**
 * Copyright 2023 MegaEase
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

﻿using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using zipkin4net.Transport.Http;
using common;
using Microsoft.Extensions.DependencyInjection;

namespace frontend
{
    public class Startup : CommonStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("Tracer").AddHttpMessageHandler(provider =>
                TracingHandler.WithoutInnerHandler(easeagent.Agent.GetServiceName()));
        }

        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                var callServiceUrl = config["callServiceUrl"];
                var clientFactory = app.ApplicationServices.GetService<IHttpClientFactory>();
                using (var httpClient = clientFactory.CreateClient("Tracer"))
                {
                    var response = await httpClient.GetAsync(callServiceUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        await context.Response.WriteAsync(response.ReasonPhrase);
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        await context.Response.WriteAsync(content);
                    }
                }
            });
        }
    }
}
