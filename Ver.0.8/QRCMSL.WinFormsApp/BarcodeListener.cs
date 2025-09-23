using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace QRCMSL.WinFormsApp
{
    /// <summary>Detecta entrada de lector tipo teclado (ráfagas rápidas + Enter).</summary>
    public class BarcodeListener
    {
        public event Action<string> Scanned;
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly StringBuilder _buffer = new StringBuilder();

        public int MaxMillisBetweenChars { get; set; } = 35;
        public int MinLength { get; set; } = 5;

        public bool ProcessKey(Keys keyCode, char keyChar)
        {
            bool isChar = keyChar >= ' ' && keyChar <= '~';
            if (keyCode == Keys.Enter)
            {
                if (_buffer.Length >= MinLength)
                {
                    var text = _buffer.ToString();
                    _buffer.Clear();
                    Scanned?.Invoke(text);
                    return true;
                }
                _buffer.Clear();
                return false;
            }

            var elapsed = _sw.ElapsedMilliseconds;
            _sw.Restart();

            if (elapsed > MaxMillisBetweenChars)
                _buffer.Clear();

            if (isChar) _buffer.Append(keyChar);
            return false;
        }
    }
}
