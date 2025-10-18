// Decompiled with JetBrains decompiler
// Type: SlidingTabControl
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

#nullable disable
public class SlidingTabControl : Control
{
  private int selectedIndex;
  private float animationX;
  private float targetX;
  private float[] textAlphaValues;
  private float[] targetAlphaValues;
  private float[] imageAlphaValues;
  private float[] targetImageAlphaValues;
  private Timer animTimer;
  private List<TabItem> tabs;
  private Font tabsFont;
  private int borderThickness = 1;
  private float animationSpeed = 0.25f;
  private float colorAnimationSpeed = 0.3f;
  private bool enableAnimations = true;
  private TabOrientation tabOrientation;
  private ImagePosition imagePosition;
  private Size imageSize = new Size(16 /*0x10*/, 16 /*0x10*/);
  private int imagePadding = 4;
  private ColorMatrix activeImageColorMatrix;
  private ColorMatrix inactiveImageColorMatrix;

  public event EventHandler SelectedIndexChanged;

  public SlidingTabControl()
  {
    this.tabs = new List<TabItem>()
    {
      new TabItem("Tab 1"),
      new TabItem("Tab 2"),
      new TabItem("Tab 3")
    };
    this.DoubleBuffered = true;
    this.Height = 40;
    this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    this.animTimer = new Timer() { Interval = 8 };
    this.animTimer.Tick += new EventHandler(this.AnimTimer_Tick);
    this.Cursor = Cursors.Hand;
    this.InitializeAnimations();
    this.InitializeImageColorMatrices();
  }

  [Category("Behavior")]
  [Description("Número de abas exibidas")]
  [DefaultValue(3)]
  public int TabCount
  {
    get
    {
      List<TabItem> tabs = this.tabs;
      return tabs == null ? 0 : tabs.Count;
    }
    set
    {
      value = Math.Max(1, Math.Min(10, value));
      List<TabItem> tabItemList = new List<TabItem>((IEnumerable<TabItem>) this.tabs);
      this.tabs = new List<TabItem>();
      for (int index = 0; index < value; ++index)
      {
        if (index < tabItemList.Count)
          this.tabs.Add(tabItemList[index]);
        else
          this.tabs.Add(new TabItem($"Tab {index + 1}"));
      }
      if (this.selectedIndex >= this.tabs.Count)
        this.selectedIndex = Math.Max(0, this.tabs.Count - 1);
      this.InitializeAnimations();
      this.animationX = this.targetX = (float) (this.selectedIndex * this.GetSegmentSize());
      this.Invalidate();
    }
  }

  [Category("Behavior")]
  [Description("Orientação das abas (Horizontal ou Vertical)")]
  [DefaultValue(TabOrientation.Horizontal)]
  public TabOrientation TabOrientation
  {
    get => this.tabOrientation;
    set
    {
      if (this.tabOrientation == value)
        return;
      this.tabOrientation = value;
      this.Invalidate();
    }
  }

  [Category("Images")]
  [Description("Posição da imagem em relação ao texto")]
  [DefaultValue(ImagePosition.Left)]
  public ImagePosition ImagePosition
  {
    get => this.imagePosition;
    set
    {
      if (this.imagePosition == value)
        return;
      this.imagePosition = value;
      this.Invalidate();
    }
  }

  [Category("Images")]
  [Description("Tamanho das imagens nas abas")]
  public Size ImageSize
  {
    get => this.imageSize;
    set
    {
      this.imageSize = new Size(Math.Max(8, value.Width), Math.Max(8, value.Height));
      this.Invalidate();
    }
  }

