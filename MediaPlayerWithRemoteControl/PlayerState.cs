using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPlayerWithRemoteControl
{
    public class PlayerState
    {
        public string AlbumTitle { get; set; }
        public string SongTitle { get; set; }
        public TimeSpan Position { get; set; }
        public bool IsPlaying { get; set; }
    }
}
