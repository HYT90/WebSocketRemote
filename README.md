# WebRTC 遠程控制電腦
嘗試通過簡單的方法來實現能夠遠端控制電腦的程式，易於使用的遠程控制解決方案。
### 功能

- **通過WebSocket實現穩定的點對點連線，確保即時控制和數據傳輸的可靠性。**
- **Kestrel實現輕量且高性能的網頁伺服器，處理HTTP請求和靜態文件。**

## 步驟
### 使用的套件

本專案使用了以下套件：
- [sipsorcery](https://github.com/sipsorcery-org/sipsorcery)
- [SIPSorceryMedia.FFmpeg](https://github.com/sipsorcery-org/SIPSorceryMedia.FFmpeg)

CLI中輸入以下指令
1. clone
```
git clone https://github.com/HYT90/WebRTCRemote.git
```
2. 切換路徑至WebRTC目錄
```
cd WebRTC
```
3. 還原依賴
```
dotnet restore
```
4. 建置
```
dotnet build
```
5. 執行
```
dotnet run
```
