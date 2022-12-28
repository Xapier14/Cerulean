using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    [SkipAutoRefGeneration]
    public class RadioButton : Component, ISized
    {

        #region Boilerplate Props

        private Size? _size;
        public Size? Size
        {
            get => _size;
            set
            {
                Modified = true;
                _size = value;
            }
        }

        private int? _hintW;
        public int? HintW
        {
            get => _hintW;
            set
            {
                Modified = true;
                _hintW = value;
            }
        }

        private int? _hintH;
        public int? HintH
        {
            get => _hintH;
            set
            {
                Modified = true;
                _hintH = value;
            }
        }

        #endregion

        private Color? _foreColor;

        public Color? ForeColor
        {
            get => _foreColor;
            set
            {
                Modified = true;
                _foreColor = value;
                GetChild<Label>("Label_Text").ForeColor = value;
            }
        }

        private Color? _selectedColor = new Color("#444");

        public Color? SelectedColor
        {
            get => _selectedColor;
            set
            {
                Modified = true;
                _selectedColor = value;
                GetChild<Rectangle>("Rectangle_Select").FillColor = value;
            }
        }

        private bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                Modified = _selected != value;
                _selected = value;
                GetChild<Rectangle>("Rectangle_Select").FillOpacity = value ? 1.0 : 0.0;
                if (Modified)
                    Button_OnClick(this, new ButtonEventArgs());
            }
        }

        private string? _text = "Radio Button";
        public string? Text
        {
            get => _text;
            set
            {
                Modified = _text != value;
                _text = value;
                GetChild<Label>("Label_Text").Text = value ?? string.Empty;
            }
        }

        private string _fontName = "Arial";
        public string FontName
        {
            get => _fontName;
            set
            {
                Modified = _fontName != value;
                _fontName = value;
                GetChild<Label>("Label_Text").FontName = value;
            }
        }

        private int _fontSize = 12;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                Modified = _fontSize != value;
                _fontSize = value;
                GetChild<Label>("Label_Text").FontSize = value;
            }
        }

        private string _fontStyle = string.Empty;
        public string FontStyle
        {
            get => _fontStyle;
            set
            {
                Modified = _fontStyle != value;
                _fontStyle = value;
                GetChild<Label>("Label_Text").FontStyle = value;
            }
        }

        private bool _wrapText = true;
        public bool WrapText
        {
            get => _wrapText;
            set
            {
                Modified = _wrapText != value;
                _wrapText = value;
                GetChild<Label>("Label_Text").WrapText = value;
            }
        }

        public string InputData { get; init; } = string.Empty;
        public string InputGroup { get; init; } = string.Empty;
        public RadioButton()
        {
            DisableTopLevelHooks = true;
            
            Button radioHandle = AddChild("Button_RadioHandle", new Button
            {
                X = 5,
                Y = 5,
                Size = new Size(12, 12),
                BorderColor = new Color(0, 0, 0),
                BackColor = new Color(150, 150, 150),
                HighlightColor = new Color(190, 190, 190),
                ActivatedColor = new Color(210, 210, 210)
            });
            AddChild("Rectangle_Select", new Rectangle()
            {
                X = 8,
                Y = 8,
                Size = new Size(6, 6),
                FillColor = SelectedColor,
                FillOpacity = 0.0
            });
            radioHandle.OnRelease += (_, _) =>
            {
                if (Selected)
                    return;

                Selected = true;
            };

            AddChild("Label_Text", new Label
            {
                X = 22,
                Y = 4,
                ForeColor = _foreColor,
                Text = _text
            });
        }

        private void Button_OnClick(object sender, ButtonEventArgs e)
        {
            if (Parent is not InputContext inputContext)
                return;
            
            inputContext.UpdateRadioGroupValue(InputGroup, InputData);

            var otherRadioButtons = inputContext.Children
                .Where(x =>
                    x is RadioButton rb &&
                    rb.InputGroup == InputGroup &&
                    rb != this)
                .Cast<RadioButton>();

            foreach (var radioButton in otherRadioButtons)
            {
                radioButton._selected = false;
                radioButton.GetChild<Rectangle>("Rectangle_Select").FillOpacity = 0;
            }

            Modified = true;
        }

        public override void Init()
        {
            base.Init();

            if (Parent is not InputContext inputContext)
                return;

            if (Selected)
                inputContext.UpdateRadioGroupValue(InputGroup, InputData);
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (window is Window ceruleanWindow)
            {
                GetChild("Button_RadioHandle").X = ceruleanWindow.GetDpiScaledValue(5);
                GetChild("Button_RadioHandle").Y = ceruleanWindow.GetDpiScaledValue(5);
                GetChild<ISized>("Button_RadioHandle").Size = ceruleanWindow.GetDpiScaledValue(new Size(12, 12));
                GetChild("Rectangle_Select").X = ceruleanWindow.GetDpiScaledValue(8);
                GetChild("Rectangle_Select").Y = ceruleanWindow.GetDpiScaledValue(8);
                GetChild<ISized>("Rectangle_Select").Size = ceruleanWindow.GetDpiScaledValue(new Size(6, 6));
                GetChild("Label_Text").X = ceruleanWindow.GetDpiScaledValue(22);
                GetChild("Label_Text").Y = ceruleanWindow.GetDpiScaledValue(4);
                if (Modified)
                {
                    Modified = false;
                    ceruleanWindow.FlagForRedraw();
                }
            }

            base.Update(window, clientArea);

            if (window is not null)
                CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);
            base.Draw(graphics, viewportX, viewportY, viewportSize);
            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}
