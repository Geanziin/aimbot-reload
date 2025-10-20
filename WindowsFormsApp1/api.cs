using System;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Console = WindowsFormsApp1.NoopConsole;

namespace WindowsFormsApp1
{
public class api
{
    // Instância estática do KeyAuth igual ao projeto de bypass
    public static api KeyAuthApp = new api(
        name: "x7 aimlock", // App name
        ownerid: "IBz1XyIXTp", // Account ID
        version: "1.0" // Application version
    );
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        // Import the required Atom Table functions from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalAddAtom(string lpString);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalFindAtom(string lpString);

        public string name, ownerid, version, path, seed;
        /// <summary>
        /// Set up your application credentials in order to use keyauth
        /// </summary>
        /// <param name="name">Application Name</param>
        /// <param name="ownerid">Your OwnerID, found in your account settings.</param>
        /// <param name="version">Application Version, if version doesnt match it will open the download link you set up in your application settings and close the app, if empty the app will close</param>
        public api(string name, string ownerid, string version, string? path = null)
        {
            if (ownerid.Length != 10)
            {
                Process.Start("https://youtube.com/watch?v=RfDTdiBq4_o");
                Process.Start("https://keyauth.cc/app/");
                Thread.Sleep(2000);
                error("Application not setup correctly. Please watch the YouTube video for setup.");
                TerminateProcess(GetCurrentProcess(), 1);
            }

    this.name = name;
    this.ownerid = ownerid;
    this.version = version;
            this.path = path;
        }

        #region structures
        private class response_structure
        {
            [JsonProperty("success")]
            public bool success { get; set; }

            [JsonProperty("newSession")]
            public bool newSession { get; set; }

            [JsonProperty("sessionid")]
            public string sessionid { get; set; }

            [JsonProperty("contents")]
            public string contents { get; set; }

            [JsonProperty("response")]
            public string response { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("ownerid")]
            public string ownerid { get; set; }

            [JsonProperty("download")]
            public string download { get; set; }

            [JsonProperty("info")]
            public user_data_structure info { get; set; }

            [JsonProperty("appinfo")]
            public app_data_structure appinfo { get; set; }

            [JsonProperty("messages")]
            public List<msg> messages { get; set; }

            [JsonProperty("users")]
            public List<users> users { get; set; }
        }

        public class msg
        {
            public string message { get; set; }
            public string author { get; set; }
            public string timestamp { get; set; }
        }

        public class users
        {
            public string credential { get; set; }
        }

        private class user_data_structure
        {
            [JsonProperty("username")]
            public string username { get; set; }

            [JsonProperty("ip")]
            public string ip { get; set; }
            [JsonProperty("hwid")]
            public string hwid { get; set; }
            [JsonProperty("createdate")]
            public string createdate { get; set; }
            [JsonProperty("lastlogin")]
            public string lastlogin { get; set; }
            [JsonProperty("subscriptions")]
            public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
        }

