using System;
using System.Collections.Generic;
using System.IO;
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
    public string? sessionid;
    public string? enckey;
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
        
        // Configurar console para mostrar logs
        SetupConsoleLogging();
        
        // Verificar arquitetura e configurações do sistema
        LogSystemInfo();
    }

    private void SetupConsoleLogging()
    {
        try
        {
            // Garantir que o console está disponível
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("🔧 Console de debug configurado");
            Console.WriteLine("📋 Logs detalhados habilitados");
        }
        catch (Exception ex)
        {
            // Se não conseguir configurar console, tentar MessageBox como fallback
            System.Windows.Forms.MessageBox.Show($"Erro ao configurar console: {ex.Message}", "Debug Info", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
        }
    }

    private void LogSystemInfo()
    {
        try
        {
            Console.WriteLine("=== INFORMAÇÕES DO SISTEMA ===");
            Console.WriteLine($"Arquitetura do processo: {Environment.Is64BitProcess}");
            Console.WriteLine($"Arquitetura do sistema operacional: {Environment.Is64BitOperatingSystem}");
            Console.WriteLine($"Versão do .NET Framework: {Environment.Version}");
            Console.WriteLine($"Versão do sistema operacional: {Environment.OSVersion}");
            Console.WriteLine($"Processador: {Environment.ProcessorCount} cores");
            Console.WriteLine($"Memória disponível: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
            Console.WriteLine("================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao obter informações do sistema: {ex.Message}");
        }
    }

    public bool Init()
    {
        try
        {
            Console.WriteLine("=== INICIALIZANDO APLICAÇÃO VIA API ===");
            Console.WriteLine("🔄 Sempre buscando dados frescos da API KeyAuth...");

            string jsonData = CreateInitData();
            Console.WriteLine($"📤 Dados de inicialização: {jsonData}");

            string responseContent = SendHttpRequest(jsonData);
            Console.WriteLine($"📥 Resposta de inicialização: {responseContent}");

            return ProcessInitResponse(responseContent);
        }
        catch (Exception ex)
        {
            return HandleInitException(ex);
        }
    }

    private string CreateInitData()
    {
        try
        {
            Console.WriteLine("🔧 Criando dados de inicialização...");
            Console.WriteLine($"📝 App Name: {this.name}");
            Console.WriteLine($"🆔 Owner ID: {this.ownerid}");
            Console.WriteLine($"🔑 Secret: {this.secret?.Substring(0, Math.Min(8, this.secret?.Length ?? 0))}...");
            Console.WriteLine($"📦 Version: {this.version}");

            // Validar credenciais
            if (string.IsNullOrEmpty(this.name) || string.IsNullOrEmpty(this.ownerid) || string.IsNullOrEmpty(this.secret))
            {
                Console.WriteLine("❌ ERRO: Credenciais incompletas!");
                Console.WriteLine($"Name vazio: {string.IsNullOrEmpty(this.name)}");
                Console.WriteLine($"OwnerID vazio: {string.IsNullOrEmpty(this.ownerid)}");
                Console.WriteLine($"Secret vazio: {string.IsNullOrEmpty(this.secret)}");
                throw new ArgumentException("Credenciais KeyAuth incompletas");
            }

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
            Console.WriteLine($"✅ Dados de inicialização criados com sucesso");
            Console.WriteLine($"📊 JSON gerado: {jsonData.Length} caracteres");
            
            return jsonData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao criar dados de inicialização: {ex.Message}");
            throw;
        }
    }

    private bool ProcessInitResponse(string responseContent)
    {
        Console.WriteLine("🔍 Processando resposta de inicialização...");
        
        if (string.IsNullOrEmpty(responseContent))
        {
            Console.WriteLine("❌ Resposta vazia na inicialização");
            Console.WriteLine("🔧 Possíveis causas:");
            Console.WriteLine("   - Servidor KeyAuth não respondeu");
            Console.WriteLine("   - Problema de conectividade");
            Console.WriteLine("   - URL incorreta");
            Console.WriteLine("   - Firewall bloqueando requisição");
            return false;
        }

        Console.WriteLine($"📥 Resposta recebida: {responseContent.Length} caracteres");
        Console.WriteLine($"📄 Conteúdo da resposta: {responseContent}");

        try
        {
            var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            
            if (jsonResult == null)
            {
                Console.WriteLine("❌ Falha ao deserializar JSON - resposta inválida");
                return false;
            }

            Console.WriteLine("🔍 Analisando campos da resposta:");
            foreach (var kv in jsonResult)
            {
                Console.WriteLine($"   {kv.Key}: {kv.Value}");
            }

            if (jsonResult.ContainsKey("success"))
            {
                bool success = Convert.ToBoolean(jsonResult["success"]);
                Console.WriteLine($"✅ Campo 'success' encontrado: {success}");
                
                if (success)
                {
                    return SetInitSuccess(jsonResult);
                }
                else
                {
                    return HandleInitError(jsonResult);
                }
            }
            else
            {
                Console.WriteLine("❌ Campo 'success' não encontrado na resposta");
                Console.WriteLine("🔧 Resposta pode estar em formato incorreto");
                return false;
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"❌ Erro ao processar JSON: {jsonEx.Message}");
            Console.WriteLine($"🔧 Resposta pode não ser JSON válido");
            Console.WriteLine($"📄 Resposta original: {responseContent}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro inesperado ao processar resposta: {ex.Message}");
            Console.WriteLine($"📄 Resposta original: {responseContent}");
            return false;
        }
    }

    private bool SetInitSuccess(Dictionary<string, object> jsonResult)
    {
        this.sessionid = jsonResult.ContainsKey("sessionid") ? jsonResult["sessionid"].ToString() : "";
        this.enckey = jsonResult.ContainsKey("enckey") ? jsonResult["enckey"].ToString() : "";
        this.initialized = true;
        Console.WriteLine("✅ Aplicação inicializada com sucesso!");
        return true;
    }

    private bool HandleInitError(Dictionary<string, object> jsonResult)
    {
        string errorMsg = jsonResult.ContainsKey("message") ? jsonResult["message"].ToString() : "Erro desconhecido";
        Console.WriteLine($"❌ Erro na inicialização: {errorMsg}");
        return false;
    }

    private bool HandleInitException(Exception ex)
    {
        Console.WriteLine($"❌ EXCEÇÃO na inicialização: {ex.Message}");
        Console.WriteLine($"Tipo da exceção: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
        }
        return false;
    }

    public bool Login(string userid)
    {
        try
        {
            if (!ValidateLoginPreconditions(userid))
                return false;

            Console.WriteLine($"=== FAZENDO LOGIN ===");
            Console.WriteLine($"Usuário: {userid}");
            Console.WriteLine($"SessionID: {this.sessionid}");
            Console.WriteLine($"Enckey: {this.enckey?.Substring(0, Math.Min(8, this.enckey?.Length ?? 0))}...");

            string jsonData = CreateLoginData(userid);
            Console.WriteLine($"Dados enviados: {jsonData}");
            
            string responseContent = SendHttpRequest(jsonData);

            Console.WriteLine($"=== RESPOSTA DE LOGIN ===");
            Console.WriteLine($"Resposta: {responseContent}");

            return ProcessLoginResponse(responseContent, userid);
        }
        catch (Exception ex)
        {
            return HandleLoginException(ex);
        }
    }

    private bool ValidateLoginPreconditions(string userid)
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

        return true;
    }

    private string CreateLoginData(string userid)
    {
        var loginData = new
        {
            type = "login",
            username = userid,
            pass = "",
            sessionid = this.sessionid,
            name = this.name,
            ownerid = this.ownerid
        };
        return JsonConvert.SerializeObject(loginData);
    }

    private bool ProcessLoginResponse(string responseContent, string userid)
    {
        if (string.IsNullOrEmpty(responseContent))
        {
            Console.WriteLine($"❌ FALHA no login para usuário: {userid}");
            return false;
        }

        try
        {
            var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            if (jsonResult != null)
            {
                LogLoginResponse(jsonResult);
                
                if (jsonResult.ContainsKey("success"))
                {
                    bool success = Convert.ToBoolean(jsonResult["success"]);
                    Console.WriteLine($"Success: {success}");
                    
                    if (success)
                    {
                        return SetLoginSuccess(userid);
                    }
                    else
                    {
                        return HandleLoginError(jsonResult);
                    }
                }
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"❌ Erro ao processar JSON: {jsonEx.Message}");
        }

        Console.WriteLine($"❌ FALHA no login para usuário: {userid}");
        return false;
    }

    private void LogLoginResponse(Dictionary<string, object> jsonResult)
    {
        Console.WriteLine($"Resultado parseado:");
        foreach (var kv in jsonResult)
        {
            Console.WriteLine($"  {kv.Key} = {kv.Value}");
        }
    }

    private bool SetLoginSuccess(string userid)
    {
        this.logged = true;
        Console.WriteLine($"✅ LOGIN BEM-SUCEDIDO para usuário: {userid}");
        return true;
    }

    private bool HandleLoginError(Dictionary<string, object> jsonResult)
    {
        if (jsonResult.ContainsKey("message"))
        {
            Console.WriteLine($"❌ Erro: {jsonResult["message"]}");
        }
        return false;
    }

    private bool HandleLoginException(Exception ex)
    {
        Console.WriteLine($"❌ EXCEÇÃO no login: {ex.Message}");
        Console.WriteLine($"Tipo da exceção: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Exceção interna: {ex.InnerException.Message}");
        }
        return false;
    }

    public bool IsInitialized()
    {
        return this.initialized;
    }

    public bool IsLogged()
    {
        return this.logged;
    }

    public void Logout()
    {
        try
        {
            Console.WriteLine("=== FAZENDO LOGOUT ===");
            this.logged = false;
            this.initialized = false;
            this.sessionid = null;
            this.enckey = null;
            
            Console.WriteLine("✅ Logout realizado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro durante logout: {ex.Message}");
        }
    }

    private string SendHttpRequest(string jsonData)
    {
        try
        {
            Console.WriteLine("🔗 Enviando requisição HTTP...");
            Console.WriteLine($"🌐 URL: https://keyauth.win/api/1.0/");
            Console.WriteLine($"📊 Tamanho dos dados: {jsonData.Length} bytes");
            
            // Testar conectividade primeiro
            if (!TestConnectivity())
            {
                Console.WriteLine("❌ Falha no teste de conectividade");
                return "";
            }
            
            return ExecuteHttpRequest(jsonData);
        }
        catch (WebException webEx)
        {
            return HandleWebException(webEx);
        }
        catch (Exception ex)
        {
            return HandleGenericException(ex);
        }
    }

    private bool TestConnectivity()
    {
        try
        {
            Console.WriteLine("🔍 Testando conectividade com KeyAuth...");
            
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "KeyAuth/1.0");
                
                // Teste simples de conectividade
                string testUrl = "https://keyauth.win/api/1.0/";
                byte[] testData = Encoding.UTF8.GetBytes("{\"type\":\"test\"}");
                
                byte[] response = client.UploadData(testUrl, "POST", testData);
                Console.WriteLine("✅ Conectividade OK - Servidor respondeu");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Falha na conectividade: {ex.Message}");
            Console.WriteLine($"🔧 Tentando URLs alternativas...");
            
            // Tentar URLs alternativas
            return TestAlternativeUrls();
        }
    }

    private bool TestAlternativeUrls()
    {
        string[] alternativeUrls = {
            "https://keyauth.win/api/1.2/",
            "https://keyauth.win/api/1.1/",
            "https://keyauth.win/api/"
        };

        foreach (string url in alternativeUrls)
        {
            try
            {
                Console.WriteLine($"🔄 Testando URL alternativa: {url}");
                
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "KeyAuth/1.0");
                    
                    byte[] testData = Encoding.UTF8.GetBytes("{\"type\":\"test\"}");
                    byte[] response = client.UploadData(url, "POST", testData);
                    
                    Console.WriteLine($"✅ URL alternativa funcionando: {url}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ URL alternativa falhou: {url} - {ex.Message}");
            }
        }
        
        Console.WriteLine("❌ Todas as URLs falharam");
        return false;
    }

    private string ExecuteHttpRequest(string jsonData)
    {
        // Tentar primeiro com WebClient
        try
        {
            return ExecuteWithWebClient(jsonData);
        }
        catch (Exception webClientEx)
        {
            Console.WriteLine($"⚠️ WebClient falhou: {webClientEx.Message}");
            Console.WriteLine("🔄 Tentando com HttpWebRequest...");
            
            // Fallback para HttpWebRequest
            try
            {
                return ExecuteWithHttpWebRequest(jsonData);
            }
            catch (Exception httpWebRequestEx)
            {
                Console.WriteLine($"❌ HttpWebRequest também falhou: {httpWebRequestEx.Message}");
                throw; // Re-throw para ser capturado pelo catch principal
            }
        }
    }

    private string ExecuteWithWebClient(string jsonData)
    {
        using (WebClient client = CreateWebClient())
        {
            Console.WriteLine($"📤 Dados sendo enviados via WebClient: {jsonData}");
            
            string response = client.UploadString("https://keyauth.win/api/1.0/", "POST", jsonData);
            
            Console.WriteLine($"📥 Resposta recebida via WebClient: {response}");
            
            return response;
        }
    }

    private string ExecuteWithHttpWebRequest(string jsonData)
    {
        Console.WriteLine($"📤 Dados sendo enviados via HttpWebRequest: {jsonData}");
        
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://keyauth.win/api/1.0/");
        request.Method = "POST";
        request.ContentType = "application/json";
        request.UserAgent = "KeyAuth/1.0";
        request.Timeout = 30000;
        request.KeepAlive = false;
        request.ProtocolVersion = HttpVersion.Version11;
        
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        request.ContentLength = data.Length;
        
        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(data, 0, data.Length);
        }
        
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseContent = reader.ReadToEnd();
                    Console.WriteLine($"📥 Resposta recebida via HttpWebRequest: {responseContent}");
                    return responseContent;
                }
            }
        }
    }

    private WebClient CreateWebClient()
    {
        WebClient client = new WebClient();
        client.Headers.Add("Content-Type", "application/json");
        client.Headers.Add("User-Agent", "KeyAuth/1.0");
        client.Encoding = Encoding.UTF8;
        return client;
    }

    private string HandleWebException(WebException webEx)
    {
        Console.WriteLine($"❌ Erro de rede: {webEx.Message}");
        Console.WriteLine($"Status: {webEx.Status}");
        
        if (webEx.Response != null)
        {
            return ReadErrorResponse(webEx.Response);
        }
        return "";
    }

    private string ReadErrorResponse(WebResponse response)
    {
        try
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string errorResponse = reader.ReadToEnd();
                Console.WriteLine($"Resposta de erro: {errorResponse}");
                return errorResponse;
            }
        }
        catch (Exception readEx)
        {
            Console.WriteLine($"❌ Erro ao ler resposta de erro: {readEx.Message}");
            return "";
        }
    }

    private string HandleGenericException(Exception ex)
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
