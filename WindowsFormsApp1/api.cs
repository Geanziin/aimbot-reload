using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1;

public class api
{
    private string name;
    private string ownerid;
    private string secret;
    private string version;
    private HttpClient httpClient;
    private bool initialized = false;

    public static api KeyAuthApp = new api(
        name: "x7 aimlock", // App name
        ownerid: "IBz1XyIXTp", // Account ID
        secret: "bc10f3702f8d3295c9542895ea2ae21c053cc43cb219e2f46d212c3e258d8e7e", // Secret (opcional para autenticação por IP)
        version: "1.0" // Application version
    );

    public api(string name, string ownerid, string secret, string version)
    {
        this.name = name;
        this.ownerid = ownerid;
        this.secret = secret;
        this.version = version;
        this.httpClient = new HttpClient();
    }

    public async Task<bool> Login(string userid)
    {
        try
        {
            if (string.IsNullOrEmpty(userid))
            {
                return false;
            }

            // Obter IP público do usuário
            string userIP = await GetPublicIP();
            if (string.IsNullOrEmpty(userIP))
            {
                return false;
            }

            // Preparar dados para autenticação por IP
            var loginData = new
            {
                type = "login",
                username = userid,
                password = "", // Sem senha para autenticação por IP
                ownerid = this.ownerid,
                ip = userIP
            };

            string jsonData = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Fazer requisição para o servidor KeyAuth
            var response = await httpClient.PostAsync("https://keyauth.win/api/1.2/", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                if (result != null && result.ContainsKey("success") && result["success"] is bool success && success)
                {
                    this.initialized = true;
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na autenticação: {ex.Message}");
            return false;
        }
    }

    private async Task<string?> GetPublicIP()
    {
        try
        {
            var response = await httpClient.GetStringAsync("https://api.ipify.org");
            return response?.Trim();
        }
        catch
        {
            try
            {
                var response = await httpClient.GetStringAsync("https://ipinfo.io/ip");
                return response?.Trim();
            }
            catch
            {
                return null;
            }
        }
    }

    public bool IsInitialized()
    {
        return this.initialized;
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
