// Decompiled with JetBrains decompiler
// Type: LczxyAUTH
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
public class LczxyAUTH
{
  private readonly string appID;
  private readonly string appDataBase;
  private static readonly HttpClient client = new HttpClient();
  private static readonly string apiUrl = "https://4uth.squareweb.app/";

  public LczxyAUTH(string appID, string appDataBase)
  {
    this.appID = appID;
    this.appDataBase = appDataBase;
  }

  public async Task CheckAppStatus()
  {
    try
    {
      StringContent content = new StringContent($"{{\"appid\":\"{this.appID}\", \"appDatabase\":\"{this.appDataBase}\"}}", Encoding.UTF8, "application/json");
      HttpResponseMessage response = await LczxyAUTH.client.PostAsync(LczxyAUTH.apiUrl + "/check-app-status", (HttpContent) content);
      string message = await response.Content.ReadAsStringAsync();
      if (response.IsSuccessStatusCode)
      {
        if (message.Contains("AppID está ativo."))
        {
        }
      }
      else
        LczxyAUTH.error(message);
      response = (HttpResponseMessage) null;
    }
    catch (Exception)
    {
      LczxyAUTH.error("LczxyAuth Offline. Verifique Sua conexão");
    }
  }

  public static void error(string message)
  {
    string str = "Logs";
    string path = Path.Combine(str, "ErrorLogs.txt");
    if (!Directory.Exists(str))
      Directory.CreateDirectory(str);
    if (!File.Exists(path))
    {
      using (File.Create(path))
        File.AppendAllText(path, DateTime.Now.ToString() + " > This is the start of your error logs file");
    }
    File.AppendAllText(path, $"{DateTime.Now.ToString()} > {message}{Environment.NewLine}");
    Process.Start(new ProcessStartInfo("cmd.exe", $"/c start cmd /C \"color b && title Error && echo {message} && timeout /t 5\"")
    {
      CreateNoWindow = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false
    });
    Environment.Exit(0);
  }

  private static string GetHWID() => WindowsIdentity.GetCurrent().User.Value ?? "";

