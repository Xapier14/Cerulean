﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    public class CheckBox : Component, ISized
    {
        #region SVG Strings

        private const string SVG_CHECK =
            "svg: <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 406.834 406.834\" style=\"enable-background:new 0 0 406.834 406.834;\" xml:space=\"preserve\"><polygon points=\"385.621,62.507 146.225,301.901 21.213,176.891 0,198.104 146.225,344.327 406.834,83.72 \"/></svg>";
        #endregion

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

        private bool _checked = false;
        public bool Checked
        {
            get => _checked;
            set
            {
                Modified = _checked != value;
                _checked = value;
                GetChild<Image>("Image_Check").Visible = value;
            }
        }

        private string? _text = "Checkbox";
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

        public CheckBox()
        {
            DisableTopLevelHooks = true;
            Button boxHandle = AddChild("Button_BoxHandle", new Button
            {
                X = 5,
                Y = 5,
                Size = new Size(12, 12),
                BorderColor = new Color(0, 0, 0),
                BackColor = new Color(150, 150, 150),
                HighlightColor = new Color(190, 190, 190)
            });
            AddChild("Image_Check", new Image
            {
                X = 5,
                Y = 1,
                Size = new Size(16, 16),
                PictureMode = PictureMode.Fit,
                ImageSource = SVG_CHECK,
                Visible = false
            });
            boxHandle.OnRelease += Button_OnClick;

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
            Checked = !Checked;

            if (Parent is not InputContext inputContext) return;

            if (Checked)
                inputContext.AddCheckboxValue(InputGroup, InputData);
            else
                inputContext.RemoveCheckboxValue(InputGroup, InputData);
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (Modified && window is Window ceruleanWindow)
            {
                Modified = false;
                ceruleanWindow.FlagForRedraw();
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
