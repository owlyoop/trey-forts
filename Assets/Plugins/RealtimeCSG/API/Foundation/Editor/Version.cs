namespace RealtimeCSG.Foundation
{
    internal static class Versioning
    {
	#if TEST_ENABLED
	    public const string PluginVersion = "TEST";
        public const string PrevPluginVersion = "1.556";
	#else
        public const string PluginVersion = "1.556";
	#endif
    }
}