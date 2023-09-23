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

namespace easeagent.Tracers.Zipkin.V2
{
    public class Annotation
    {
        /// <summary>
        /// Microseconds from epoch.
        /// <p>This value should be set directly by instrumentation, using the most precise value possible.
        /// </summary>
        public readonly long Timestamp;

        /// <summary>
        /// Usually a short tag indicating an event, like <code>cache.miss</code> or <code>error</code>
        /// </summary>
        public readonly string Value;

        public Annotation(long timestamp, string value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
