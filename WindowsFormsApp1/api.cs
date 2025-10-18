using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

#nullable disable
namespace WindowsFormsApp1;

public class api
{
  private string name;
  private string ownerid;
  private string secret;
  private string version;
  private static readonly HttpClient client = new HttpClient();
  private static readonly string apiUrl = "https://keyauth.win/api/1.2/";
  
  // Propriedades para controle de autenticação
  public bool IsAuthenticated { get; private set; } = false;
  public string UserIP { get; private set; } = "";
  public string SessionID { get; private set; } = "";

  public api(string name, string ownerid, string secret, string version)
  {
    this.name = name;
    this.ownerid = ownerid;
    this.secret = secret;
    this.version = version;
  }

  // Método para obter IP público do usuário
  public async Task<string> GetUserIP()
  {
    try
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        var response = await httpClient.GetStringAsync("https://api.ipify.org");
        UserIP = response.Trim();
        return UserIP;
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro ao obter IP: {ex.Message}", "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return "";
    }
  }

  // Método principal de autenticação KeyAuth por IP
  public async Task<bool> AuthenticateByIP()
  {
    try
    {
      // Obter IP do usuário
      string userIP = await GetUserIP();
      if (string.IsNullOrEmpty(userIP))
      {
        MessageBox.Show("Não foi possível obter seu IP. Verifique sua conexão com a internet.", "Erro de IP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Preparar dados para autenticação
      var authData = new
      {
        type = "login",
        username = userIP, // Usar IP como username
        password = "", // Sem senha necessária
        ownerid = this.ownerid,
        secret = this.secret,
        version = this.version,
        name = this.name
      };

      string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(authData);
      var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

      // Fazer requisição para KeyAuth
      var response = await client.PostAsync(apiUrl, content);
      string responseContent = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        var jsonResponse = JObject.Parse(responseContent);
        
        if (jsonResponse["success"]?.ToString() == "true")
        {
          IsAuthenticated = true;
          SessionID = jsonResponse["sessionid"]?.ToString() ?? "";
          
          MessageBox.Show($"Autenticação bem-sucedida!\nIP autorizado: {userIP}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
          return true;
        }
        else
        {
          string errorMessage = jsonResponse["message"]?.ToString() ?? "Erro desconhecido na autenticação";
          MessageBox.Show($"Falha na autenticação: {errorMessage}\nSeu IP ({userIP}) não está autorizado.", "Acesso Negado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return false;
        }
      }
      else
      {
        MessageBox.Show($"Erro na comunicação com KeyAuth: {response.StatusCode}", "Erro de Servidor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro durante autenticação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return false;
    }
  }

  // Método para verificar se a sessão ainda é válida
  public async Task<bool> ValidateSession()
  {
    if (!IsAuthenticated || string.IsNullOrEmpty(SessionID))
      return false;

    try
    {
      var validateData = new
      {
        type = "validate",
        sessionid = SessionID,
        ownerid = this.ownerid,
        secret = this.secret
      };

      string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(validateData);
      var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

      var response = await client.PostAsync(apiUrl, content);
      string responseContent = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        var jsonResponse = JObject.Parse(responseContent);
        return jsonResponse["success"]?.ToString() == "true";
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao validar sessão: {ex.Message}");
    }

    return false;
  }

  // Método para fazer logout
  public async Task Logout()
  {
    if (!IsAuthenticated || string.IsNullOrEmpty(SessionID))
      return;

    try
    {
      var logoutData = new
      {
        type = "logout",
        sessionid = SessionID,
        ownerid = this.ownerid,
        secret = this.secret
      };

      string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(logoutData);
      var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

      await client.PostAsync(apiUrl, content);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao fazer logout: {ex.Message}");
    }
    finally
    {
      IsAuthenticated = false;
      SessionID = "";
      UserIP = "";
    }
  }

  // Método para verificar status da aplicação
  public async Task<bool> CheckAppStatus()
  {
    try
    {
      var statusData = new
      {
        type = "app",
        ownerid = this.ownerid,
        secret = this.secret
      };

      string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(statusData);
      var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

      var response = await client.PostAsync(apiUrl, content);
      string responseContent = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        var jsonResponse = JObject.Parse(responseContent);
        return jsonResponse["success"]?.ToString() == "true";
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao verificar status da aplicação: {ex.Message}");
    }

    return false;
  }
}
