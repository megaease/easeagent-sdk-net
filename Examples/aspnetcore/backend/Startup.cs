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
using System;
using common;
using Microsoft.Extensions.DependencyInjection;

namespace backend
{
    public class Startup : CommonStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }

        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(DateTime.Now.ToString());
            });
        }
    }
}
