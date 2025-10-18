// Decompiled with JetBrains decompiler
// Type: SmoothScroll
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
public class SmoothScroll
{
  private Guna2Panel contentPanel;
  private Panel scrollIndicator;
  private Timer smoothScrollTimer;
  private float targetScrollPosition;
  private float currentScrollPosition;
  private const float SCROLL_SPEED = 30f;
  private bool isDraggingScrollbar;
  private int dragStartY;
  private int lastMouseY;
  private int maxScrollPosition;
  private Control[] contentControls;
  private const float SMOOTH_FACTOR = 0.35f;
  private float scrollVelocity;
  private const float MOMENTUM_DECAY = 0.9f;

  public SmoothScroll(Guna2Panel contentPanel, Panel scrollIndicator)
  {
    this.contentPanel = contentPanel;
    this.scrollIndicator = scrollIndicator;
    ((ScrollableControl) contentPanel).AutoScroll = false;
    this.contentControls = new Control[((Control) contentPanel).Controls.Count];
    ((Control) contentPanel).Controls.CopyTo((Array) this.contentControls, 0);
    this.smoothScrollTimer = new Timer() { Interval = 8 };
    this.smoothScrollTimer.Tick += new EventHandler(this.SmoothScrollTimer_Tick);
    ((Control) contentPanel).MouseWheel += new MouseEventHandler(this.ContentPanel_MouseWheel);
    scrollIndicator.MouseDown += new MouseEventHandler(this.ScrollIndicator_MouseDown);
    ((Control) contentPanel).MouseMove += new MouseEventHandler(this.ScrollIndicator_MouseMove);
    scrollIndicator.MouseMove += new MouseEventHandler(this.ScrollIndicator_MouseMove);
    ((Control) contentPanel).MouseUp += new MouseEventHandler(this.ScrollIndicator_MouseUp);
    scrollIndicator.MouseUp += new MouseEventHandler(this.ScrollIndicator_MouseUp);
    this.currentScrollPosition = 0.0f;
    this.targetScrollPosition = 0.0f;
    this.RecalcularLimitesRolagem();
    this.smoothScrollTimer.Start();
  }

  public void RecalcularLimitesRolagem()
  {
    int num1 = 0;
    foreach (Control contentControl in this.contentControls)
    {
      int num2 = contentControl.Top + contentControl.Height;
      if (num2 > num1)
        num1 = num2;
    }
    this.maxScrollPosition = Math.Max(0, num1 + 20 - ((Control) this.contentPanel).Height);
    this.AjustarTamanhoScrollbar();
    this.AtualizarPosicaoControles();
  }

  private void AjustarTamanhoScrollbar()
  {
    int num1 = 0;
    foreach (Control contentControl in this.contentControls)
    {
      int num2 = contentControl.Top + contentControl.Height;
      if (num2 > num1)
        num1 = num2;
    }
    if (num1 <= ((Control) this.contentPanel).Height)
    {
      this.scrollIndicator.Visible = false;
    }
    else
    {
      this.scrollIndicator.Visible = true;
      this.scrollIndicator.Height = Math.Max(40, (int) ((double) ((Control) this.contentPanel).Height / (double) num1 * (double) ((Control) this.contentPanel).Height));
    }
  }

  private void ContentPanel_MouseWheel(object sender, MouseEventArgs e)
  {
    float num = e.Delta > 0 ? -30f : 30f;
    this.scrollVelocity += num * 0.3f;
    this.targetScrollPosition = Math.Max(0.0f, Math.Min(this.targetScrollPosition + num, (float) this.maxScrollPosition));
  }

  private void SmoothScrollTimer_Tick(object sender, EventArgs e)
  {
    bool flag = false;
    if ((double) Math.Abs(this.scrollVelocity) > 0.10000000149011612)
    {
      this.targetScrollPosition += this.scrollVelocity;
      this.targetScrollPosition = Math.Max(0.0f, Math.Min(this.targetScrollPosition, (float) this.maxScrollPosition));
      this.scrollVelocity *= 0.9f;
      flag = true;
    }
    float num = this.targetScrollPosition - this.currentScrollPosition;
    if ((double) Math.Abs(num) > 0.10000000149011612)
    {
      this.currentScrollPosition += num * 0.35f;
      flag = true;
    }
    else if ((double) Math.Abs(num) > 0.0099999997764825821)
    {
      this.currentScrollPosition = this.targetScrollPosition;
      flag = true;
    }
    if (!flag)
      return;
    this.AtualizarPosicaoControles();
  }

