using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grpc.Core;
using Helloworld;

namespace WpfNetCore31
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChannelHolder holder = new ChannelHolder("127.0.0.1:30051");
        int accu = 0;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            holder.Dispose();
        }

        void Test(int nLoopCount = 1)
        {
            try
            {
                string lastMessage = "";
                holder.Init();

                DateTime t0 = DateTime.UtcNow;
                for (int i = 0; i < nLoopCount; i++)
                {
                    var reply = holder.rpc.SayHello(new HelloRequest() { Name = String.Format("Test-{0:d3}", ++accu) });
                    lastMessage = reply.Message;
                }
                DateTime tEnd = DateTime.UtcNow;

                var ms = (tEnd - t0).TotalMilliseconds;
                var avg = (ms / nLoopCount);
                string msgTime = String.Format("Time cost: {0:f0} ms (Loop x{1}) \r\n - avg {2:f3} ms", ms, nLoopCount, avg);
                MessageBox.Show(lastMessage + "\r\n\r\n" + msgTime);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int nLoop = 0;
            if (Int32.TryParse(txtLoopCount.Text, out nLoop))
            {
                Test(nLoop);
            }
        }

        void ShowException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            sb.AppendLine();
            sb.AppendLine(ex.StackTrace);
            MessageBox.Show(sb.ToString(), ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public class ChannelHolder : IDisposable
        {
            Channel channel;
            string targetUri;
            public Greeter.GreeterClient rpc;

            public ChannelHolder(string uri)
            {
                targetUri = uri;
            }

            public void Init()
            {
                if (null == channel)
                {
                    var chn = new Channel(targetUri, ChannelCredentials.Insecure);
                    var client = new Greeter.GreeterClient(chn);
                    this.rpc = client;
                    this.channel = chn;
                }
            }

            public void Dispose()
            {
                channel?.ShutdownAsync();
            }

        }

    }
}
