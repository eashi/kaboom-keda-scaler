using Google.Protobuf.Collections;
using TwitchLib.Client;

namespace kaboom_scaler
{
    internal class ScaledObjectData
    {
        public TwitchClient TwitchClient { get; set; }
        public MapField<string, string> Metadata { get; set; }

        public int State { get; set; }

    }
}