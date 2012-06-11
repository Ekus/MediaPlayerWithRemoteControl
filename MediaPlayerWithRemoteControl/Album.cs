using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaPlayerWithRemoteControl
{
    public class Album
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public List<Song> Songs { get; private set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public override string ToString()
        {
            return Title;
        }

        public Album()
        {
            Songs = new List<Song>();
        }
    }

    public class Song
    {
        public string Title { get; set; }
        public Album Album { get; set; }
       // public TimeSpan Length { get; set; }
        public string FilePath { get; set; }
    }
}
