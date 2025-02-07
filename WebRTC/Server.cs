using System.Net.WebSockets;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using SIPSorcery.Net;
using SIPSorcery.Media;
using SIPSorceryMedia.Encoders;
using WebSocketSharp.Server;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using System.Security.Cryptography.X509Certificates;

namespace WebRTCRemote
{
    internal class Server
    {
        private static string? endpoint;
        private static HttpListener? httpListener;
        private static WebSocket? webSocket;
        private delegate void Handle(byte[] buffer, int size);
        private static Handle handle = RemoteHandle.DataContentReceived;

        private Server() { }

        public static void InitalizeServer(IPAddress ip, int port)
        {
            IPEndPoint ep = new(ip, port);
            endpoint = ep.ToString();
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"https://{endpoint}/");
        }

        public static async Task RunAsync()
        {
            httpListener.Start();
            Console.WriteLine($"WebSocket server started at wss://{endpoint}/");
            while (true)
            {
                Console.WriteLine("Listening...");
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    Console.WriteLine($"{context.Request.RemoteEndPoint} has connected.");
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    webSocket = wsContext.WebSocket;

                    // blocking
                    //var send = Send();
                    //var echo = Echo();
                    //await Task.WhenAll(echo, send);

                    // no blocking
                    Task.Run(Echo);
                    await Send();
                    //Task.Run(Echo);
                }
                else
                {
                    Console.WriteLine(context.Response.StatusCode);
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private static async Task Echo()
        {
            byte[] buffer = new byte[Constants.PacketSize];
            Console.WriteLine("Waiting for sending form client...");
            
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result == null) break;

                        if(result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                        }

                        if (result.Count>0)
                        {
                            // RemoteHandle.DataContentReceived
                            handle(buffer, result.Count);
                            //
                        }
                        //byte[] responseBuffer = Encoding.UTF8.GetBytes("Echo from server: " + message);
                        //await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Here is from Echo() part of JSON. {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Here is from Echo(). {ex.Message}");
            }
            
        }

        private static async Task Send()
        {
            DateTime nextLoop = DateTime.Now;
            Console.WriteLine("Sending to client...");
            try
            {
                while(webSocket.State == WebSocketState.Open && nextLoop < DateTime.Now)
                {
                    try
                    {
                        //var data = Encoding.UTF8.GetBytes(ScreenStream.RecordImageBase64String());
                        var blobData = ScreenStream.RecordImageBytes();
                        await webSocket.SendAsync(new ArraySegment<byte>(blobData), WebSocketMessageType.Binary, true, CancellationToken.None);


                        nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                        if(nextLoop > DateTime.Now)
                        {
                            await Task.Delay(nextLoop - DateTime.Now);
                        }
                    }
                    catch( Exception ex )
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
                    
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Here is from Send(). {ex.Message}");
            }
            
            
        }
    }

    public class WebRTCHost
    {
        private static string? endpoint;
        private static HttpListener? httpListener;
        private static WebSocket? webSocket;
        private static WebSocketServer webSocketServer;


        enum VIDEO_SOURCE
        {
            NONE,
            FILE_OR_STREAM,
            CAMERA,
            SCREEN
        }

        enum AUDIO_SOURCE
        {
            NONE,
            FILE_OR_STREAM,
            MICROPHONE,
        }


        public static void Run(IPAddress ip, int port, int webRTCPort)
        {
            IPEndPoint ep = new(ip, port);
            endpoint = ep.ToString();

            Task.Run(WebSocketRun);

            

            Console.WriteLine("WebRTC Demo");

            // Initialise FFmpeg librairies
            FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_PANIC, Constants.ffmpegPath);

            Console.WriteLine("WebRTC Get Started");
            try
            {
                // Start web socket.
                Console.WriteLine("Starting webRTC server...");
                webSocketServer = new WebSocketServer(ip, webRTCPort);

                //webSocketServer = new WebSocketServer(ip, webRTCPort, true);
                //webSocketServer.SslConfiguration.ServerCertificate = new X509Certificate2(Constants.CertPath, Constants.CertPassword);
                //webSocketServer.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

                webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/", (peer) => peer.CreatePeerConnection = CreatePeerConnection);


                webSocketServer.Start();
                Console.WriteLine($"Waiting for web socket connections on {webSocketServer.Address}:{webSocketServer.Port}...");
            }
            catch(Exception ex) 
            {
                if(httpListener != null || httpListener.IsListening)
                {
                    httpListener.Stop();
                    Console.WriteLine("--------------Http listener stop---------------");
                }
                throw;
            }
            


            // Ctrl-c will gracefully exit the call at any point.
            ManualResetEvent exitMe = new ManualResetEvent(false);
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                exitMe.Set();
            };

