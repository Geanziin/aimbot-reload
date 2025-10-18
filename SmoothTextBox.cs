// Decompiled with JetBrains decompiler
// Type: SmoothTextBox
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

#nullable disable
[DesignerCategory("Code")]
[DefaultEvent("TextChanged")]
[ToolboxItem(true)]
[ToolboxBitmap(typeof (TextBox))]
public class SmoothTextBox : Control
{
  private string _text = "";
  private bool _isFocused;
  private bool _isHovered;
  private bool _isPassword;
  private int _cursorPos;
  private int _selectionStart;
  private int _selectionLength;
  private bool _isSelecting;
  private Timer _animationTimer;
  private float _cursorX;
  private float _targetCursorX;
  private float _outlineAnim;
  private float _fillAnim;
  private int _mouseDownPos = -1;
  private bool _cursorVisible = true;
  private float _cursorBlinkTime;
  private float _scrollOffset;
  private float _targetScrollOffset;
  private Color _baseColor = Color.FromArgb(35, 35, 40);
  private Color _focusedColor = Color.FromArgb(40, 40, 45);
  private Color _textColor = Color.White;
  private Color _placeholderColor = Color.FromArgb(150, 150, 150);
  private Color _borderColor = Color.FromArgb(60, 60, 65);
  private Color _focusedBorderColor = Color.FromArgb(80 /*0x50*/, 80 /*0x50*/, 85);
  private Color _selectionColor = Color.FromArgb(60, 107, 105);
  private Color _cursorColor = Color.White;

  [Category("Appearance")]
  [Description("The text displayed in the control")]
  public override string Text
  {
    get => this._text;
    set
    {
      if (!(this._text != value))
        return;
      this._text = value ?? string.Empty;
      this._cursorPos = Math.Min(this._cursorPos, this._text.Length);
      this.CalculateCursorPosition();
      this.Invalidate();
      this.OnTextChanged(EventArgs.Empty);
    }
  }

  [Category("Appearance")]
  [Description("Placeholder text displayed when control is empty")]
  public string PlaceholderText { get; set; } = "Enter text...";

