using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MediaPlayerWithRemoteControl
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRemoteControl" in both code and config file together.
    [ServiceContract]
    public interface IRemoteControl
    {
        [OperationContract]
        Album[] GetAlbums();

        [OperationContract]
        void PlayAlbum(string albumTitle);

        [OperationContract]
        void PlaySong(string albumTitle, string songTitle);

        [OperationContract]
        void Play();

        [OperationContract]
        void Pause();

        [OperationContract]
        PlayerState GetState();
    }
}
