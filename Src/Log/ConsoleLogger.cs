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

using System;

using zipkin4net;

namespace easeagent.Log
{
    public class ConsoleLogger : ILogger
    {
        public void LogError(string message)
        {
            Console.Error.WriteLine("Easeagent [ERROR] " + message);
        }

        public void LogInformation(string message)
        {
            Console.WriteLine("Easeagent [INFO] " + message);
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Easeagent [WARNING] " + message);
            Console.ResetColor();
        }
    }
}
