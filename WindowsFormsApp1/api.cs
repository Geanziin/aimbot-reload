using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
            Console.WriteLine($"Dados de inicialização: {jsonData}");
            
            string responseContent = SendHttpRequest(jsonData);
            Console.WriteLine($"Resposta de inicialização: {responseContent}");

            if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    if (jsonResult != null && jsonResult.ContainsKey("success"))
                    {
                        bool success = Convert.ToBoolean(jsonResult["success"]);
                        if (success)
                        {
                            this.sessionid = jsonResult.ContainsKey("sessionid") ? jsonResult["sessionid"].ToString() : "";
                            this.enckey = jsonResult.ContainsKey("enckey") ? jsonResult["enckey"].ToString() : "";
                            this.initialized = true;
                            Console.WriteLine("✅ Aplicação inicializada com sucesso!");
                            return true;
                        }
                        else
                        {
                            string errorMsg = jsonResult.ContainsKey("message") ? jsonResult["message"].ToString() : "Erro desconhecido";
                            Console.WriteLine($"❌ Erro na inicialização: {errorMsg}");
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"❌ Erro ao processar JSON: {jsonEx.Message}");
                }
            }

            Console.WriteLine("❌ Falha na inicialização da aplicação");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEÇÃO na inicialização: {ex.Message}");
            Console.WriteLine($"Tipo da exceção: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
            }
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
            Console.WriteLine($"Enckey: {this.enckey?.Substring(0, Math.Min(8, this.enckey?.Length ?? 0))}...");

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
            
            string responseContent = SendHttpRequest(jsonData);

            Console.WriteLine($"=== RESPOSTA DE LOGIN ===");
            Console.WriteLine($"Resposta: {responseContent}");

            if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    if (jsonResult != null)
                    {
                        Console.WriteLine($"Resultado parseado:");
                        foreach (var kv in jsonResult)
                        {
                            Console.WriteLine($"  {kv.Key} = {kv.Value}");
                        }
                        
                        if (jsonResult.ContainsKey("success"))
                        {
                            bool success = Convert.ToBoolean(jsonResult["success"]);
                            Console.WriteLine($"Success: {success}");
                            
                            if (success)
                            {
                                this.logged = true;
                                Console.WriteLine($"✅ LOGIN BEM-SUCEDIDO para usuário: {userid}");
                                return true;
                            }
                            else
                            {
                                if (jsonResult.ContainsKey("message"))
                                {
                                    Console.WriteLine($"❌ Erro: {jsonResult["message"]}");
                                }
                            }
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"❌ Erro ao processar JSON: {jsonEx.Message}");
                }
            }

            Console.WriteLine($"❌ FALHA no login para usuário: {userid}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEÇÃO no login: {ex.Message}");
            Console.WriteLine($"Tipo da exceção: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
            }
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

    private string SendHttpRequest(string jsonData)
    {
        try
        {
            Console.WriteLine("🔗 Enviando requisição HTTP...");
            
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("User-Agent", "KeyAuth/1.0");
                client.Encoding = Encoding.UTF8;
                
                Console.WriteLine($"📤 Dados sendo enviados: {jsonData}");
                
                string response = client.UploadString("https://keyauth.win/api/1.0/", "POST", jsonData);
                
                Console.WriteLine($"📥 Resposta recebida: {response}");
                
                return response;
            }
        }
        catch (WebException webEx)
        {
            Console.WriteLine($"❌ Erro de rede: {webEx.Message}");
            Console.WriteLine($"Status: {webEx.Status}");
            
            if (webEx.Response != null)
            {
                try
                {
                    using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string errorResponse = reader.ReadToEnd();
                        Console.WriteLine($"Resposta de erro: {errorResponse}");
                        return errorResponse;
                    }
                }
                catch (Exception readEx)
                {
                    Console.WriteLine($"❌ Erro ao ler resposta de erro: {readEx.Message}");
                }
            }
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na requisição HTTP: {ex.Message}");
            Console.WriteLine($"Tipo: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
            }
            return "";
        }
    }
}
