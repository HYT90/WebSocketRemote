// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Runtime.InteropServices;
using WebRTCRemote;
using Microsoft.AspNetCore.Owin;
using System.Drawing.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

// 設置專案檔案 (.csproj) 設置類型為 WinExe
// <OutputType>WinExe</OutputType>
// 使用 DllImport 來調用 WinAPI 函數
// SetLastError 是一個在 P/Invoke 調用中使用的屬性，用於指定是否將原生函式呼叫的錯誤碼傳遞給 .NET 程式碼。
// 當設置 SetLastError = true 時，如果原生函式呼叫失敗，它會將錯誤碼儲存到 LastError 屬性中，
// 你可以通過呼叫 Marshal.GetLastWin32Error() 來獲取這個錯誤碼。
[DllImport("kernel32.dll", SetLastError = true)]
// MarshalAs屬性用於指定在 .NET 程式碼和非 .NET 程式碼之間傳遞的資料的機制。
// 用於指定從原生函式呼叫返回的資料類型。此處是bool。
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool AllocConsole();

//Console.WriteLine("Hello, World!");
//args 可帶入 env 變數

AllocConsole();
// 如果 AllocConsole() 函式呼叫失敗，錯誤碼將儲存到 LastError 中，並可以通過 Marshal.GetLastWin32Error() 來獲取。
int errorCode = Marshal.GetLastWin32Error();


var host = new WebHostBuilder()
    .UseKestrel()
    .UseUrls($"http://{Constants.IP}:80")
    .Configure((app) =>
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.Run(async (context) =>
        {
            var request = context.Request;
            var response = context.Response;
            string path = request.Path.Value;
            if (path == "/help")
            {
                response.StatusCode = 200;
                await response.WriteAsync("This is a WebRTCRemoteService.");
            }
            else
            {
                response.StatusCode = 400;
            }
        });
    })
    .Build();

host.RunAsync();




//Console.WriteLine("畫面容量大小： " + ScreenStream.RecordImageBytes().Length + " Bytes.");
while (true)
{
    try
    {
        Console.Write($"請輸入本機IP(或enter直接跳過，使用預設IP {Constants.IP} )：");
        string ip = Console.ReadLine();
        Constants.IP = ip.Equals(string.Empty)? Constants.IP:ip;
        Console.WriteLine($"Web socket server will run at http://{Constants.IP}:{Constants.WebRTCPort}/");
        //Server.InitalizeServer(address, port);
//        //Server.InitalizeServer(IPAddress.Parse(Constants.IP), Constants.Port);
        //await Server.RunAsync();
        //WebRTCHost.Run(address, port, Constants.WebRTCPort);
        WebRTCHost.Run(IPAddress.Parse(Constants.IP), Constants.WebRTCPort);
    }
    catch (Exception ex)
    {
        Console.WriteLine("-------------------------");
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("-------------------------");
    }
}



//Application.EnableVisualStyles(); 
//Application.SetCompatibleTextRenderingDefault(false); 
//Application.Run(new 預覽視窗(Constants.MS_PER_TICK));


