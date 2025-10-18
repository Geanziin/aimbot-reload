using System;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace WindowsFormsApp1;

internal static class Program
{
  // Instância global do KeyAuth
  public static api KeyAuthApp = new api(
    name: "x7 aimlock", // App name
    ownerid: "IBz1XyIXTp", // Account ID
    secret: "bc10f3702f8d3295c9542895ea2ae21c053cc43cb219e2f46d212c3e258d8e7e", // Secret key
    version: "1.0" // Application version
  );

  [STAThread]
  private static void Main()
  {
    try
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      // Mostrar formulário de autenticação KeyAuth
      using (var authForm = new KeyAuthForm(KeyAuthApp))
      {
        var authResult = authForm.ShowDialog();
        
        if (authResult == DialogResult.OK && KeyAuthApp.IsAuthenticated)
        {
          // Autenticação bem-sucedida, iniciar aplicação principal
          Application.Run(new Spotify());
        }
        else
        {
          // Usuário cancelou ou falhou na autenticação
          MessageBox.Show("Acesso negado. Aplicação será encerrada.", "Acesso Negado", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
          Environment.Exit(0);
        }
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro crítico: {ex.Message}", "Erro", 
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
      Environment.Exit(1);
    }
  }

  // Método para verificar periodicamente se a sessão ainda é válida
  public static async Task<bool> ValidateKeyAuthSession()
  {
    if (KeyAuthApp == null || !KeyAuthApp.IsAuthenticated)
      return false;

    try
    {
      return await KeyAuthApp.ValidateSession();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao validar sessão KeyAuth: {ex.Message}");
      return false;
    }
  }

  // Método para fazer logout do KeyAuth
  public static async Task LogoutKeyAuth()
  {
    if (KeyAuthApp != null && KeyAuthApp.IsAuthenticated)
    {
      try
      {
        await KeyAuthApp.Logout();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao fazer logout KeyAuth: {ex.Message}");
      }
    }
  }
}