  private void AtualizarPosicaoControles()
  {
    ((Control) this.contentPanel).SuspendLayout();
    foreach (Control contentControl in this.contentControls)
    {
      if (contentControl.Tag == null)
        contentControl.Tag = (object) contentControl.Top;
      int num = Convert.ToInt32(contentControl.Tag) - (int) Math.Round((double) this.currentScrollPosition);
      if (contentControl.Top != num)
        contentControl.Top = num;
    }
    ((Control) this.contentPanel).ResumeLayout(false);
    this.AtualizarPosicaoIndicador();
  }

  private void AtualizarPosicaoIndicador()
  {
    if (this.maxScrollPosition <= 0)
    {
      this.scrollIndicator.Top = 0;
    }
    else
    {
      int num = (int) Math.Round((double) this.currentScrollPosition / (double) this.maxScrollPosition * (double) (((Control) this.contentPanel).Height - this.scrollIndicator.Height));
      if (this.scrollIndicator.Top == num)
        return;
      this.scrollIndicator.Top = num;
    }
  }

  private void ScrollIndicator_MouseDown(object sender, MouseEventArgs e)
  {
    this.isDraggingScrollbar = true;
    this.dragStartY = e.Y;
    this.lastMouseY = Cursor.Position.Y;
    this.scrollIndicator.Capture = true;
    this.scrollVelocity = 0.0f;
  }

  private void ScrollIndicator_MouseMove(object sender, MouseEventArgs e)
  {
    if (!this.isDraggingScrollbar)
      return;
    Point position = Cursor.Position;
    int num1 = position.Y - this.lastMouseY;
    position = Cursor.Position;
    this.lastMouseY = position.Y;
    if (num1 == 0)
      return;
    int num2 = ((Control) this.contentPanel).Height - this.scrollIndicator.Height;
    if (num2 <= 0)
      return;
    float num3 = (float) this.maxScrollPosition / (float) num2;
    this.targetScrollPosition = Math.Max(0.0f, Math.Min(this.targetScrollPosition + (float) num1 * num3, (float) this.maxScrollPosition));
    this.currentScrollPosition = this.targetScrollPosition;
    this.AtualizarPosicaoControles();
  }

  private void ScrollIndicator_MouseUp(object sender, MouseEventArgs e)
  {
    if (!this.isDraggingScrollbar)
      return;
    this.isDraggingScrollbar = false;
    this.scrollIndicator.Capture = false;
  }

  public void ScrollTo(float position)
  {
    this.targetScrollPosition = Math.Max(0.0f, Math.Min(position, (float) this.maxScrollPosition));
    this.scrollVelocity = 0.0f;
  }

  public void ScrollToInstant(float position)
  {
    this.targetScrollPosition = Math.Max(0.0f, Math.Min(position, (float) this.maxScrollPosition));
    this.currentScrollPosition = this.targetScrollPosition;
    this.scrollVelocity = 0.0f;
    this.AtualizarPosicaoControles();
  }

  public void Dispose()
  {
    if (this.smoothScrollTimer != null)
    {
      this.smoothScrollTimer.Stop();
      this.smoothScrollTimer.Dispose();
    }
    ((Control) this.contentPanel).MouseWheel -= new MouseEventHandler(this.ContentPanel_MouseWheel);
    this.scrollIndicator.MouseDown -= new MouseEventHandler(this.ScrollIndicator_MouseDown);
    ((Control) this.contentPanel).MouseMove -= new MouseEventHandler(this.ScrollIndicator_MouseMove);
    this.scrollIndicator.MouseMove -= new MouseEventHandler(this.ScrollIndicator_MouseMove);
    ((Control) this.contentPanel).MouseUp -= new MouseEventHandler(this.ScrollIndicator_MouseUp);
    this.scrollIndicator.MouseUp -= new MouseEventHandler(this.ScrollIndicator_MouseUp);
  }
}