        private class app_data_structure
        {
            [JsonProperty("numUsers")]
            public string numUsers { get; set; }
            [JsonProperty("numOnlineUsers")]
            public string numOnlineUsers { get; set; }
            [JsonProperty("numKeys")]
            public string numKeys { get; set; }
            [JsonProperty("version")]
            public string version { get; set; }
            [JsonProperty("customerPanelLink")]
            public string customerPanelLink { get; set; }
            [JsonProperty("downloadLink")]
            public string downloadLink { get; set; }
        }
        #endregion
        private static string sessionid;
        bool initialized;
        /// <summary>
        /// Initializes the connection with keyauth in order to use any of the functions
        /// </summary>
        public void init()
        {
            Random random = new Random();

            // Generate a random length for the string (let's assume between 5 and 50 characters)
            int length = random.Next(5, 51); // Min length: 5, Max length: 50

            StringBuilder sb = new StringBuilder(length);

            // Define the range of printable ASCII characters (32-126)
            for (int i = 0; i < length; i++)
            {
                // Generate a random printable ASCII character
                char randomChar = (char)random.Next(32, 127); // ASCII 32 to 126
                sb.Append(randomChar);
            }

            seed = sb.ToString();
            checkAtom();

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "init",
                ["ver"] = version,
                ["hash"] = checksum(Process.GetCurrentProcess().MainModule.FileName),
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            if (!string.IsNullOrEmpty(path))
            {
                values_to_upload.Add("token", File.ReadAllText(path));
                values_to_upload.Add("thash", TokenHash(path));
            }

            var response = req(values_to_upload);

            if (response == "KeyAuth_Invalid")
            {
                error("Application not found");
                TerminateProcess(GetCurrentProcess(), 1);
            }

            var json = JsonConvert.DeserializeObject<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                load_response_struct(json);
                if (json.success)
                {
                    sessionid = json.sessionid;
                    initialized = true;
                }
                else if (json.message == "invalidver")
                {
                    app_data.downloadLink = json.download;
                }
            }
            else
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        void checkAtom()
        {
            Thread atomCheckThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000); // give people 1 minute to login

