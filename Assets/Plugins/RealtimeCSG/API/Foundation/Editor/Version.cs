namespace RealtimeCSG.Foundation
{
    internal static class Versioning
    {
	#if TEST_ENABLED
	    public const string PluginVersion = "TEST";
        public const string PrevPluginVersion = "1.536";
	#else
        public const string PluginVersion = "1.536";
	#endif
    }
}