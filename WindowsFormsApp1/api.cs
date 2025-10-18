using System;
using System.Collections.Generic;
using System.Linq;
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

    public bool Login(string userid)
    {
        try
        {
            if (string.IsNullOrEmpty(userid))
            {
                return false;
            }

            Console.WriteLine($"Iniciando autenticação para usuário: {userid}");

            // Preparar dados para autenticação KeyAuth
            var loginData = new
            {
                type = "login",
                username = userid,
                password = "", // Sem senha para autenticação por IP
                ownerid = this.ownerid,
                secret = this.secret
            };

            string jsonData = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Fazer requisição síncrona para o servidor KeyAuth
            var response = httpClient.PostAsync("https://keyauth.win/api/1.2/", content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Resposta KeyAuth: {responseContent}");
            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                if (result != null)
                {
                    Console.WriteLine($"Resultado parseado: {string.Join(", ", result.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    
                    if (result.ContainsKey("success") && result["success"] is bool success && success)
                    {
                        Console.WriteLine($"Autenticação bem-sucedida para usuário: {userid}");
                        this.initialized = true;
                        return true;
                    }
                    else if (result.ContainsKey("message"))
                    {
                        Console.WriteLine($"Mensagem de erro: {result["message"]}");
                    }
                }
            }

            Console.WriteLine($"Falha na autenticação para usuário: {userid}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na autenticação: {ex.Message}");
            return false;
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