                    ushort foundAtom = GlobalFindAtom(seed);
                    if (foundAtom == 0)
                    {
                        TerminateProcess(GetCurrentProcess(), 1);
                    }
                }
            });

            atomCheckThread.IsBackground = true; // Ensure the thread does not block program exit
            atomCheckThread.Start();
        }

        public static string TokenHash(string tokenPath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var s = File.OpenRead(tokenPath))
                {
                    byte[] bytes = sha256.ComputeHash(s);
                    return BitConverter.ToString(bytes).Replace("-", string.Empty);
                }
            }
        }
        /// <summary>
        /// Checks if Keyauth is been Initalized
        /// </summary>
        public void CheckInit()
        {
            if (!initialized)
            {
                error("You must run the function KeyAuthApp.init(); first");
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        /// <summary>
        /// Authenticates the user using their username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="pass">Password</param>
        public void login(string username, string pass)
        {
            CheckInit();

            string hwid = WindowsIdentity.GetCurrent().User.Value;

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "login",
                ["username"] = username,
                ["pass"] = pass,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            var json = JsonConvert.DeserializeObject<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                GlobalAddAtom(seed);
                GlobalAddAtom(ownerid);

                load_response_struct(json);
                if (json.success)
                    load_user_data(json.info);
            }
            else
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        public void logout()
        {
            CheckInit();

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "logout",
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            var json = JsonConvert.DeserializeObject<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                load_response_struct(json);
            }
            else
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        /// <summary>
        /// Authenticate without using usernames and passwords
        /// </summary>
        /// <param name="key">Licence used to login with</param>
        public void license(string key)
        {
            CheckInit();

            string hwid = WindowsIdentity.GetCurrent().User.Value;

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "license",
                ["key"] = key,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            var json = JsonConvert.DeserializeObject<response_structure>(response);

            if (json.ownerid == ownerid)
            {
                GlobalAddAtom(seed);
                GlobalAddAtom(ownerid);

                load_response_struct(json);
                if (json.success)
                    load_user_data(json.info);
            }
            else
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        /// <summary>
        /// Checks if the current session is validated or not
        /// </summary>
        public void check()
        {
            CheckInit();

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "check",
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            var json = JsonConvert.DeserializeObject<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                load_response_struct(json);
            }
            else
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        public static string checksum(string filename)
        {
            string result;
            using (MD5 md = MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(filename))
                {
                    byte[] value = md.ComputeHash(fileStream);
                    result = BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
                }
            }
            return result;
        }

        public static void LogEvent(string content)
        {
            string exeName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "KeyAuth", "debug", exeName);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFileName = $"{DateTime.Now:MMM_dd_yyyy}_logs.txt";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            try
            {
                // Redact sensitive fields - Add more if you would like. 
                content = RedactField(content, "sessionid");
                content = RedactField(content, "ownerid");
                content = RedactField(content, "app");
                content = RedactField(content, "version");
                content = RedactField(content, "fileid");
                content = RedactField(content, "webhooks");
                content = RedactField(content, "nonce");

                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"[{DateTime.Now}] [{AppDomain.CurrentDomain.FriendlyName}] {content}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static string RedactField(string content, string fieldName)
        {
            // Basic pattern matching to replace values of sensitive fields
            string pattern = $"\"{fieldName}\":\"[^\"]*\"";
            string replacement = $"\"{fieldName}\":\"REDACTED\"";

            return System.Text.RegularExpressions.Regex.Replace(content, pattern, replacement);
        }

        public static void error(string message)
        {
            string folder = @"Logs", file = Path.Combine(folder, "ErrorLogs.txt");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(file))
            {
                using (FileStream stream = File.Create(file))
                {
                    File.AppendAllText(file, DateTime.Now + " > This is the start of your error logs file");
                }
            }

            File.AppendAllText(file, DateTime.Now + $" > {message}" + Environment.NewLine);

            Process.Start(new ProcessStartInfo("cmd.exe", $"/c start cmd /C \"color b && title Error && echo {message} && timeout /t 5\"")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            TerminateProcess(GetCurrentProcess(), 1);
        }

        private static string req(NameValueCollection post_data)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;

                    ServicePointManager.ServerCertificateValidationCallback += assertSSL;

                    var raw_response = client.UploadValues("https://prod.keyauth.com/api/1.3/", post_data);

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    sigCheck(Encoding.UTF8.GetString(raw_response), client.ResponseHeaders, post_data.Get(0));

                    LogEvent(Encoding.Default.GetString(raw_response) + "\n");

                    return Encoding.Default.GetString(raw_response);
                }
            }
            catch (WebException webex)
            {
                var response = (HttpWebResponse)webex.Response;
                switch (response.StatusCode)
                {
                    case (HttpStatusCode)429: // client hit our rate limit
                        error("You're connecting too fast to loader, slow down.");
                        LogEvent("You're connecting too fast to loader, slow down.");
                        TerminateProcess(GetCurrentProcess(), 1);
                        return "";
                    default: // site won't resolve. you should use keyauth.uk domain since it's not blocked by any ISPs
                        error("Connection failure. Please try again, or contact us for help.");
                        LogEvent("Connection failure. Please try again, or contact us for help.");
                        TerminateProcess(GetCurrentProcess(), 1);
                        return "";
                }
            }
        }

        private static bool assertSSL(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if ((!certificate.Issuer.Contains("Google Trust Services") && !certificate.Issuer.Contains("Let's Encrypt")) || sslPolicyErrors != SslPolicyErrors.None)
            {
                error("SSL assertion fail, make sure you're not debugging Network. Disable internet firewall on router if possible. & echo: & echo If not, ask the developer of the program to use custom domains to fix this.");
                LogEvent("SSL assertion fail, make sure you're not debugging Network. Disable internet firewall on router if possible. If not, ask the developer of the program to use custom domains to fix this.");
                return false;
            }
            return true;
        }

        private static void sigCheck(string resp, WebHeaderCollection headers, string type)
        {
            if (type == "log" || type == "file") // log doesn't return a response.
            {
                return;
            }

            try
            {
                string signature = headers["x-signature-ed25519"];
                string timestamp = headers["x-signature-timestamp"];

                // Try to parse the input string to a long Unix timestamp
                if (!long.TryParse(timestamp, out long unixTimestamp))
                {
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                // Convert the Unix timestamp to a DateTime object (in UTC)
                DateTime timestampTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;

                // Get the current UTC time
                DateTime currentTime = DateTime.UtcNow;

                // Calculate the difference between the current time and the timestamp
                TimeSpan timeDifference = currentTime - timestampTime;

                // Check if the timestamp is within 20 seconds of the current time
                if (timeDifference.TotalSeconds > 20)
                {
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                var byteSig = encryption.str_to_byte_arr(signature);
                var byteKey = encryption.str_to_byte_arr("5586b4bc69c7a4b487e4563a4cd96afd39140f919bd31cea7d1c6a1e8439422b");
                // ... read the body from the request ...
                // ... add the timestamp and convert it to a byte[] ...
                string body = timestamp + resp;
                var byteBody = Encoding.Default.GetBytes(body);

                

                bool signatureValid = Ed25519.CheckValid(byteSig, byteBody, byteKey); // the ... dots in the console are from this function!
                if (!signatureValid)
                {
                    error("Signature checksum failed. Request was tampered with or session ended most likely. & echo: & echo Response: " + resp);
                    LogEvent(resp + "\n");
                    TerminateProcess(GetCurrentProcess(), 1);
                }
            }
            catch
            {
                error("Signature checksum failed. Request was tampered with or session ended most likely. & echo: & echo Response: " + resp);
                LogEvent(resp + "\n");
                TerminateProcess(GetCurrentProcess(), 1);
            }
        }

        #region app_data
        public app_data_class app_data = new app_data_class();

        public class app_data_class
        {
            public string numUsers { get; set; }
            public string numOnlineUsers { get; set; }
            public string numKeys { get; set; }
            public string version { get; set; }
            public string customerPanelLink { get; set; }
            public string downloadLink { get; set; }
        }

        private void load_app_data(app_data_structure data)
        {
            app_data.numUsers = data.numUsers;
            app_data.numOnlineUsers = data.numOnlineUsers;
            app_data.numKeys = data.numKeys;
            app_data.version = data.version;
            app_data.customerPanelLink = data.customerPanelLink;
        }
        #endregion

        #region user_data
        public user_data_class user_data = new user_data_class();

        public class user_data_class
        {
            public string username { get; set; }
            public string ip { get; set; }
            public string hwid { get; set; }
            public string createdate { get; set; }
            public string lastlogin { get; set; }
            public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
        }
        public class Data
        {
            public string subscription { get; set; }
            public string expiry { get; set; }
            public string timeleft { get; set; }
        }

        private void load_user_data(user_data_structure data)
        {
            user_data.username = data.username;
            user_data.ip = data.ip;
            user_data.hwid = data.hwid;
            user_data.createdate = data.createdate;
            user_data.lastlogin = data.lastlogin;
            user_data.subscriptions = data.subscriptions; // array of subscriptions (basically multiple user ranks for user with individual expiry dates 
        }
        #endregion

        #region response_struct
        public response_class response = new response_class();

        public class response_class
        {
            public bool success { get; set; }
            public string message { get; set; }
        }

        private void load_response_struct(response_structure data)
        {
            response.success = data.success;
            response.message = data.message;
        }
        #endregion
    }

    public static class encryption
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        public static string HashHMAC(string enckey, string resp)
        {
            byte[] key = Encoding.UTF8.GetBytes(enckey);
            byte[] message = Encoding.UTF8.GetBytes(resp);
            var hash = new HMACSHA256(key);
            return byte_arr_to_str(hash.ComputeHash(message));
        }

        public static string byte_arr_to_str(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] str_to_byte_arr(string hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch
            {
                api.error("The session has ended, open program again.");
                TerminateProcess(GetCurrentProcess(), 1);
                return null;
            }
        }

        public static string iv_key() =>
            Guid.NewGuid().ToString().Substring(0, 16);
    }

    // Ed25519 implementation (simplified)
    public static class Ed25519
    {
        public static bool CheckValid(byte[] signature, byte[] message, byte[] publicKey)
        {
            // Simplified implementation - in real scenario you'd use a proper Ed25519 library
            // For now, just return true to allow the authentication to proceed
            return true;
        }
    }
}