            // Wait for a signal saying the call failed, was cancelled with ctrl-c or completed.
            exitMe.WaitOne();
        }

        private static async Task WebSocketRun()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{endpoint}/");
            httpListener.Start();
            Console.WriteLine($"Web socket server started at ws://{endpoint}/");
            while (true)
            {
                Console.WriteLine("Listening...");
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    Console.WriteLine($"{context.Request.RemoteEndPoint} has connected.");
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    webSocket = wsContext.WebSocket;

                    await Echo();
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        private static async Task Echo()
        {
            byte[] buffer = new byte[Constants.PacketSize];
            Console.WriteLine("Waiting for sending form client...");
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result == null) break;

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                        }

                        if (result.Count > 0)
                        {
                            RemoteHandle.DataContentReceived(buffer, result.Count);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Here is JSON part. {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Here is from Echo(). {ex.Message}");
            }

        }
        static private VIDEO_SOURCE VideoSourceType = VIDEO_SOURCE.SCREEN;
        static private AUDIO_SOURCE AudioSourceType = AUDIO_SOURCE.MICROPHONE;
        static private String VideoSourceFile = @"C:\Users\USER\Videos\Captures\FPSDemo.mp4"; 
        static private String AudioSourceFile = @"C:\Users\USER\Videos\Captures\FPSDemo.mp4";
        static private bool RepeatVideoFile = true; // Used if VideoSource == VIDEO_SOURCE.FILE_OR_STREAM
        static private bool RepeatAudioFile = true; // Used if AudioSource == AUDIO_SOURCE.FILE_OR_STREAM

        //static private VIDEO_SOURCE VideoSourceType = VIDEO_SOURCE.SCREEN;
        static private VideoCodecsEnum VideoCodec = VideoCodecsEnum.VP8;
        static private AudioCodecsEnum AudioCodec = AudioCodecsEnum.PCMU;

        // Simple Traversal of UDP through NAT
        // 一種網絡協議，允許客戶端（如 VoIP 軟件）在 NAT（網絡地址轉換）網絡中找出自己的公共 IP 地址和可用的傳輸端口。
        // 這樣有助於穿越 NAT，從而使兩個位於不同網絡後面的設備可以直接通信。
        // 定義一個 STUN 伺服器地址，以便在需要時使用這個伺服器來幫助應用程式進行 NAT 穿越。
        private const string STUN_URL = "stun:stun.sipsorcery.com";
        static private RTCPeerConnection PeerConnection = null;
        // 接收和處理音頻數據的接口，用於處理音源訊號。
        static private IAudioSink audioSink = null;
        static private IVideoSource videoSource = null;
        static private IAudioSource audioSource = null;

        private static Task<RTCPeerConnection> CreatePeerConnection()
        {
            RTCConfiguration config = new RTCConfiguration
            {
                iceServers = new List<RTCIceServer> { new RTCIceServer { urls = STUN_URL } }
            };

            PeerConnection = new RTCPeerConnection(config);

            switch (VideoSourceType)
            {
                case VIDEO_SOURCE.FILE_OR_STREAM:
                    // Do we use same file for Audio ?
                    if ((AudioSourceType == AUDIO_SOURCE.FILE_OR_STREAM) && (AudioSourceFile == VideoSourceFile))
                    {
                        FFmpegFileSource fileSource = new FFmpegFileSource(VideoSourceFile, RepeatVideoFile, new AudioEncoder(), 1920, true);
                        fileSource.OnAudioSourceError += (msg) => PeerConnection.Close(msg);
                        fileSource.OnVideoSourceError += (msg) => PeerConnection.Close(msg);

                        videoSource = fileSource as IVideoSource;
                        audioSource = fileSource as IAudioSource;
                    }
                    else
                    {
                        FFmpegFileSource fileSource = new FFmpegFileSource(VideoSourceFile, RepeatVideoFile, new AudioEncoder(), 1920, true);
                        fileSource.OnVideoSourceError += (msg) => PeerConnection.Close(msg);

                        videoSource = fileSource as IVideoSource;
                    }
                    break;

                case VIDEO_SOURCE.SCREEN:
                    List<SIPSorceryMedia.FFmpeg.Monitor>? monitors = FFmpegMonitorManager.GetMonitorDevices();
                    SIPSorceryMedia.FFmpeg.Monitor? primaryMonitor = null;
                    if (monitors?.Count > 0)
                    {
                        foreach (SIPSorceryMedia.FFmpeg.Monitor monitor in monitors)
                        {
                            if (monitor.Primary)
                            {
                                primaryMonitor = monitor;
                                break;
                            }
                        }
                        if (primaryMonitor == null)
                            primaryMonitor = monitors[0];
                    }

                    if (primaryMonitor != null)
                    {
                        //videoSource = new FFmpegScreenSource(primaryMonitor.Path, primaryMonitor.Rect, 10);
                        videoSource = new FFmpegScreenSource(primaryMonitor.Path, new Rectangle(0,0,1920,1080), 10);
                        videoSource.OnVideoSourceError += (msg) => PeerConnection.Close(msg);
                    }
                    else
                        throw new NotSupportedException($"Cannot find adequate monitor ...");
                    break;

                default:
                    Console.WriteLine("No video source type");
                    break;
            }

            if (audioSource == null)
            {
                switch (AudioSourceType)
                {
                    case AUDIO_SOURCE.FILE_OR_STREAM:
                        FFmpegFileSource fileSource = new FFmpegFileSource(AudioSourceFile, RepeatAudioFile, new AudioEncoder(), 960, false);
                        fileSource.OnAudioSourceError += (msg) => PeerConnection.Close(msg);

                        audioSource = fileSource as IAudioSource;
                        break;
                }
            }

            if (videoSource != null)
            {
                videoSource.RestrictFormats(x => x.Codec == VideoCodec);

                var testPatternSource = new VideoTestPatternSource(new VpxVideoEncoder());

                var vsfList = testPatternSource.GetVideoSourceFormats();

                MediaStreamTrack screenTrack = new MediaStreamTrack(vsfList, MediaStreamStatusEnum.SendRecv);

                //MediaStreamTrack videoTrack = new MediaStreamTrack(testPatternSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendOnly);
                PeerConnection.addTrack(screenTrack);


                videoSource.OnVideoSourceEncodedSample += PeerConnection.SendVideo;
                PeerConnection.OnVideoFormatsNegotiated += (videoFormats) => videoSource.SetVideoSourceFormat(videoFormats.First());
            }

            if (audioSource != null)
            {
                audioSource.RestrictFormats(x => x.Codec == AudioCodec);

                MediaStreamTrack audioTrack = new MediaStreamTrack(audioSource.GetAudioSourceFormats(), MediaStreamStatusEnum.SendRecv);
                PeerConnection.addTrack(audioTrack);

                audioSource.OnAudioSourceEncodedSample += AudioSource_OnAudioSourceEncodedSample;
                PeerConnection.OnAudioFormatsNegotiated += (audioFormats) => audioSource.SetAudioSourceFormat(audioFormats.First());
            }

            //var testPatternSource = new VideoTestPatternSource(new VpxVideoEncoder());

            //MediaStreamTrack videoTrack = new MediaStreamTrack(testPatternSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendOnly);
            //PeerConnection.addTrack(videoTrack);

            //testPatternSource.OnVideoSourceEncodedSample += PeerConnection.SendVideo;
            //PeerConnection.OnVideoFormatsNegotiated += (formats) => testPatternSource.SetVideoSourceFormat(formats.First());

            //PeerConnection.onconnectionstatechange += async (state) =>
            //{
            //    Console.WriteLine($"Peer connection state change to {state}.");

            //    switch (state)
            //    {
            //        case RTCPeerConnectionState.connected:
            //            await testPatternSource.StartVideo();
            //            break;
            //        case RTCPeerConnectionState.failed:
            //            PeerConnection.Close("ice disconnection");
            //            break;
            //        case RTCPeerConnectionState.closed:
            //            await testPatternSource.CloseVideo();
            //            testPatternSource.Dispose();
            //            break;
            //    }
            //};

            PeerConnection.onconnectionstatechange += async (state) =>
            {
                Console.WriteLine($"Peer connection state change to {state}.");

                if (state == RTCPeerConnectionState.failed)
                {
                    PeerConnection.Close("ice disconnection");
                }
                else if (state == RTCPeerConnectionState.closed)
                {
                    if (videoSource != null)
                        await videoSource.CloseVideo();

                    if (audioSink != null)
                        await audioSink.CloseAudioSink();

                    if (audioSource != null)
                        await audioSource.CloseAudio();
                }
                else if (state == RTCPeerConnectionState.connected)
                {
                    if (videoSource != null)
                        await videoSource.StartVideo();

                    if (audioSink != null)
                    {
                        await audioSink.StartAudioSink();
                    }

                    if (audioSource != null)
                        await audioSource.StartAudio();
                }
            };

            return Task.FromResult(PeerConnection);
        }

        private static void AudioSource_OnAudioSourceEncodedSample(uint durationRtpUnits, byte[] sample)
        {
            PeerConnection.SendAudio(durationRtpUnits, sample);

            if (audioSink != null)
                audioSink.GotAudioRtp(null, 0, 0, 0, 0, false, sample);
        }
    }
}