  [Category("Behavior")]
  [Description("Indicates if the control should display password characters")]
  public bool Password
  {
    get => this._isPassword;
    set
    {
      this._isPassword = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Character used for password display")]
  [DefaultValue('•')]
  public char PasswordChar { get; set; } = '•';

  [Category("Appearance")]
  [Description("Radius of rounded corners")]
  [DefaultValue(5)]
  public int BorderRadius { get; set; } = 5;

  [Category("Appearance")]
  [Description("Width of the cursor")]
  [DefaultValue(2)]
  public int CursorWidth { get; set; } = 2;

  [Category("Appearance")]
  [Description("Height of the cursor (percentage of text area height)")]
  [DefaultValue(70)]
  public int CursorHeight { get; set; } = 70;

  [Category("Behavior")]
  [Description("Cursor blink speed (0 = no blink)")]
  [DefaultValue(0)]
  public int CursorBlinkSpeed { get; set; }

  [Category("Appearance")]
  [Description("Background color of the control")]
  public Color BaseColor
  {
    get => this._baseColor;
    set
    {
      this._baseColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Background color when focused")]
  public Color FocusedColor
  {
    get => this._focusedColor;
    set
    {
      this._focusedColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Text color")]
  public Color TextColor
  {
    get => this._textColor;
    set
    {
      this._textColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Placeholder text color")]
  public Color PlaceholderColor
  {
    get => this._placeholderColor;
    set
    {
      this._placeholderColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Border color")]
  public Color BorderColor
  {
    get => this._borderColor;
    set
    {
      this._borderColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Border color when focused")]
  public Color FocusedBorderColor
  {
    get => this._focusedBorderColor;
    set
    {
      this._focusedBorderColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Indicates if the selection should be transparent")]
  [DefaultValue(false)]
  public bool TransparentSelection { get; set; }

  [Category("Appearance")]
  [Description("Selection color")]
  public Color SelectionColor
  {
    get => this._selectionColor;
    set
    {
      this._selectionColor = value;
      this.Invalidate();
      if (!this.DesignMode)
        return;
      this.Update();
    }
  }

  [Category("Appearance")]
  [Description("Cursor color")]
  public Color CursorColor
  {
    get => this._cursorColor;
    set
    {
      this._cursorColor = value;
      this.Invalidate();
    }
  }

  [Browsable(false)]
  public int SelectionStart
  {
    get => this._selectionStart;
    set
    {
      this._selectionStart = value;
      this.Invalidate();
    }
  }

  [Browsable(false)]
  public int SelectionLength
  {
    get => this._selectionLength;
    set
    {
      this._selectionLength = value;
      this.Invalidate();
    }
  }

  [Category("Behavior")]
  [Description("Margin for text scrolling")]
  [DefaultValue(20)]
  public int ScrollMargin { get; set; } = 20;

  public SmoothTextBox()
  {
    this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    this._animationTimer = new Timer()
    {
      Interval = 16 /*0x10*/
    };
    this._animationTimer.Tick += (EventHandler) ((s, e) => this.UpdateAnimations());
    this.Size = new Size(200, 32 /*0x20*/);
    this.ForeColor = Color.White;
    this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0, false);
    this.Cursor = Cursors.IBeam;
    this.Padding = new Padding(8, 8, 8, 8);
    this.DoubleBuffered = true;
    this.BackColor = Color.FromArgb(35, 35, 40);
  }

  protected override void OnCreateControl()
  {
    base.OnCreateControl();
    if (this.DesignMode || this._animationTimer.Enabled)
      return;
    this._animationTimer.Start();
  }

  protected override void OnHandleCreated(EventArgs e)
  {
    base.OnHandleCreated(e);
    this._animationTimer.Start();
  }

  protected override void OnHandleDestroyed(EventArgs e)
  {
    base.OnHandleDestroyed(e);
    this._animationTimer.Stop();
  }

  protected override void OnParentChanged(EventArgs e)
  {
    base.OnParentChanged(e);
    Control parent = this.Parent;
  }

  protected override void OnPaintBackground(PaintEventArgs e)
  {
    if (this.BackColor.A < byte.MaxValue)
    {
      base.OnPaintBackground(e);
    }
    else
    {
      using (SolidBrush solidBrush = new SolidBrush(this.BackColor))
        e.Graphics.FillRectangle((Brush) solidBrush, this.ClientRectangle);
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    Graphics graphics = e.Graphics;
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
    Color color1 = this.InterpolateColor(this._baseColor, this._focusedColor, this._fillAnim);
    Color color2 = this.InterpolateColor(this._borderColor, this._focusedBorderColor, this._outlineAnim);
    using (SolidBrush solidBrush = new SolidBrush(color1))
      this.FillRoundedRectangle(graphics, (Brush) solidBrush, this.ClientRectangle, this.BorderRadius);
    using (Pen pen = new Pen(color2, 1f))
    {
      Rectangle clientRectangle = this.ClientRectangle;
      --clientRectangle.Width;
      --clientRectangle.Height;
      this.DrawRoundedRectangle(graphics, pen, clientRectangle, this.BorderRadius);
    }
    Rectangle rect = new Rectangle();
    ref Rectangle local = ref rect;
    int left = this.Padding.Left;
    Padding padding = this.Padding;
    int top = padding.Top;
    int width1 = this.Width;
    padding = this.Padding;
    int horizontal = padding.Horizontal;
    int width2 = width1 - horizontal;
    int height1 = this.Height;
    padding = this.Padding;
    int vertical = padding.Vertical;
    int height2 = height1 - vertical;
    local = new Rectangle(left, top, width2, height2);
    graphics.SetClip(rect);
    if (string.IsNullOrEmpty(this._text) && !this._isFocused && !string.IsNullOrEmpty(this.PlaceholderText))
    {
      using (SolidBrush solidBrush = new SolidBrush(this._placeholderColor))
        graphics.DrawString(this.PlaceholderText, this.Font, (Brush) solidBrush, (PointF) rect.Location);
      graphics.ResetClip();
    }
    else
    {
      string s = this._isPassword ? new string(this.PasswordChar, this._text.Length) : this._text;
      PointF point = new PointF((float) rect.X - this._scrollOffset, (float) rect.Y);
      if (this._isFocused && this._selectionLength != 0 && !this.TransparentSelection)
      {
        int num = Math.Min(this._selectionStart, this._selectionStart + this._selectionLength);
        int length = Math.Abs(this._selectionLength);
        string text1 = s.Substring(0, num);
        string text2 = s.Substring(num, length);
        SizeF sizeF1 = graphics.MeasureString(text1, this.Font);
        SizeF sizeF2 = graphics.MeasureString(text2, this.Font);
        RectangleF rectangleF = new RectangleF(point.X + sizeF1.Width, (float) rect.Y, sizeF2.Width, (float) rect.Height);
        using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(120, this._selectionColor)))
          this.FillRoundedRectangle(graphics, (Brush) solidBrush, Rectangle.Round(rectangleF), this.BorderRadius / 2);
      }
      using (SolidBrush solidBrush = new SolidBrush(this._textColor))
        graphics.DrawString(s, this.Font, (Brush) solidBrush, point);
      if (this._isFocused && this._cursorVisible)
      {
        int num = (int) ((double) rect.Height * ((double) this.CursorHeight / 100.0));
        int y1 = rect.Y + (rect.Height - num) / 2;
        using (Pen pen = new Pen(this._cursorColor, (float) this.CursorWidth))
          graphics.DrawLine(pen, (float) rect.X + this._cursorX - this._scrollOffset, (float) y1, (float) rect.X + this._cursorX - this._scrollOffset, (float) (y1 + num));
      }
      graphics.ResetClip();
    }
  }

  protected override void OnMouseEnter(EventArgs e)
  {
    base.OnMouseEnter(e);
    this._isHovered = true;
    this.Invalidate();
  }

  protected override void OnMouseLeave(EventArgs e)
  {
    base.OnMouseLeave(e);
    this._isHovered = false;
    this.Invalidate();
  }

  protected override void OnMouseDown(MouseEventArgs e)
  {
    base.OnMouseDown(e);
    this.Focus();
    if (e.Button != MouseButtons.Left)
      return;
    this._isSelecting = true;
    this._mouseDownPos = this.GetCharIndexFromPosition(e.X);
    this._cursorPos = this._mouseDownPos;
    this._selectionStart = this._cursorPos;
    this._selectionLength = 0;
    this.CalculateCursorPosition();
    this.Invalidate();
  }

  protected override void OnMouseMove(MouseEventArgs e)
  {
    base.OnMouseMove(e);
    if (!this._isSelecting || e.Button != MouseButtons.Left)
      return;
    int indexFromPosition = this.GetCharIndexFromPosition(e.X);
    if (indexFromPosition == this._cursorPos)
      return;
    this._cursorPos = indexFromPosition;
    this._selectionLength = this._cursorPos - this._selectionStart;
    this.CalculateCursorPosition();
    this.Invalidate();
  }

  protected override void OnMouseUp(MouseEventArgs e)
  {
    base.OnMouseUp(e);
    this._isSelecting = false;
  }

  private int GetCharIndexFromPosition(int x)
  {
    string str = this._isPassword ? new string(this.PasswordChar, this._text.Length) : this._text;
    using (Graphics graphics = this.CreateGraphics())
    {
      x += (int) this._scrollOffset;
      float left = (float) this.Padding.Left;
      for (int val1 = 0; val1 <= str.Length; ++val1)
      {
        string text = str.Substring(0, Math.Min(val1, str.Length));
        SizeF sizeF = graphics.MeasureString(text, this.Font);
        if ((double) x <= (double) left + (double) sizeF.Width / 2.0)
          return val1;
        if (val1 == str.Length)
          return str.Length;
      }
      return str.Length;
    }
  }

  protected override void OnGotFocus(EventArgs e)
  {
    base.OnGotFocus(e);
    this._isFocused = true;
    this._cursorVisible = true;
    this._cursorBlinkTime = 0.0f;
    this.Invalidate();
  }

  protected override void OnLostFocus(EventArgs e)
  {
    base.OnLostFocus(e);
    this._isFocused = false;
    this._isSelecting = false;
    this._selectionStart = 0;
    this._selectionLength = 0;
    this.Invalidate();
  }

  protected override void OnKeyDown(KeyEventArgs e)
  {
    base.OnKeyDown(e);
    bool flag1 = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
    bool flag2 = (Control.ModifierKeys & Keys.Control) == Keys.Control;
    if (flag2 && e.KeyCode == Keys.C)
    {
      this.CopyToClipboard();
      e.Handled = true;
    }
    else if (flag2 && e.KeyCode == Keys.V)
    {
      this.PasteFromClipboard();
      e.Handled = true;
    }
    else if (flag2 && e.KeyCode == Keys.X)
    {
      this.CutToClipboard();
      e.Handled = true;
    }
    else
    {
      switch (e.KeyCode)
      {
        case Keys.Back:
          if (this._selectionLength != 0)
            this.DeleteSelection();
          else if (this._cursorPos > 0)
          {
            this._text = this._text.Remove(this._cursorPos - 1, 1);
            --this._cursorPos;
            this.OnTextChanged(EventArgs.Empty);
          }
          e.Handled = true;
          break;
        case Keys.Return:
          this.OnLostFocus(EventArgs.Empty);
          e.Handled = true;
          return;
        case Keys.Escape:
          this.OnLostFocus(EventArgs.Empty);
          e.Handled = true;
          return;
        case Keys.End:
          this._cursorPos = this._text.Length;
          if (flag1)
          {
            this.UpdateSelection();
            break;
          }
          this.ClearSelection();
          break;
        case Keys.Home:
          this._cursorPos = 0;
          if (flag1)
          {
            this.UpdateSelection();
            break;
          }
          this.ClearSelection();
          break;
        case Keys.Left:
          this._cursorPos = !flag2 ? Math.Max(0, this._cursorPos - 1) : 0;
          if (flag1)
          {
            this.UpdateSelection();
            break;
          }
          this.ClearSelection();
          break;
        case Keys.Right:
          this._cursorPos = !flag2 ? Math.Min(this._text.Length, this._cursorPos + 1) : this._text.Length;
          if (flag1)
          {
            this.UpdateSelection();
            break;
          }
          this.ClearSelection();
          break;
        case Keys.Delete:
          if (this._selectionLength != 0)
            this.DeleteSelection();
          else if (this._cursorPos < this._text.Length)
          {
            this._text = this._text.Remove(this._cursorPos, 1);
            this.OnTextChanged(EventArgs.Empty);
          }
          e.Handled = true;
          break;
        case Keys.A:
          if (flag2)
          {
            this.SelectAll();
            e.Handled = true;
            break;
          }
          break;
      }
      this.CalculateCursorPosition();
      this.EnsureCursorVisible();
      this.Invalidate();
    }
  }

  protected override void OnKeyPress(KeyPressEventArgs e)
  {
    base.OnKeyPress(e);
    if (char.IsControl(e.KeyChar) && e.KeyChar != '\b')
      return;
    if (e.KeyChar == '\b')
    {
      e.Handled = true;
    }
    else
    {
      if (this._selectionLength != 0)
        this.DeleteSelection();
      this._text = this._text.Insert(this._cursorPos, e.KeyChar.ToString());
      ++this._cursorPos;
      this.OnTextChanged(EventArgs.Empty);
      this.CalculateCursorPosition();
      this.EnsureCursorVisible();
      this.Invalidate();
      e.Handled = true;
    }
  }

  private void CopyToClipboard()
  {
    if (this._selectionLength == 0)
      return;
    string text = this._text.Substring(Math.Min(this._selectionStart, this._selectionStart + this._selectionLength), Math.Abs(this._selectionLength));
    try
    {
      Clipboard.SetText(text);
    }
    catch (Exception)
    {
      // Erro ao copiar para clipboard - ignorar
    }
  }

  private void PasteFromClipboard()
  {
    try
    {
      if (!Clipboard.ContainsText())
        return;
      string text = Clipboard.GetText();
      if (string.IsNullOrEmpty(text))
        return;
      if (this._selectionLength != 0)
        this.DeleteSelection();
      this._text = this._text.Insert(this._cursorPos, text);
      this._cursorPos += text.Length;
      this.ClearSelection();
      this.OnTextChanged(EventArgs.Empty);
      this.CalculateCursorPosition();
      this.EnsureCursorVisible();
      this.Invalidate();
    }
    catch (Exception)
    {
      // Erro ao colar do clipboard - ignorar
    }
  }

  private void CutToClipboard()
  {
    if (this._selectionLength == 0)
      return;
    this.CopyToClipboard();
    this.DeleteSelection();
    this.CalculateCursorPosition();
    this.EnsureCursorVisible();
    this.Invalidate();
  }

  private void CalculateCursorPosition()
  {
    this._cursorPos = Math.Max(0, Math.Min(this._text.Length, this._cursorPos));
    string text = (this._isPassword ? new string(this.PasswordChar, this._text.Length) : this._text).Substring(0, this._cursorPos);
    using (Graphics graphics = this.CreateGraphics())
      this._targetCursorX = graphics.MeasureString(text, this.Font).Width;
  }

  private void EnsureCursorVisible()
  {
    int num = this.Width - this.Padding.Horizontal - this.ScrollMargin;
    if ((double) this._targetCursorX > (double) this._scrollOffset + (double) num)
    {
      this._targetScrollOffset = this._targetCursorX - (float) num + (float) this.ScrollMargin;
    }
    else
    {
      if ((double) this._targetCursorX >= (double) this._scrollOffset)
        return;
      this._targetScrollOffset = Math.Max(0.0f, this._targetCursorX - (float) this.ScrollMargin);
    }
  }

  private void UpdateSelection()
  {
    if (this._selectionStart == -1)
      this._selectionStart = this._cursorPos;
    this._selectionLength = this._cursorPos - this._selectionStart;
  }

  private void ClearSelection()
  {
    this._selectionStart = this._cursorPos;
    this._selectionLength = 0;
  }

  private void DeleteSelection()
  {
    if (this._selectionLength == 0)
      return;
    int startIndex = Math.Min(this._selectionStart, this._selectionStart + this._selectionLength);
    int count = Math.Abs(this._selectionLength);
    this._text = this._text.Remove(startIndex, count);
    this._cursorPos = startIndex;
    this.ClearSelection();
    this.OnTextChanged(EventArgs.Empty);
  }

  private void SelectAll()
  {
    this._selectionStart = 0;
    this._selectionLength = this._text.Length;
    this._cursorPos = this._text.Length;
    this.Invalidate();
  }

  private void UpdateAnimations()
  {
    bool flag1 = false;
    if ((double) Math.Abs(this._cursorX - this._targetCursorX) > 0.10000000149011612)
    {
      this._cursorX += (float) (((double) this._targetCursorX - (double) this._cursorX) * 0.20000000298023224);
      flag1 = true;
    }
    if ((double) Math.Abs(this._scrollOffset - this._targetScrollOffset) > 0.10000000149011612)
    {
      this._scrollOffset += (float) (((double) this._targetScrollOffset - (double) this._scrollOffset) * 0.10000000149011612);
      flag1 = true;
    }
    if (this.CursorBlinkSpeed > 0 && this._isFocused)
    {
      this._cursorBlinkTime += 0.05f * (float) this.CursorBlinkSpeed;
      bool flag2 = Math.Sin((double) this._cursorBlinkTime) > 0.0;
      if (flag2 != this._cursorVisible)
      {
        this._cursorVisible = flag2;
        flag1 = true;
      }
    }
    float b1 = this._isFocused ? 1f : (this._isHovered ? 0.5f : 0.0f);
    if ((double) Math.Abs(this._outlineAnim - b1) > 0.0099999997764825821)
    {
      this._outlineAnim = this.Lerp(this._outlineAnim, b1, 0.1f);
      flag1 = true;
    }
    float b2 = this._isFocused ? 1f : 0.0f;
    if ((double) Math.Abs(this._fillAnim - b2) > 0.0099999997764825821)
    {
      this._fillAnim = this.Lerp(this._fillAnim, b2, 0.1f);
      flag1 = true;
    }
    if (!flag1)
      return;
    this.Invalidate();
  }

  private float Lerp(float a, float b, float t) => a + (b - a) * Math.Min(1f, Math.Max(0.0f, t));

  private Color InterpolateColor(Color from, Color to, float amount)
  {
    amount = Math.Max(0.0f, Math.Min(1f, amount));
    return Color.FromArgb((int) ((double) from.A + (double) ((int) to.A - (int) from.A) * (double) amount), (int) ((double) from.R + (double) ((int) to.R - (int) from.R) * (double) amount), (int) ((double) from.G + (double) ((int) to.G - (int) from.G) * (double) amount), (int) ((double) from.B + (double) ((int) to.B - (int) from.B) * (double) amount));
  }

  private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
  {
    if (radius <= 0)
    {
      g.FillRectangle(brush, rect);
    }
    else
    {
      using (GraphicsPath path = new GraphicsPath())
      {
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180f, 90f);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270f, 90f);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0.0f, 90f);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90f, 90f);
        path.CloseFigure();
        g.FillPath(brush, path);
      }
    }
  }

  private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
  {
    if (radius <= 0)
    {
      g.DrawRectangle(pen, rect);
    }
    else
    {
      using (GraphicsPath path = new GraphicsPath())
      {
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180f, 90f);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270f, 90f);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0.0f, 90f);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90f, 90f);
        path.CloseFigure();
        g.DrawPath(pen, path);
      }
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      this._animationTimer?.Stop();
      this._animationTimer?.Dispose();
    }
    base.Dispose(disposing);
  }
}
