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
                Console.WriteLine("ERRO: UserID está vazio");
                return false;
            }

            Console.WriteLine($"=== INICIANDO AUTENTICAÇÃO ===");
            Console.WriteLine($"Usuário: {userid}");
            Console.WriteLine($"OwnerID: {this.ownerid}");
            Console.WriteLine($"Secret: {this.secret?.Substring(0, 8)}...");
            Console.WriteLine($"App Name: {this.name}");

            // Primeiro, inicializar a aplicação
            Console.WriteLine("Passo 1: Inicializando aplicação...");
            var initData = new
            {
                type = "init",
                ownerid = this.ownerid,
                name = this.name,
                version = this.version
            };

            string initJson = JsonConvert.SerializeObject(initData);
            var initContent = new StringContent(initJson, Encoding.UTF8, "application/json");
            var initResponse = httpClient.PostAsync("https://keyauth.win/api/1.2/", initContent).Result;
            string initResponseContent = initResponse.Content.ReadAsStringAsync().Result;
            
            Console.WriteLine($"Resposta de inicialização: {initResponseContent}");
            Console.WriteLine($"Status inicialização: {initResponse.StatusCode}");

            // Agora fazer o login
            Console.WriteLine("Passo 2: Fazendo login...");
            var loginData = new
            {
                type = "login",
                username = userid,
                password = "",
                ownerid = this.ownerid,
                secret = this.secret
            };

            string jsonData = JsonConvert.SerializeObject(loginData);
            Console.WriteLine($"Dados enviados: {jsonData}");
            
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Fazer requisição síncrona para o servidor KeyAuth
            var response = httpClient.PostAsync("https://keyauth.win/api/1.2/", content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"=== RESPOSTA COMPLETA ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Resposta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                if (result != null)
                {
                    Console.WriteLine($"Resultado parseado:");
                    foreach (var kv in result)
                    {
                        Console.WriteLine($"  {kv.Key} = {kv.Value}");
                    }
                    
                    if (result.ContainsKey("success"))
                    {
                        bool success = Convert.ToBoolean(result["success"]);
                        Console.WriteLine($"Success: {success}");
                        
                        if (success)
                        {
                            Console.WriteLine($"✅ AUTENTICAÇÃO BEM-SUCEDIDA para usuário: {userid}");
                            this.initialized = true;
                            return true;
                        }
                        else
                        {
                            if (result.ContainsKey("message"))
                            {
                                Console.WriteLine($"❌ Erro: {result["message"]}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ Erro HTTP: {response.StatusCode}");
            }

            Console.WriteLine($"❌ FALHA na autenticação para usuário: {userid}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEÇÃO na autenticação: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
