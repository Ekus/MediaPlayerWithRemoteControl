using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPlayerWithRemoteControl;
using System.ServiceModel;

namespace MediaPlayerRemoteControlClient
{
    public partial class Form1 : Form
    {
        IRemoteControl remote;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
                        remote = ChannelFactory<IRemoteControl>.CreateChannel(
                new BasicHttpBinding(),
                new EndpointAddress(txtServer.Text));

            comboBox1.DataSource = remote.GetAlbums();
            comboBox1.DisplayMember = "Title";
            comboBox1.ValueMember = "Title";

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            remote = ChannelFactory<IRemoteControl>.CreateChannel(
    new BasicHttpBinding(),
    new EndpointAddress(txtServer.Text));
            remote.PlayAlbum(comboBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            remote = ChannelFactory<IRemoteControl>.CreateChannel(
    new BasicHttpBinding(),
    new EndpointAddress(txtServer.Text));
            remote.Pause();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            remote = ChannelFactory<IRemoteControl>.CreateChannel(
new BasicHttpBinding(),
new EndpointAddress(txtServer.Text));
            remote.Play();
        }
    }
}
