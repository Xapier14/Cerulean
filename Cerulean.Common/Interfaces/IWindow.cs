namespace Cerulean.Common;

public interface IWindow
{
    public Action<string>? TextUpdatedDelegate { get; set; }
    public static readonly Size DefaultWindowSize = new(600, 400);

    public bool IsInitialized { get; }
    public IntPtr WindowPtr { get; }
    public bool Closed { get; }
    public bool IsFlaggedForRedraw { get; }
    public bool AlwaysRedraw { get; set; }
    public bool AutoScale { get; set; }
    public IGraphics? GraphicsContext { get; }
    public string WindowTitle { get; set; }
    public Size WindowSize { get; set; }
    public Size? MinimumWindowSize { get; set; }
    public Size? MaximumWindowSize { get; set; }
    public (int, int) WindowPosition { get; set; }
    public bool Enabled { get; set; }
    public bool HandleClose { get; set; }
    public IWindow? ParentWindow { get; }
    public dynamic Layout { get; }
    public Color BackColor { get; set; }
    public IntPtr GLContext { get; }
    public Component? HoveredComponent { get; }
    public bool IsModal { get; }

    public void StartTextInput(Component? inputOwner, int x, int y, Size area, string text = "", int index = 0,
        int maxLength = 2048);
    public void StopTextInput(Component? inputOwner = null);

    public void Close();
    public void FlagForRedraw();
    public int GetDpiScaledValue(int value);
    public double GetDpiScaledValue(double value);
    public float GetDpiScaledValue(float value);
    public Size GetDpiScaledValue(Size value);

    public void SetComponentScaledX(Component component, int x);
    public void SetComponentScaledY(Component component, int y);
    public void SetComponentScaledPosition(Component component, int x, int y);
    public void SetComponentScaledSize(ISized component, Size size);
}
