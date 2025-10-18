using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class KeyAuthForm : Form
    {
        private api keyAuthApp;
        private bool isAuthenticating = false;

        // Controles da interface
        private Panel mainPanel;
        private Label lblX7;
        private Label lblPrivate;
        private Label lblThankYou;
        private TextBox txtUserID;
        private Button btnEnter;
        private Label lblStatus;
        private ProgressBar progressBar;

        public KeyAuthForm(api keyAuthApp)
        {
            this.keyAuthApp = keyAuthApp;
            InitializeComponent();
            InitializeAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configurações do formulário
            this.Text = "x7 Private";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(20, 20, 21);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Painel principal
            mainPanel = new Panel();
            mainPanel.Size = new Size(400, 300);
            mainPanel.Location = new Point(0, 0);
            mainPanel.BackColor = Color.FromArgb(20, 20, 21);
            mainPanel.BorderStyle = BorderStyle.FixedSingle;

            // Label "X7" no canto superior esquerdo
            lblX7 = new Label();
            lblX7.Text = "X7";
            lblX7.Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold);
            lblX7.ForeColor = Color.White;
            lblX7.Location = new Point(20, 20);
            lblX7.AutoSize = true;

            // Label "x7 Private" centralizado
            lblPrivate = new Label();
            lblPrivate.Text = "x7 Private";
            lblPrivate.Font = new Font("Microsoft Sans Serif", 18, FontStyle.Bold);
            lblPrivate.ForeColor = Color.White;
            lblPrivate.Location = new Point(150, 80);
            lblPrivate.AutoSize = true;

            // Destacar "Private" em roxo
            lblPrivate.Text = "x7 ";
            var lblPrivatePurple = new Label();
            lblPrivatePurple.Text = "Private";
            lblPrivatePurple.Font = new Font("Microsoft Sans Serif", 18, FontStyle.Bold);
            lblPrivatePurple.ForeColor = Color.FromArgb(147, 51, 234);
            lblPrivatePurple.Location = new Point(190, 80);
            lblPrivatePurple.AutoSize = true;

            // Label "Thank you for choosing."
            lblThankYou = new Label();
            lblThankYou.Text = "Thank you for choosing.";
            lblThankYou.Font = new Font("Microsoft Sans Serif", 12);
            lblThankYou.ForeColor = Color.White;
            lblThankYou.Location = new Point(120, 120);
            lblThankYou.AutoSize = true;

            // Campo de entrada do ID do usuário
            txtUserID = new TextBox();
            txtUserID.Size = new Size(200, 30);
            txtUserID.Location = new Point(100, 160);
            txtUserID.Font = new Font("Microsoft Sans Serif", 11);
            txtUserID.BackColor = Color.FromArgb(35, 35, 36);
            txtUserID.ForeColor = Color.White;
            txtUserID.BorderStyle = BorderStyle.FixedSingle;
            txtUserID.PlaceholderText = "Enter your ID";
            txtUserID.TextAlign = HorizontalAlignment.Center;

            // Botão "Enter"
            btnEnter = new Button();
            btnEnter.Text = "Enter";
            btnEnter.Size = new Size(100, 35);
            btnEnter.Location = new Point(150, 200);
            btnEnter.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            btnEnter.ForeColor = Color.White;
            btnEnter.BackColor = Color.Transparent;
            btnEnter.FlatStyle = FlatStyle.Flat;
            btnEnter.FlatAppearance.BorderSize = 0;
            btnEnter.Cursor = Cursors.Hand;
            btnEnter.Click += BtnEnter_Click;

            // Label de status (invisível inicialmente)
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = new Font("Microsoft Sans Serif", 10);
            lblStatus.ForeColor = Color.FromArgb(220, 53, 69);
            lblStatus.Location = new Point(50, 250);
            lblStatus.Size = new Size(300, 20);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Visible = false;

            // Barra de progresso (invisível inicialmente)
            progressBar = new ProgressBar();
            progressBar.Size = new Size(200, 6);
            progressBar.Location = new Point(100, 240);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Visible = false;

            // Adicionar controles ao painel principal
            mainPanel.Controls.Add(lblX7);
            mainPanel.Controls.Add(lblPrivate);
            mainPanel.Controls.Add(lblPrivatePurple);
            mainPanel.Controls.Add(lblThankYou);
            mainPanel.Controls.Add(txtUserID);
            mainPanel.Controls.Add(btnEnter);
            mainPanel.Controls.Add(lblStatus);
            mainPanel.Controls.Add(progressBar);
            this.Controls.Add(mainPanel);

            // Permitir arrastar o formulário
            mainPanel.MouseDown += MainPanel_MouseDown;
            mainPanel.MouseMove += MainPanel_MouseMove;
            mainPanel.MouseUp += MainPanel_MouseUp;

            this.ResumeLayout(false);
        }

        private async void InitializeAsync()
        {
            try
            {
                // Obter IP do usuário em background
                string userIP = await keyAuthApp.GetUserIP();
                if (!string.IsNullOrEmpty(userIP))
                {
                    // Definir IP como placeholder do campo ID
                    txtUserID.PlaceholderText = $"Your IP: {userIP}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter IP: {ex.Message}");
            }
        }

        private async void BtnEnter_Click(object sender, EventArgs e)
        {
            if (isAuthenticating) return;

            string userID = txtUserID.Text.Trim();
            if (string.IsNullOrEmpty(userID))
            {
                ShowError("Please enter your ID");
                return;
            }

            isAuthenticating = true;
            btnEnter.Enabled = false;
            txtUserID.Enabled = false;

            try
            {
                ShowProgress("Authenticating...", true);

                // Usar o ID fornecido pelo usuário para autenticação
                bool success = await AuthenticateWithUserID(userID);

                if (success)
                {
                    ShowProgress("Authentication successful!", false);
                    await Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Authentication failed. Invalid ID or IP not authorized.");
                    btnEnter.Enabled = true;
                    txtUserID.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
                btnEnter.Enabled = true;
                txtUserID.Enabled = true;
            }
            finally
            {
                isAuthenticating = false;
            }
        }

        private async Task<bool> AuthenticateWithUserID(string userID)
        {
            try
            {
                // Obter IP do usuário
                string userIP = await keyAuthApp.GetUserIP();
                if (string.IsNullOrEmpty(userIP))
                {
                    return false;
                }

                // Preparar dados para autenticação usando o ID fornecido
                var authData = new
                {
                    type = "login",
                    username = userID, // Usar ID fornecido pelo usuário
                    password = "", // Sem senha necessária
                    ownerid = keyAuthApp.GetOwnerID(),
                    secret = keyAuthApp.GetSecret(),
                    version = keyAuthApp.GetVersion(),
                    name = keyAuthApp.GetName()
                };

                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(authData);
                var content = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                // Fazer requisição para KeyAuth
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.PostAsync("https://keyauth.win/api/1.2/", content);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                        
                        if (jsonResponse["success"]?.ToString() == "true")
                        {
                            keyAuthApp.SetAuthenticated(true);
                            keyAuthApp.SetSessionID(jsonResponse["sessionid"]?.ToString() ?? "");
                            keyAuthApp.SetUserIP(userIP);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na autenticação: {ex.Message}");
            }

            return false;
        }

        private void ShowError(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.FromArgb(220, 53, 69);
            lblStatus.Visible = true;
            progressBar.Visible = false;
        }

        private void ShowProgress(string message, bool showProgressBar)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.FromArgb(40, 167, 69);
            lblStatus.Visible = true;
            progressBar.Visible = showProgressBar;
            if (showProgressBar)
            {
                progressBar.Value = 50;
            }
        }

        // Permitir arrastar o formulário
        private bool mouseDown;
        private Point lastLocation;

        private void MainPanel_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void MainPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X,
                    (this.Location.Y - lastLocation.Y) + e.Y);
                this.Update();
            }
        }

        private void MainPanel_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Focar no campo de texto
            txtUserID.Focus();
        }
    }
}
