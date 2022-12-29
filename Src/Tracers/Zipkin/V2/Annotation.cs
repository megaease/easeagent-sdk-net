namespace easeagent.Tracers.Zipkin.V2
{
    public class Annotation
    {
        /// <summary>
        /// Microseconds from epoch.
        /// 
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