  private async Task<string> GetIPInfo()
  {
    try
    {
      using (HttpClient client = new HttpClient())
      {
        HttpResponseMessage async = await client.GetAsync("http://ip-api.com/json/");
        if (async.IsSuccessStatusCode)
          return await async.Content.ReadAsStringAsync();
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("Erro ao obter informações do IP: " + ex.Message);
    }
    return "Unknown";
  }

  private async Task SendLogToAPI(
    string usernameOrKey,
    string hwid,
    string ipInfo,
    bool isKeyLogin,
    string appid,
    string appDatabase)
  {
    try
    {
      StringContent content = new StringContent($"{{\"usernameOrKey\":\"{this.EscapeJsonString(usernameOrKey)}\", \"hwid\":\"{this.EscapeJsonString(hwid)}\", \"ipInfo\":\"{this.EscapeJsonString(ipInfo)}\", \"isKeyLogin\":{isKeyLogin.ToString().ToLower()}, \"computerUsername\":\"{this.EscapeJsonString(Environment.UserName)}\", \"appid\":\"{this.EscapeJsonString(appid)}\", \"appDatabase\":\"{this.EscapeJsonString(appDatabase)}\"}}", Encoding.UTF8, "application/json");

      HttpResponseMessage response = await LczxyAUTH.client.PostAsync(LczxyAUTH.apiUrl + "/log-login", (HttpContent) content);
      if (!response.IsSuccessStatusCode)
      {
        int num = (int) MessageBox.Show($"Erro ao enviar log para API. Status: {response.StatusCode}, Resposta: {await response.Content.ReadAsStringAsync()}");
      }
      response = (HttpResponseMessage) null;
    }
    catch (HttpRequestException ex)
    {
      LczxyAUTH.error($"Erro de requisição HTTP ao enviar log para API: {ex.Message}\nDetalhes: {ex.ToString()}");
    }
    catch (TaskCanceledException ex)
    {
      LczxyAUTH.error($"Timeout ao enviar log para API: {ex.Message}\nDetalhes: {ex.ToString()}");
    }
    catch (Exception ex)
    {
      LczxyAUTH.error($"Erro inesperado ao enviar log para API: {ex.Message}\nDetalhes: {ex.ToString()}");
    }
  }

  private string EscapeJsonString(string value)
  {
    return value.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
  }

  public async Task Check()
  {
    try
    {
      if ((await LczxyAUTH.client.GetAsync($"{LczxyAUTH.apiUrl}/health?appDatabase={this.appDataBase}")).IsSuccessStatusCode)
        return;
      LczxyAUTH.error("LczxyAuth offline. Verifique Sua Conexão");
    }
    catch (Exception)
    {
      LczxyAUTH.error("LczxyAuth offline. Verifique Sua Conexão");
    }
  }

  public async Task<bool> CheckApiAvailability()
  {
    try
    {
      return (await LczxyAUTH.client.GetAsync($"{LczxyAUTH.apiUrl}/health?appDatabase={this.appDataBase}")).IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<string> LoginWithKey(string key)
  {
    if (!await this.CheckApiAvailability())
      LczxyAUTH.error("Erro: LczxyAuth não está disponível no momento. Verifique Sua Conexão");
    string hwid = LczxyAUTH.GetHWID();
    StringContent content = new StringContent($"{{\"key\":\"{key}\", \"hwid\":\"{hwid}\", \"appid\":\"{this.appID}\", \"appDatabase\":\"{this.appDataBase}\"}}", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await LczxyAUTH.client.PostAsync(LczxyAUTH.apiUrl + "/login", (HttpContent) content);
    string str = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
      return JObject.Parse(str)["message"]?.ToString() ?? "Erro desconhecido ao fazer login com a key.";
    string ip = await this.GetIPInfo();
    _ = Task.Run((Func<Task>) (async () => await this.SendLogToAPI(key, hwid, ip, true, this.appID, this.appDataBase)));
    return "Login com a key bem-sucedido!";
  }

  public async Task<string> LoginWithUser(string username, string password)
  {
    if (!await this.CheckApiAvailability())
      LczxyAUTH.error("Erro: LczxyAuth não está disponível no momento. Verifique Sua Conexão");
    string hwid = LczxyAUTH.GetHWID();
    StringContent content = new StringContent($"{{\"username\":\"{username}\", \"password\":\"{password}\", \"hwid\":\"{hwid}\", \"appid\":\"{this.appID}\", \"appDatabase\":\"{this.appDataBase}\"}}", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await LczxyAUTH.client.PostAsync(LczxyAUTH.apiUrl + "/user-login", (HttpContent) content);
    string str = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
      return JObject.Parse(str)["message"]?.ToString() ?? "Erro desconhecido ao fazer login com usuário.";
    string ip = await this.GetIPInfo();
    _ = Task.Run((Func<Task>) (async () => await this.SendLogToAPI(username, hwid, ip, false, this.appID, this.appDataBase)));
    return "Login com usuário bem-sucedido!";
  }

  public async Task<string> RegisterUserWithKey(string username, string password, string key)
  {
    if (!await this.CheckApiAvailability())
      LczxyAUTH.error("Erro: LczxyAuth não está disponível no momento. Verifique Sua Conexão");
    StringContent content = new StringContent($"{{\"username\":\"{username}\", \"password\":\"{password}\", \"key\":\"{key}\", \"appid\":\"{this.appID}\", \"appDatabase\":\"{this.appDataBase}\"}}", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await LczxyAUTH.client.PostAsync(LczxyAUTH.apiUrl + "/register", (HttpContent) content);
    string str1 = await response.Content.ReadAsStringAsync();
    string str2 = !response.IsSuccessStatusCode ? JObject.Parse(str1)["message"]?.ToString() ?? "Erro desconhecido ao registrar usuário." : "Usuário registrado com sucesso!";
    response = (HttpResponseMessage) null;
    return str2;
  }
}
