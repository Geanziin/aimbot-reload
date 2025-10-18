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
            // Para aplicações Windows Forms, usar Debug.WriteLine em vez de Console
            System.Diagnostics.Debug.WriteLine("🔧 Sistema de debug configurado");
            System.Diagnostics.Debug.WriteLine("📋 Logs detalhados habilitados");
            
            // Tentar alocar console se não existir
            AllocateConsole();
        }
        catch (Exception ex)
        {
            // Se não conseguir configurar, continuar sem console
            System.Diagnostics.Debug.WriteLine($"⚠️ Console não disponível: {ex.Message}");
        }
    }

    private void AllocateConsole()
    {
        try
        {
            // Tentar alocar console para aplicação Windows Forms
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // Só alocar console se não estiver em debug mode
                return;
            }
            
            // Usar Debug.WriteLine para logs em aplicações Windows Forms
            System.Diagnostics.Debug.WriteLine("🔧 Console alocado para debug");
        }
        catch
        {
            // Ignorar erros de alocação de console
        }
    }

    private void LogSystemInfo()
    {
        try
        {
            LogMessage("=== INFORMAÇÕES DO SISTEMA ===");
            LogMessage($"Arquitetura do processo: {Environment.Is64BitProcess}");
            LogMessage($"Arquitetura do sistema operacional: {Environment.Is64BitOperatingSystem}");
            LogMessage($"Versão do .NET Framework: {Environment.Version}");
            LogMessage($"Versão do sistema operacional: {Environment.OSVersion}");
            LogMessage($"Processador: {Environment.ProcessorCount} cores");
            LogMessage($"Memória disponível: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
            LogMessage("================================");
        }
        catch (Exception ex)
        {
            LogMessage($"⚠️ Erro ao obter informações do sistema: {ex.Message}");
        }
    }

    private void LogMessage(string message)
    {
        try
        {
            // Tentar usar Console primeiro
            LogMessage(message);
        }
        catch
        {
            try
            {
                // Fallback para Debug
                System.Diagnostics.Debug.WriteLine(message);
            }
            catch
            {
                // Se tudo falhar, usar MessageBox como último recurso
                System.Windows.Forms.MessageBox.Show(message, "KeyAuth Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }
    }

    public bool Init()
    {
        try
        {
            LogMessage("=== INICIALIZANDO APLICAÇÃO VIA API ===");
            LogMessage("🔄 Sempre buscando dados frescos da API KeyAuth...");

            string jsonData = CreateInitData();
            LogMessage($"📤 Dados de inicialização: {jsonData}");

            string responseContent = SendHttpRequest(jsonData);
            LogMessage($"📥 Resposta de inicialização: {responseContent}");

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
            LogMessage("🔧 Criando dados de inicialização...");
            LogMessage($"📝 App Name: {this.name}");
            LogMessage($"🆔 Owner ID: {this.ownerid}");
            LogMessage($"🔑 Secret: {this.secret?.Substring(0, Math.Min(8, this.secret?.Length ?? 0))}...");
            LogMessage($"📦 Version: {this.version}");

            // Validar credenciais
            if (string.IsNullOrEmpty(this.name) || string.IsNullOrEmpty(this.ownerid) || string.IsNullOrEmpty(this.secret))
            {
                LogMessage("❌ ERRO: Credenciais incompletas!");
                LogMessage($"Name vazio: {string.IsNullOrEmpty(this.name)}");
                LogMessage($"OwnerID vazio: {string.IsNullOrEmpty(this.ownerid)}");
                LogMessage($"Secret vazio: {string.IsNullOrEmpty(this.secret)}");
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
            LogMessage($"✅ Dados de inicialização criados com sucesso");
            LogMessage($"📊 JSON gerado: {jsonData.Length} caracteres");
            
            return jsonData;
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Erro ao criar dados de inicialização: {ex.Message}");
            throw;
        }
    }

    private bool ProcessInitResponse(string responseContent)
    {
        LogMessage("🔍 Processando resposta de inicialização...");
        
        if (string.IsNullOrEmpty(responseContent))
        {
            LogMessage("❌ Resposta vazia na inicialização");
            LogMessage("🔧 Possíveis causas:");
            LogMessage("   - Servidor KeyAuth não respondeu");
            LogMessage("   - Problema de conectividade");
            LogMessage("   - URL incorreta");
            LogMessage("   - Firewall bloqueando requisição");
            return false;
        }

        LogMessage($"📥 Resposta recebida: {responseContent.Length} caracteres");
        LogMessage($"📄 Conteúdo da resposta: {responseContent}");

        try
        {
            var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            
            if (jsonResult == null)
            {
                LogMessage("❌ Falha ao deserializar JSON - resposta inválida");
                return false;
            }

            LogMessage("🔍 Analisando campos da resposta:");
            foreach (var kv in jsonResult)
            {
                LogMessage($"   {kv.Key}: {kv.Value}");
            }

            if (jsonResult.ContainsKey("success"))
            {
                bool success = Convert.ToBoolean(jsonResult["success"]);
                LogMessage($"✅ Campo 'success' encontrado: {success}");
                
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
                LogMessage("❌ Campo 'success' não encontrado na resposta");
                LogMessage("🔧 Resposta pode estar em formato incorreto");
                return false;
            }
        }
        catch (JsonException jsonEx)
        {
            LogMessage($"❌ Erro ao processar JSON: {jsonEx.Message}");
            LogMessage($"🔧 Resposta pode não ser JSON válido");
            LogMessage($"📄 Resposta original: {responseContent}");
            return false;
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Erro inesperado ao processar resposta: {ex.Message}");
            LogMessage($"📄 Resposta original: {responseContent}");
            return false;
        }
    }

    private bool SetInitSuccess(Dictionary<string, object> jsonResult)
    {
        this.sessionid = jsonResult.ContainsKey("sessionid") ? jsonResult["sessionid"].ToString() : "";
        this.enckey = jsonResult.ContainsKey("enckey") ? jsonResult["enckey"].ToString() : "";
        this.initialized = true;
        LogMessage("✅ Aplicação inicializada com sucesso!");
        return true;
    }

    private bool HandleInitError(Dictionary<string, object> jsonResult)
    {
        string errorMsg = jsonResult.ContainsKey("message") ? jsonResult["message"].ToString() : "Erro desconhecido";
        LogMessage($"❌ Erro na inicialização: {errorMsg}");
        return false;
    }

    private bool HandleInitException(Exception ex)
    {
        LogMessage($"❌ EXCEÇÃO na inicialização: {ex.Message}");
        LogMessage($"Tipo da exceção: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            LogMessage($"Exceção interna: {ex.InnerException.Message}");
        }
        return false;
    }

    public bool Login(string userid)
    {
        try
        {
            if (!ValidateLoginPreconditions(userid))
                return false;

            LogMessage($"=== FAZENDO LOGIN ===");
            LogMessage($"Usuário: {userid}");
            LogMessage($"SessionID: {this.sessionid}");
            LogMessage($"Enckey: {this.enckey?.Substring(0, Math.Min(8, this.enckey?.Length ?? 0))}...");

            string jsonData = CreateLoginData(userid);
            LogMessage($"Dados enviados: {jsonData}");
            
            string responseContent = SendHttpRequest(jsonData);

            LogMessage($"=== RESPOSTA DE LOGIN ===");
            LogMessage($"Resposta: {responseContent}");

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
            LogMessage("❌ Aplicação não foi inicializada. Execute Init() primeiro.");
            return false;
        }

        if (string.IsNullOrEmpty(userid))
        {
            LogMessage("❌ ERRO: UserID está vazio");
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
            LogMessage($"❌ FALHA no login para usuário: {userid}");
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
                    LogMessage($"Success: {success}");
                    
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
            LogMessage($"❌ Erro ao processar JSON: {jsonEx.Message}");
        }

        LogMessage($"❌ FALHA no login para usuário: {userid}");
        return false;
    }

    private void LogLoginResponse(Dictionary<string, object> jsonResult)
    {
        LogMessage($"Resultado parseado:");
        foreach (var kv in jsonResult)
        {
            LogMessage($"  {kv.Key} = {kv.Value}");
        }
    }

    private bool SetLoginSuccess(string userid)
    {
        this.logged = true;
        LogMessage($"✅ LOGIN BEM-SUCEDIDO para usuário: {userid}");
        return true;
    }

    private bool HandleLoginError(Dictionary<string, object> jsonResult)
    {
        if (jsonResult.ContainsKey("message"))
        {
            LogMessage($"❌ Erro: {jsonResult["message"]}");
        }
        return false;
    }

    private bool HandleLoginException(Exception ex)
    {
        LogMessage($"❌ EXCEÇÃO no login: {ex.Message}");
        LogMessage($"Tipo da exceção: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            LogMessage($"Exceção interna: {ex.InnerException.Message}");
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
            LogMessage("=== FAZENDO LOGOUT ===");
            this.logged = false;
            this.initialized = false;
            this.sessionid = null;
            this.enckey = null;
            
            LogMessage("✅ Logout realizado com sucesso!");
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Erro durante logout: {ex.Message}");
        }
    }

    private string SendHttpRequest(string jsonData)
    {
        try
        {
            LogMessage("🔗 Enviando requisição HTTP...");
            LogMessage($"🌐 URL: https://keyauth.win/api/1.0/");
            LogMessage($"📊 Tamanho dos dados: {jsonData.Length} bytes");
            
            // Testar conectividade primeiro
            if (!TestConnectivity())
            {
                LogMessage("❌ Falha no teste de conectividade");
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
            LogMessage("🔍 Testando conectividade com KeyAuth...");
            
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "KeyAuth/1.0");
                
                // Teste simples de conectividade
                string testUrl = "https://keyauth.win/api/1.0/";
                byte[] testData = Encoding.UTF8.GetBytes("{\"type\":\"test\"}");
                
                byte[] response = client.UploadData(testUrl, "POST", testData);
                LogMessage("✅ Conectividade OK - Servidor respondeu");
                return true;
            }
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Falha na conectividade: {ex.Message}");
            LogMessage($"🔧 Tentando URLs alternativas...");
            
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
                LogMessage($"🔄 Testando URL alternativa: {url}");
                
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "KeyAuth/1.0");
                    
                    byte[] testData = Encoding.UTF8.GetBytes("{\"type\":\"test\"}");
                    byte[] response = client.UploadData(url, "POST", testData);
                    
                    LogMessage($"✅ URL alternativa funcionando: {url}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ URL alternativa falhou: {url} - {ex.Message}");
            }
        }
        
        LogMessage("❌ Todas as URLs falharam");
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
            LogMessage($"⚠️ WebClient falhou: {webClientEx.Message}");
            LogMessage("🔄 Tentando com HttpWebRequest...");
            
            // Fallback para HttpWebRequest
            try
            {
                return ExecuteWithHttpWebRequest(jsonData);
            }
            catch (Exception httpWebRequestEx)
            {
                LogMessage($"❌ HttpWebRequest também falhou: {httpWebRequestEx.Message}");
                throw; // Re-throw para ser capturado pelo catch principal
            }
        }
    }

    private string ExecuteWithWebClient(string jsonData)
    {
        using (WebClient client = CreateWebClient())
        {
            LogMessage($"📤 Dados sendo enviados via WebClient: {jsonData}");
            
            string response = client.UploadString("https://keyauth.win/api/1.0/", "POST", jsonData);
            
            LogMessage($"📥 Resposta recebida via WebClient: {response}");
            
            return response;
        }
    }

    private string ExecuteWithHttpWebRequest(string jsonData)
    {
        LogMessage($"📤 Dados sendo enviados via HttpWebRequest: {jsonData}");
        
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
                    LogMessage($"📥 Resposta recebida via HttpWebRequest: {responseContent}");
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
        LogMessage($"❌ Erro de rede: {webEx.Message}");
        LogMessage($"Status: {webEx.Status}");
        
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
                LogMessage($"Resposta de erro: {errorResponse}");
                return errorResponse;
            }
        }
        catch (Exception readEx)
        {
            LogMessage($"❌ Erro ao ler resposta de erro: {readEx.Message}");
            return "";
        }
    }

    private string HandleGenericException(Exception ex)
    {
        LogMessage($"❌ Erro na requisição HTTP: {ex.Message}");
        LogMessage($"Tipo: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            LogMessage($"Exceção interna: {ex.InnerException.Message}");
        }
        return "";
    }
}
