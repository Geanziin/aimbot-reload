using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace WindowsFormsApp1
{
    public partial class KeyAuthForm : Form
    {
        private api keyAuthApp;
        private bool isAuthenticating = false;

        // Controles da interface
        private Guna2Panel mainPanel;
        private Guna2Panel headerPanel;
        private Guna2Panel contentPanel;
        private Guna2Button btnAuthenticate;
        private Guna2Button btnCancel;
        private Guna2Label lblTitle;
        private Guna2Label lblStatus;
        private Guna2Label lblIP;
        private Guna2ProgressBar progressBar;
        private Guna2PictureBox pictureBox;

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
            this.Text = "x7 aimlock - Autenticação";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(15, 15, 16);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Painel principal
            mainPanel = new Guna2Panel();
            mainPanel.Size = new Size(400, 300);
            mainPanel.Location = new Point(0, 0);
            mainPanel.BackColor = Color.FromArgb(20, 20, 21);
            mainPanel.BorderRadius = 15;
            mainPanel.BorderThickness = 1;
            mainPanel.BorderColor = Color.FromArgb(35, 35, 36);

            // Painel do cabeçalho
            headerPanel = new Guna2Panel();
            headerPanel.Size = new Size(380, 50);
            headerPanel.Location = new Point(10, 10);
            headerPanel.BackColor = Color.FromArgb(147, 51, 234);
            headerPanel.BorderRadius = 10;

            // Título
            lblTitle = new Guna2Label();
            lblTitle.Text = "🔐 Autenticação KeyAuth";
            lblTitle.Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSize = true;

            // Painel de conteúdo
            contentPanel = new Guna2Panel();
            contentPanel.Size = new Size(380, 200);
            contentPanel.Location = new Point(10, 70);
            contentPanel.BackColor = Color.Transparent;

            // Label de status
            lblStatus = new Guna2Label();
            lblStatus.Text = "Verificando conexão...";
            lblStatus.Font = new Font("Microsoft Sans Serif", 11);
            lblStatus.ForeColor = Color.White;
            lblStatus.Location = new Point(20, 20);
            lblStatus.AutoSize = true;

            // Label do IP
            lblIP = new Guna2Label();
            lblIP.Text = "IP: Carregando...";
            lblIP.Font = new Font("Microsoft Sans Serif", 10);
            lblIP.ForeColor = Color.FromArgb(200, 200, 200);
            lblIP.Location = new Point(20, 50);
            lblIP.AutoSize = true;

            // Barra de progresso
            progressBar = new Guna2ProgressBar();
            progressBar.Size = new Size(340, 8);
            progressBar.Location = new Point(20, 80);
            progressBar.ProgressColor = Color.FromArgb(147, 51, 234);
            progressBar.ProgressColor2 = Color.FromArgb(147, 51, 234);
            progressBar.Value = 0;

            // Botão de autenticação
            btnAuthenticate = new Guna2Button();
            btnAuthenticate.Text = "🔑 Autenticar";
            btnAuthenticate.Size = new Size(150, 40);
            btnAuthenticate.Location = new Point(20, 120);
            btnAuthenticate.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            btnAuthenticate.ForeColor = Color.White;
            btnAuthenticate.BackColor = Color.FromArgb(147, 51, 234);
            btnAuthenticate.BorderRadius = 8;
            btnAuthenticate.BorderThickness = 0;
            btnAuthenticate.Click += BtnAuthenticate_Click;

            // Botão de cancelar
            btnCancel = new Guna2Button();
            btnCancel.Text = "❌ Cancelar";
            btnCancel.Size = new Size(150, 40);
            btnCancel.Location = new Point(190, 120);
            btnCancel.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.BackColor = Color.FromArgb(220, 53, 69);
            btnCancel.BorderRadius = 8;
            btnCancel.BorderThickness = 0;
            btnCancel.Click += BtnCancel_Click;

            // Adicionar controles aos painéis
            headerPanel.Controls.Add(lblTitle);
            contentPanel.Controls.Add(lblStatus);
            contentPanel.Controls.Add(lblIP);
            contentPanel.Controls.Add(progressBar);
            contentPanel.Controls.Add(btnAuthenticate);
            contentPanel.Controls.Add(btnCancel);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(contentPanel);
            this.Controls.Add(mainPanel);

            this.ResumeLayout(false);
        }

        private async void InitializeAsync()
        {
            try
            {
                // Obter IP do usuário
                lblStatus.Text = "Obtendo seu IP...";
                progressBar.Value = 25;
                await Task.Delay(500);

                string userIP = await keyAuthApp.GetUserIP();
                if (!string.IsNullOrEmpty(userIP))
                {
                    lblIP.Text = $"IP: {userIP}";
                    lblStatus.Text = "Pronto para autenticação KeyAuth";
                    progressBar.Value = 50;
                    btnAuthenticate.Enabled = true;
                }
                else
                {
                    lblStatus.Text = "Erro ao obter IP";
                    lblIP.Text = "IP: Não disponível";
                    progressBar.Value = 0;
                    btnAuthenticate.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Erro: {ex.Message}";
                progressBar.Value = 0;
                btnAuthenticate.Enabled = false;
            }
        }

        private async void BtnAuthenticate_Click(object sender, EventArgs e)
        {
            if (isAuthenticating) return;

            isAuthenticating = true;
            btnAuthenticate.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                lblStatus.Text = "Autenticando...";
                progressBar.Value = 75;

                bool success = await keyAuthApp.AuthenticateByIP();

                if (success)
                {
                    lblStatus.Text = "✅ Autenticação bem-sucedida!";
                    progressBar.Value = 100;
                    progressBar.ProgressColor = Color.FromArgb(40, 167, 69);
                    
                    await Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "❌ Falha na autenticação";
                    progressBar.Value = 0;
                    progressBar.ProgressColor = Color.FromArgb(220, 53, 69);
                    btnAuthenticate.Enabled = true;
                    btnCancel.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Erro: {ex.Message}";
                progressBar.Value = 0;
                btnAuthenticate.Enabled = true;
                btnCancel.Enabled = true;
            }
            finally
            {
                isAuthenticating = false;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
            // Adicionar eventos de mouse para arrastar
            mainPanel.MouseDown += MainPanel_MouseDown;
            mainPanel.MouseMove += MainPanel_MouseMove;
            mainPanel.MouseUp += MainPanel_MouseUp;
            headerPanel.MouseDown += MainPanel_MouseDown;
            headerPanel.MouseMove += MainPanel_MouseMove;
            headerPanel.MouseUp += MainPanel_MouseUp;
        }
    }
}
