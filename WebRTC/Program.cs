// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using WebRTCRemote;

// 設置專案檔案 (.csproj) 設置類型為 WinExe
// <OutputType>WinExe</OutputType>
// 使用 DllImport 來調用 WinAPI 函數
[DllImport("kernel32.dll", SetLastError = true)] 
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool AllocConsole();


//Console.WriteLine("Hello, World!");
//args 可帶入 env 變數
static void Main()
{
    AllocConsole();
    Server server = new Server([127, 0, 0, 1], 8080);
    Task.Run(server.RunAsync);
    Console.WriteLine(ScreenStream.RecordImageBytes().Length);
    Application.EnableVisualStyles(); 
    Application.SetCompatibleTextRenderingDefault(false); 
    Application.Run(new 預覽視窗());
}

Main();

