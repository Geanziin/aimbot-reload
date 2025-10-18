using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1;

public class api
{
    public string name;
    public string ownerid;
    public string secret;
    public string version;
    public string sessionid;
    public string enckey;
    public bool initialized;
    public bool logged;
    public HttpClient httpClient;

    public static api KeyAuthApp = new api(
        name: "x7 aimlock",
        ownerid: "IBz1XyIXTp",
        secret: "bc10f3702f8d3295c9542895ea2ae21c053cc43cb219e2f46d212c3e258d8e7e",
        version: "1.0"
    );

    public api(string name, string ownerid, string secret, string version)
    {
        this.name = name;
        this.ownerid = ownerid;
        this.secret = secret;
        this.version = version;
        this.httpClient = new HttpClient();
        this.initialized = false;
        this.logged = false;
    }

    public bool Init()
    {
        try
        {
            Console.WriteLine("=== INICIALIZANDO APLICAÇÃO ===");
            
            var initData = new
            {
                type = "init",
                ver = this.version,
                hash = "",
                enckey = "",
                name = this.name,
                ownerid = this.ownerid
            };

            string jsonData = JsonConvert.SerializeObject(initData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync("https://keyauth.win/api/1.2/", content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Resposta de inicialização: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                if (result != null && result.ContainsKey("success"))
                {
                    bool success = Convert.ToBoolean(result["success"]);
                    if (success)
                    {
                        this.sessionid = result.ContainsKey("sessionid") ? result["sessionid"].ToString() : "";
                        this.enckey = result.ContainsKey("enckey") ? result["enckey"].ToString() : "";
                        this.initialized = true;
                        Console.WriteLine("✅ Aplicação inicializada com sucesso!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Erro na inicialização: {result.ContainsKey("message") ? result["message"] : "Erro desconhecido"}");
                    }
                }
            }

            Console.WriteLine("❌ Falha na inicialização da aplicação");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEÇÃO na inicialização: {ex.Message}");
            return false;
        }
    }

    public bool Login(string userid)
    {
        try
        {
            if (!this.initialized)
            {
                Console.WriteLine("❌ Aplicação não foi inicializada. Execute Init() primeiro.");
                return false;
            }

            if (string.IsNullOrEmpty(userid))
            {
                Console.WriteLine("❌ ERRO: UserID está vazio");
                return false;
            }

            Console.WriteLine($"=== FAZENDO LOGIN ===");
            Console.WriteLine($"Usuário: {userid}");
            Console.WriteLine($"SessionID: {this.sessionid}");
            Console.WriteLine($"Enckey: {this.enckey?.Substring(0, 8)}...");

            var loginData = new
            {
                type = "login",
                username = userid,
                pass = "",
                sessionid = this.sessionid,
                name = this.name,
                ownerid = this.ownerid
            };

            string jsonData = JsonConvert.SerializeObject(loginData);
            Console.WriteLine($"Dados enviados: {jsonData}");
            
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync("https://keyauth.win/api/1.2/", content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"=== RESPOSTA DE LOGIN ===");
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
                            this.logged = true;
                            Console.WriteLine($"✅ LOGIN BEM-SUCEDIDO para usuário: {userid}");
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

            Console.WriteLine($"❌ FALHA no login para usuário: {userid}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEÇÃO no login: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public bool IsInitialized()
    {
        return this.initialized;
    }

    public bool IsLogged()
    {
        return this.logged;
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
