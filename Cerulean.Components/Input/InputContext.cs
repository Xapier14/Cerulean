using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class InputGroupEventArgs : EventArgs
    {
        public string Group { get; init; } = string.Empty;
        public IEnumerable<object> Values { get; init; } = Array.Empty<object>();
        public int Length => Values.Count();
    }
    [SkipAutoRefGeneration]
    public class InputContext : Component
    {
        private readonly IDictionary<string, List<object>> _valueMap = new Dictionary<string, List<object>>();

        public event EventHandler<InputGroupEventArgs>? OnRadioGroupUpdate;
        public event EventHandler<InputGroupEventArgs>? OnCheckboxGroupUpdate;
        public event EventHandler<ButtonEventArgs>? OnSubmit;

        private Button? _submitButton;

        [ComponentType("Cerulean.Component.Button")]
        public Button? SubmitButton
        {
            get => _submitButton;
            set
            {
                _submitButton = value;
                if (_submitButton != null)
                {
                    _submitButton.OnRelease += SubmitButtonOnOnRelease;
                }
            }
        }

        public InputContext()
        {
            DisableTopLevelHooks = false;
        }

        private void SubmitButtonOnOnRelease(object sender, ButtonEventArgs e)
        {
            OnSubmit?.Invoke(this, e);
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
            base.Update(window, clientArea);
        }

        public void UpdateRadioGroupValue(string group, object value)
        {
            var key = $"Radio_{group}";
            if (!_valueMap.ContainsKey(key))
                _valueMap.Add(key, new List<object>());
            _valueMap[key].Clear();
            _valueMap[key].Add(value);
            OnRadioGroupUpdate?.Invoke(this, new InputGroupEventArgs
            {
                Group = group,
                Values = new[] { value }
            });
        }

        public void AddCheckboxValue(string group, object value)
        {
            var key = $"Checkbox_{group}";
            if (!_valueMap.ContainsKey(key))
                _valueMap.Add(key, new List<object>());
            _valueMap[key].Add(value);
            OnCheckboxGroupUpdate?.Invoke(this, new InputGroupEventArgs
            {
                Group = group,
                Values = _valueMap[key]
            });
        }

        public void RemoveCheckboxValue(string group, object value)
        {
            var key = $"Checkbox_{group}";
            if (!_valueMap.ContainsKey(key))
                _valueMap.Add(key, new List<object>());
            _valueMap[key].Remove(value);
            OnCheckboxGroupUpdate?.Invoke(this, new InputGroupEventArgs
            {
                Group = group,
                Values = _valueMap[key]
            });
        }

        public void ClearCheckboxGroupValue(string group)
        {
            var key = $"Checkbox_{group}";
            if (_valueMap.ContainsKey(key))
                _valueMap[key].Clear();
            foreach (var child in Children.Where(c => c is CheckBox cBox && cBox.InputGroup == group))
            {
                var checkBox = (CheckBox)child;
                checkBox.Checked = false;
            }
        }

        public object GetValueFromRadioGroup(string group)
        {
            var key = $"Radio_{group}";
            var values = _valueMap[key];
            return values[0];
        }

        public T GetValueFromRadioGroup<T>(string group)
        {
            var key = $"Radio_{group}";
            var values = _valueMap[key];
            return (T)values[0];
        }

        public IEnumerable<object> GetValuesFromCheckboxGroup(string group)
        {
            var key = $"Checkbox_{group}";
            if (!_valueMap.ContainsKey(key))
                return Array.Empty<object>();
            var values = _valueMap[key];
            return new List<object>(values);
        }

        public IEnumerable<T> GetValuesFromCheckboxGroup<T>(string group)
        {
            var key = $"Checkbox_{group}";
            if (!_valueMap.ContainsKey(key))
                return Array.Empty<T>();
            var values = _valueMap[key];
            return new List<T>(values.Cast<T>());
        }
    }
}