  [Category("Images")]
  [Description("Espaçamento entre imagem e texto")]
  [DefaultValue(4)]
  public int ImagePadding
  {
    get => this.imagePadding;
    set
    {
      this.imagePadding = Math.Max(0, value);
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor do slide da aba ativa")]
  [DefaultValue(typeof (Color), "DeepSkyBlue")]
  public Color SlideColor { get; set; } = Color.DeepSkyBlue;

  [Category("Appearance")]
  [Description("Cor do texto da aba ativa")]
  [DefaultValue(typeof (Color), "White")]
  public Color TextColor { get; set; } = Color.White;

  [Category("Appearance")]
  [Description("Cor do texto das abas inativas")]
  [DefaultValue(typeof (Color), "LightGray")]
  public Color TextColorInactive { get; set; } = Color.LightGray;

  [Category("Appearance")]
  [Description("Cor da imagem da aba ativa")]
  [DefaultValue(typeof (Color), "White")]
  public Color ImageColorActive { get; set; } = Color.White;

  [Category("Appearance")]
  [Description("Cor da imagem das abas inativas")]
  [DefaultValue(typeof (Color), "LightGray")]
  public Color ImageColorInactive { get; set; } = Color.LightGray;

  [Category("Appearance")]
  [Description("Cor da borda do controle")]
  [DefaultValue(typeof (Color), "Gray")]
  public Color BorderColor { get; set; } = Color.Gray;

  [Category("Appearance")]
  [Description("Espessura da borda")]
  [DefaultValue(1)]
  public int BorderThickness
  {
    get => this.borderThickness;
    set
    {
      this.borderThickness = Math.Max(1, value);
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Fonte das abas")]
  public Font TabsFont
  {
    get => this.tabsFont ?? this.Font;
    set
    {
      this.tabsFont = value;
      this.Invalidate();
    }
  }

  [Category("Animation")]
  [Description("Velocidade da animação de transição")]
  [DefaultValue(0.25f)]
  public float AnimationSpeed
  {
    get => this.animationSpeed;
    set => this.animationSpeed = Math.Max(0.1f, Math.Min(1f, value));
  }

  [Category("Animation")]
  [Description("Velocidade da transição de cor do texto")]
  [DefaultValue(0.3f)]
  public float ColorAnimationSpeed
  {
    get => this.colorAnimationSpeed;
    set => this.colorAnimationSpeed = Math.Max(0.1f, Math.Min(1f, value));
  }

  [Category("Animation")]
  [Description("Habilita animações")]
  [DefaultValue(true)]
  public bool EnableAnimations
  {
    get => this.enableAnimations;
    set
    {
      this.enableAnimations = value;
      if (value)
        return;
      this.animTimer.Stop();
      this.animationX = this.targetX;
      Array.Copy((Array) this.targetAlphaValues, (Array) this.textAlphaValues, this.targetAlphaValues.Length);
      Array.Copy((Array) this.targetImageAlphaValues, (Array) this.imageAlphaValues, this.targetImageAlphaValues.Length);
      this.Invalidate();
    }
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 1")]
  public Image Tab1Image
  {
    get => this.GetTabImage(0);
    set => this.SetTabImageProperty(0, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 2")]
  public Image Tab2Image
  {
    get => this.GetTabImage(1);
    set => this.SetTabImageProperty(1, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 3")]
  public Image Tab3Image
  {
    get => this.GetTabImage(2);
    set => this.SetTabImageProperty(2, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 4")]
  public Image Tab4Image
  {
    get => this.GetTabImage(3);
    set => this.SetTabImageProperty(3, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 5")]
  public Image Tab5Image
  {
    get => this.GetTabImage(4);
    set => this.SetTabImageProperty(4, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 6")]
  public Image Tab6Image
  {
    get => this.GetTabImage(5);
    set => this.SetTabImageProperty(5, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 7")]
  public Image Tab7Image
  {
    get => this.GetTabImage(6);
    set => this.SetTabImageProperty(6, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 8")]
  public Image Tab8Image
  {
    get => this.GetTabImage(7);
    set => this.SetTabImageProperty(7, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 9")]
  public Image Tab9Image
  {
    get => this.GetTabImage(8);
    set => this.SetTabImageProperty(8, value);
  }

  [Category("Tab Images")]
  [Description("Imagem da Tab 10")]
  public Image Tab10Image
  {
    get => this.GetTabImage(9);
    set => this.SetTabImageProperty(9, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 1")]
  public string Tab1Text
  {
    get => this.GetTabText(0);
    set => this.SetTabTextProperty(0, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 2")]
  public string Tab2Text
  {
    get => this.GetTabText(1);
    set => this.SetTabTextProperty(1, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 3")]
  public string Tab3Text
  {
    get => this.GetTabText(2);
    set => this.SetTabTextProperty(2, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 4")]
  public string Tab4Text
  {
    get => this.GetTabText(3);
    set => this.SetTabTextProperty(3, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 5")]
  public string Tab5Text
  {
    get => this.GetTabText(4);
    set => this.SetTabTextProperty(4, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 6")]
  public string Tab6Text
  {
    get => this.GetTabText(5);
    set => this.SetTabTextProperty(5, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 7")]
  public string Tab7Text
  {
    get => this.GetTabText(6);
    set => this.SetTabTextProperty(6, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 8")]
  public string Tab8Text
  {
    get => this.GetTabText(7);
    set => this.SetTabTextProperty(7, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 9")]
  public string Tab9Text
  {
    get => this.GetTabText(8);
    set => this.SetTabTextProperty(8, value);
  }

  [Category("Tab Texts")]
  [Description("Texto da Tab 10")]
  public string Tab10Text
  {
    get => this.GetTabText(9);
    set => this.SetTabTextProperty(9, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 1")]
  public int Tab1TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(0);
    set => this.SetTabTopLeftRadius(0, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 1")]
  public int Tab1TopRightRadius
  {
    get => this.GetTabTopRightRadius(0);
    set => this.SetTabTopRightRadius(0, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 1")]
  public int Tab1BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(0);
    set => this.SetTabBottomLeftRadius(0, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 1")]
  public int Tab1BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(0);
    set => this.SetTabBottomRightRadius(0, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 2")]
  public int Tab2TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(1);
    set => this.SetTabTopLeftRadius(1, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 2")]
  public int Tab2TopRightRadius
  {
    get => this.GetTabTopRightRadius(1);
    set => this.SetTabTopRightRadius(1, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 2")]
  public int Tab2BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(1);
    set => this.SetTabBottomLeftRadius(1, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 2")]
  public int Tab2BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(1);
    set => this.SetTabBottomRightRadius(1, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 3")]
  public int Tab3TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(2);
    set => this.SetTabTopLeftRadius(2, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 3")]
  public int Tab3TopRightRadius
  {
    get => this.GetTabTopRightRadius(2);
    set => this.SetTabTopRightRadius(2, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 3")]
  public int Tab3BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(2);
    set => this.SetTabBottomLeftRadius(2, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 3")]
  public int Tab3BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(2);
    set => this.SetTabBottomRightRadius(2, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 4")]
  public int Tab4TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(3);
    set => this.SetTabTopLeftRadius(3, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 4")]
  public int Tab4TopRightRadius
  {
    get => this.GetTabTopRightRadius(3);
    set => this.SetTabTopRightRadius(3, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 4")]
  public int Tab4BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(3);
    set => this.SetTabBottomLeftRadius(3, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 4")]
  public int Tab4BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(3);
    set => this.SetTabBottomRightRadius(3, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 5")]
  public int Tab5TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(4);
    set => this.SetTabTopLeftRadius(4, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 5")]
  public int Tab5TopRightRadius
  {
    get => this.GetTabTopRightRadius(4);
    set => this.SetTabTopRightRadius(4, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 5")]
  public int Tab5BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(4);
    set => this.SetTabBottomLeftRadius(4, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 5")]
  public int Tab5BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(4);
    set => this.SetTabBottomRightRadius(4, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 6")]
  public int Tab6TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(5);
    set => this.SetTabTopLeftRadius(5, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 6")]
  public int Tab6TopRightRadius
  {
    get => this.GetTabTopRightRadius(5);
    set => this.SetTabTopRightRadius(5, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 6")]
  public int Tab6BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(5);
    set => this.SetTabBottomLeftRadius(5, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 6")]
  public int Tab6BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(5);
    set => this.SetTabBottomRightRadius(5, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 7")]
  public int Tab7TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(6);
    set => this.SetTabTopLeftRadius(6, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 7")]
  public int Tab7TopRightRadius
  {
    get => this.GetTabTopRightRadius(6);
    set => this.SetTabTopRightRadius(6, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 7")]
  public int Tab7BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(6);
    set => this.SetTabBottomLeftRadius(6, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 7")]
  public int Tab7BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(6);
    set => this.SetTabBottomRightRadius(6, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 8")]
  public int Tab8TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(7);
    set => this.SetTabTopLeftRadius(7, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 8")]
  public int Tab8TopRightRadius
  {
    get => this.GetTabTopRightRadius(7);
    set => this.SetTabTopRightRadius(7, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 8")]
  public int Tab8BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(7);
    set => this.SetTabBottomLeftRadius(7, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 8")]
  public int Tab8BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(7);
    set => this.SetTabBottomRightRadius(7, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 9")]
  public int Tab9TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(8);
    set => this.SetTabTopLeftRadius(8, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 9")]
  public int Tab9TopRightRadius
  {
    get => this.GetTabTopRightRadius(8);
    set => this.SetTabTopRightRadius(8, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 9")]
  public int Tab9BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(8);
    set => this.SetTabBottomLeftRadius(8, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 9")]
  public int Tab9BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(8);
    set => this.SetTabBottomRightRadius(8, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior esquerdo da Tab 10")]
  public int Tab10TopLeftRadius
  {
    get => this.GetTabTopLeftRadius(9);
    set => this.SetTabTopLeftRadius(9, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto superior direito da Tab 10")]
  public int Tab10TopRightRadius
  {
    get => this.GetTabTopRightRadius(9);
    set => this.SetTabTopRightRadius(9, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior esquerdo da Tab 10")]
  public int Tab10BottomLeftRadius
  {
    get => this.GetTabBottomLeftRadius(9);
    set => this.SetTabBottomLeftRadius(9, value);
  }

  [Category("Tab Radii")]
  [Description("Raio do canto inferior direito da Tab 10")]
  public int Tab10BottomRightRadius
  {
    get => this.GetTabBottomRightRadius(9);
    set => this.SetTabBottomRightRadius(9, value);
  }

  public int SelectedIndex
  {
    get => this.selectedIndex;
    set
    {
      if (value == this.selectedIndex || value < 0 || value >= this.tabs.Count)
        return;
      this.selectedIndex = value;
      this.targetX = (float) (value * this.GetSegmentSize());
      this.UpdateTargetAlphaValues();
      this.animTimer.Start();
      this.Invalidate();
      EventHandler selectedIndexChanged = this.SelectedIndexChanged;
      if (selectedIndexChanged == null)
        return;
      selectedIndexChanged((object) this, EventArgs.Empty);
    }
  }

  private int GetTabTopLeftRadius(int index)
  {
    return index < 0 || index >= this.tabs.Count ? 6 : this.tabs[index].TopLeftRadius;
  }

  private void SetTabTopLeftRadius(int index, int value)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].TopLeftRadius = Math.Max(0, value);
    this.Invalidate();
  }

  private int GetTabTopRightRadius(int index)
  {
    return index < 0 || index >= this.tabs.Count ? 6 : this.tabs[index].TopRightRadius;
  }

  private void SetTabTopRightRadius(int index, int value)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].TopRightRadius = Math.Max(0, value);
    this.Invalidate();
  }

  private int GetTabBottomLeftRadius(int index)
  {
    return index < 0 || index >= this.tabs.Count ? 6 : this.tabs[index].BottomLeftRadius;
  }

  private void SetTabBottomLeftRadius(int index, int value)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].BottomLeftRadius = Math.Max(0, value);
    this.Invalidate();
  }

  private int GetTabBottomRightRadius(int index)
  {
    return index < 0 || index >= this.tabs.Count ? 6 : this.tabs[index].BottomRightRadius;
  }

  private void SetTabBottomRightRadius(int index, int value)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].BottomRightRadius = Math.Max(0, value);
    this.Invalidate();
  }

  public void SetTabRadius(int index, int topLeft, int topRight, int bottomLeft, int bottomRight)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].TopLeftRadius = Math.Max(0, topLeft);
    this.tabs[index].TopRightRadius = Math.Max(0, topRight);
    this.tabs[index].BottomLeftRadius = Math.Max(0, bottomLeft);
    this.tabs[index].BottomRightRadius = Math.Max(0, bottomRight);
    this.Invalidate();
  }

  private Image GetTabImage(int index)
  {
    return index < 0 || index >= this.tabs.Count ? (Image) null : this.tabs[index].Image;
  }

  private void SetTabImageProperty(int index, Image image)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].Image = image;
    this.Invalidate();
  }

  private string GetTabText(int index)
  {
    return index < 0 || index >= this.tabs.Count ? "" : this.tabs[index].Text;
  }

  private void SetTabTextProperty(int index, string text)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].Text = text ?? "";
    this.Invalidate();
  }

  public void AddTab(string text, Image image = null)
  {
    this.tabs.Add(new TabItem(text, image));
    this.InitializeAnimations();
    this.Invalidate();
  }

  public void RemoveTab(int index)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs.RemoveAt(index);
    if (this.selectedIndex >= this.tabs.Count)
      this.selectedIndex = Math.Max(0, this.tabs.Count - 1);
    this.InitializeAnimations();
    this.Invalidate();
  }

  public void SetTabImage(int index, Image image)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].Image = image;
    this.Invalidate();
  }

  public void SetTabText(int index, string text)
  {
    if (index < 0 || index >= this.tabs.Count)
      return;
    this.tabs[index].Text = text;
    this.Invalidate();
  }

  private int GetSegmentSize()
  {
    return this.tabOrientation != TabOrientation.Horizontal ? this.Height / this.tabs.Count : this.Width / this.tabs.Count;
  }

  private void InitializeAnimations()
  {
    int count = this.tabs.Count;
    this.textAlphaValues = new float[count];
    this.targetAlphaValues = new float[count];
    this.imageAlphaValues = new float[count];
    this.targetImageAlphaValues = new float[count];
    for (int index = 0; index < count; ++index)
    {
      this.textAlphaValues[index] = index == this.selectedIndex ? 1f : 0.4f;
      this.targetAlphaValues[index] = this.textAlphaValues[index];
      this.imageAlphaValues[index] = index == this.selectedIndex ? 1f : 0.4f;
      this.targetImageAlphaValues[index] = this.imageAlphaValues[index];
    }
  }

  private void UpdateTargetAlphaValues()
  {
    for (int index = 0; index < this.tabs.Count; ++index)
    {
      this.targetAlphaValues[index] = index == this.selectedIndex ? 1f : 0.4f;
      this.targetImageAlphaValues[index] = index == this.selectedIndex ? 1f : 0.4f;
    }
  }

  private void InitializeImageColorMatrices()
  {
    this.activeImageColorMatrix = new ColorMatrix();
    this.inactiveImageColorMatrix = new ColorMatrix(new float[5][]
    {
      new float[5]{ 0.7f, 0.0f, 0.0f, 0.0f, 0.0f },
      new float[5]{ 0.0f, 0.7f, 0.0f, 0.0f, 0.0f },
      new float[5]{ 0.0f, 0.0f, 0.7f, 0.0f, 0.0f },
      new float[5]{ 0.0f, 0.0f, 0.0f, 1f, 0.0f },
      new float[5]{ 0.0f, 0.0f, 0.0f, 0.0f, 1f }
    });
  }

  private void AnimTimer_Tick(object sender, EventArgs e)
  {
    if (!this.EnableAnimations)
    {
      this.animTimer.Stop();
    }
    else
    {
      bool flag = false;
      float num1 = (this.targetX - this.animationX) * this.AnimationSpeed;
      if ((double) Math.Abs(num1) > 0.10000000149011612)
      {
        this.animationX += num1;
        flag = true;
      }
      else if ((double) Math.Abs(this.animationX - this.targetX) > 0.0099999997764825821)
      {
        this.animationX = this.targetX;
        flag = true;
      }
      for (int index = 0; index < this.textAlphaValues.Length; ++index)
      {
        float num2 = (this.targetAlphaValues[index] - this.textAlphaValues[index]) * this.ColorAnimationSpeed;
        if ((double) Math.Abs(num2) > 0.0099999997764825821)
        {
          this.textAlphaValues[index] += num2;
          flag = true;
        }
        float num3 = (this.targetImageAlphaValues[index] - this.imageAlphaValues[index]) * this.ColorAnimationSpeed;
        if ((double) Math.Abs(num3) > 0.0099999997764825821)
        {
          this.imageAlphaValues[index] += num3;
          flag = true;
        }
      }
      if (!flag)
        this.animTimer.Stop();
      else
        this.Invalidate();
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);
    Graphics graphics = e.Graphics;
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    graphics.Clear(this.BackColor);
    if (this.tabs.Count == 0)
      return;
    int segmentSize = this.GetSegmentSize();
    int width = this.tabOrientation == TabOrientation.Horizontal ? segmentSize : this.Width;
    int height = this.tabOrientation == TabOrientation.Horizontal ? this.Height : segmentSize;
    RectangleF rectangleF = this.tabOrientation == TabOrientation.Horizontal ? new RectangleF(this.animationX, 0.0f, (float) width, (float) height) : new RectangleF(0.0f, this.animationX, (float) width, (float) height);
    int topLeft = this.selectedIndex < 0 || this.selectedIndex >= this.tabs.Count ? 6 : this.tabs[this.selectedIndex].TopLeftRadius;
    int topRight = this.selectedIndex < 0 || this.selectedIndex >= this.tabs.Count ? 6 : this.tabs[this.selectedIndex].TopRightRadius;
    int bottomLeft = this.selectedIndex < 0 || this.selectedIndex >= this.tabs.Count ? 6 : this.tabs[this.selectedIndex].BottomLeftRadius;
    int bottomRight = this.selectedIndex < 0 || this.selectedIndex >= this.tabs.Count ? 6 : this.tabs[this.selectedIndex].BottomRightRadius;
    using (GraphicsPath path = this.RoundedRect(rectangleF, (float) topLeft, (float) topRight, (float) bottomLeft, (float) bottomRight))
    {
      using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangleF, this.SlideColor, this.SlideColor, LinearGradientMode.Vertical))
        graphics.FillPath((Brush) linearGradientBrush, path);
    }
    using (GraphicsPath path = this.RoundedRect(new RectangleF(0.0f, 0.0f, (float) (this.Width - 1), (float) (this.Height - 1)), 6f, 6f, 6f, 6f))
    {
      using (Pen pen = new Pen(this.BorderColor, (float) this.BorderThickness))
        graphics.DrawPath(pen, path);
    }
    for (int index = 0; index < this.tabs.Count; ++index)
    {
      Rectangle tabRect = this.tabOrientation == TabOrientation.Horizontal ? new Rectangle(index * width, 0, width, height) : new Rectangle(0, index * height, width, height);
      this.DrawTabContent(graphics, this.tabs[index], tabRect, this.textAlphaValues[index], this.imageAlphaValues[index]);
    }
    using (Pen pen = new Pen(this.BorderColor, 1f))
    {
      for (int index = 1; index < this.tabs.Count; ++index)
      {
        int x1 = this.tabOrientation == TabOrientation.Horizontal ? index * segmentSize : 0;
        int y1 = this.tabOrientation == TabOrientation.Horizontal ? 0 : index * segmentSize;
        int num1 = this.tabOrientation == TabOrientation.Horizontal ? 1 : this.Width;
        int num2 = this.tabOrientation == TabOrientation.Horizontal ? this.Height : 1;
        graphics.DrawLine(pen, x1, y1, x1 + num1, y1 + num2);
      }
    }
  }

  private void DrawTabContent(
    Graphics g,
    TabItem tab,
    Rectangle tabRect,
    float textAlpha,
    float imageAlpha)
  {
    Color color = this.InterpolateColors(this.TextColorInactive, this.TextColor, textAlpha);
    if (tab.Image != null)
      this.DrawTabWithImage(g, tab, tabRect, color, imageAlpha);
    else
      TextRenderer.DrawText((IDeviceContext) g, tab.Text, this.TabsFont, tabRect, color, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
  }

  private void DrawTabWithImage(
    Graphics g,
    TabItem tab,
    Rectangle tabRect,
    Color textColor,
    float imageAlpha)
  {
    Rectangle rect = new Rectangle();
    ref Rectangle local1 = ref rect;
    Size imageSize1 = this.ImageSize;
    int width1 = imageSize1.Width;
    imageSize1 = this.ImageSize;
    int height1 = imageSize1.Height;
    local1 = new Rectangle(0, 0, width1, height1);
    Rectangle bounds = tabRect;
    switch (this.imagePosition)
    {
      case ImagePosition.Left:
        rect.X = tabRect.X + this.ImagePadding;
        ref Rectangle local2 = ref rect;
        int y1 = tabRect.Y;
        int height2 = tabRect.Height;
        Size imageSize2 = this.ImageSize;
        int height3 = imageSize2.Height;
        int num1 = (height2 - height3) / 2;
        int num2 = y1 + num1;
        local2.Y = num2;
        ref Rectangle local3 = ref bounds;
        int x = local3.X;
        imageSize2 = this.ImageSize;
        int num3 = imageSize2.Width + this.ImagePadding * 2;
        local3.X = x + num3;
        ref Rectangle local4 = ref bounds;
        int width2 = local4.Width;
        imageSize2 = this.ImageSize;
        int num4 = imageSize2.Width + this.ImagePadding * 2;
        local4.Width = width2 - num4;
        break;
      case ImagePosition.Right:
        rect.X = tabRect.Right - this.ImageSize.Width - this.ImagePadding;
        ref Rectangle local5 = ref rect;
        int y2 = tabRect.Y;
        int height4 = tabRect.Height;
        Size imageSize3 = this.ImageSize;
        int height5 = imageSize3.Height;
        int num5 = (height4 - height5) / 2;
        int num6 = y2 + num5;
        local5.Y = num6;
        ref Rectangle local6 = ref bounds;
        int width3 = local6.Width;
        imageSize3 = this.ImageSize;
        int num7 = imageSize3.Width + this.ImagePadding * 2;
        local6.Width = width3 - num7;
        break;
      case ImagePosition.Top:
        rect.X = tabRect.X + (tabRect.Width - this.ImageSize.Width) / 2;
        rect.Y = tabRect.Y + this.ImagePadding;
        ref Rectangle local7 = ref bounds;
        int y3 = local7.Y;
        Size imageSize4 = this.ImageSize;
        int num8 = imageSize4.Height + this.ImagePadding * 2;
        local7.Y = y3 + num8;
        ref Rectangle local8 = ref bounds;
        int height6 = local8.Height;
        imageSize4 = this.ImageSize;
        int num9 = imageSize4.Height + this.ImagePadding * 2;
        local8.Height = height6 - num9;
        break;
      case ImagePosition.Bottom:
        rect.X = tabRect.X + (tabRect.Width - this.ImageSize.Width) / 2;
        ref Rectangle local9 = ref rect;
        int bottom = tabRect.Bottom;
        Size imageSize5 = this.ImageSize;
        int height7 = imageSize5.Height;
        int num10 = bottom - height7 - this.ImagePadding;
        local9.Y = num10;
        ref Rectangle local10 = ref bounds;
        int height8 = local10.Height;
        imageSize5 = this.ImageSize;
        int num11 = imageSize5.Height + this.ImagePadding * 2;
        local10.Height = height8 - num11;
        break;
      case ImagePosition.Center:
        rect.X = tabRect.X + (tabRect.Width - this.ImageSize.Width) / 2;
        rect.Y = tabRect.Y + (tabRect.Height - this.ImageSize.Height) / 2;
        bounds = Rectangle.Empty;
        break;
    }
    this.DrawColorizedImage(g, tab.Image, rect, imageAlpha);
    if (this.imagePosition == ImagePosition.Center || string.IsNullOrEmpty(tab.Text))
      return;
    TextRenderer.DrawText((IDeviceContext) g, tab.Text, this.TabsFont, bounds, textColor, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
  }

  private void DrawColorizedImage(Graphics g, Image image, Rectangle rect, float imageAlpha)
  {
    ColorMatrix colorMatrix = this.CreateColorMatrix(this.InterpolateColors(this.ImageColorInactive, this.ImageColorActive, imageAlpha), imageAlpha);
    using (ImageAttributes imageAttr = new ImageAttributes())
    {
      imageAttr.SetColorMatrix(colorMatrix);
      g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
    }
  }

  private ColorMatrix CreateColorMatrix(Color targetColor, float alpha)
  {
    float num1 = (float) targetColor.R / (float) byte.MaxValue;
    float num2 = (float) targetColor.G / (float) byte.MaxValue;
    float num3 = (float) targetColor.B / (float) byte.MaxValue;
    float num4 = alpha;
    return new ColorMatrix(new float[5][]
    {
      new float[5]{ num1 * num4, 0.0f, 0.0f, 0.0f, 0.0f },
      new float[5]{ 0.0f, num2 * num4, 0.0f, 0.0f, 0.0f },
      new float[5]{ 0.0f, 0.0f, num3 * num4, 0.0f, 0.0f },
      new float[5]{ 0.0f, 0.0f, 0.0f, num4, 0.0f },
      new float[5]{ 0.0f, 0.0f, 0.0f, 0.0f, 1f }
    });
  }

  private Color InterpolateColors(Color colorA, Color colorB, float factor)
  {
    factor = Math.Max(0.0f, Math.Min(1f, factor));
    return Color.FromArgb((int) ((double) colorA.A + (double) ((int) colorB.A - (int) colorA.A) * (double) factor), (int) ((double) colorA.R + (double) ((int) colorB.R - (int) colorA.R) * (double) factor), (int) ((double) colorA.G + (double) ((int) colorB.G - (int) colorA.G) * (double) factor), (int) ((double) colorA.B + (double) ((int) colorB.B - (int) colorA.B) * (double) factor));
  }

  protected override void OnMouseClick(MouseEventArgs e)
  {
    base.OnMouseClick(e);
    int num = this.tabOrientation == TabOrientation.Horizontal ? e.X / (this.Width / this.tabs.Count) : e.Y / (this.Height / this.tabs.Count);
    if (num < 0 || num >= this.tabs.Count)
      return;
    this.SelectedIndex = num;
  }

  protected override void OnResize(EventArgs e)
  {
    base.OnResize(e);
    this.targetX = (float) (this.selectedIndex * this.GetSegmentSize());
    this.animationX = this.targetX;
    this.Invalidate();
  }

  private GraphicsPath RoundedRect(
    RectangleF bounds,
    float topLeft,
    float topRight,
    float bottomLeft,
    float bottomRight)
  {
    GraphicsPath graphicsPath = new GraphicsPath();
    float val2_1 = topLeft * 2f;
    float val2_2 = topRight * 2f;
    float val2_3 = bottomLeft * 2f;
    float val2_4 = bottomRight * 2f;
    float num1 = Math.Max(0.0f, val2_1);
    float num2 = Math.Max(0.0f, val2_2);
    float num3 = Math.Max(0.0f, val2_3);
    float num4 = Math.Max(0.0f, val2_4);
    if ((double) num1 > 0.0)
      graphicsPath.AddArc(bounds.Left, bounds.Top, num1, num1, 180f, 90f);
    else
      graphicsPath.AddLine(bounds.Left, bounds.Top, bounds.Left, bounds.Top);
    if ((double) num2 > 0.0)
      graphicsPath.AddArc(bounds.Right - num2, bounds.Top, num2, num2, 270f, 90f);
    else
      graphicsPath.AddLine(bounds.Right, bounds.Top, bounds.Right, bounds.Top);
    if ((double) num4 > 0.0)
      graphicsPath.AddArc(bounds.Right - num4, bounds.Bottom - num4, num4, num4, 0.0f, 90f);
    else
      graphicsPath.AddLine(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom);
    if ((double) num3 > 0.0)
      graphicsPath.AddArc(bounds.Left, bounds.Bottom - num3, num3, num3, 90f, 90f);
    else
      graphicsPath.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Bottom);
    graphicsPath.CloseFigure();
    return graphicsPath;
  }
}
