using System;
using System.Collections;

namespace LocalRT.radtor.local.core
{
    public class EncripTOR
    {  
        private string pattern_token;
        private string pattern_hash;
        private string textPlain;
        

        public string encript(string _textPlain)
        {
            if (_textPlain.Length > 0)
            {
                generateToken(_textPlain);

                textPlain = _textPlain.TrimEnd().TrimStart();
                string result = "";
                int index = 0;
                foreach (char character in textPlain)
                {
                    result += encriptRadTORChar(character, textPlain.Length, index);
                    index++;
                }
                return toAsciiNumber(result);
            }
            return "";
        }

        private void generateToken(string textPlain)
        {
            int bof = 32;
            int eof = 126;
            string seq = "";
            int xf = textPlain.Length % (eof - bof);
            char tp = textPlain[0];
            int i = 0;
            int h = 0;
            for (int unicode = bof; unicode <= eof; unicode++) {
                char character = (char)unicode;
                seq += character.ToString();
                if ((i - (eof - bof)) == xf)
                {
                    h = (i - (eof - bof));
                }
                i++;
            }
            pattern_token="";
            pattern_hash = "";
            int x = h;
            int max = eof - bof;
            for (int y = 0; y <= max; y++) {
                pattern_token += seq[x];
                pattern_hash += seq[max - x];
                x++;
                if (x > max) {
                    x = 0;
                }
            }

        }

        private string encriptRadTORChar(char character, int size, int token_index)
        {
            if (pattern_token.IndexOf(character) != -1)
            {
                int indexRadTOR = (pattern_token.IndexOf(character) + size + token_index) % pattern_token.Length;
                return pattern_hash.Substring(indexRadTOR, 1);
            }
            return character.ToString();
        }

        public string decrypt(string textPlain)
        {
            if (textPlain.Length > 0)
            {
                textPlain = toNumberAscii(textPlain.TrimEnd().TrimStart());
                generateToken(textPlain);

                string result = "";
                int index = 0;
                foreach (char character in textPlain)
                {
                    result += dencriptRadTORChar(character, textPlain.Length, index);
                    index++;
                }
                return result;
            }
            return "";
        }

        private string toAsciiNumber(string ascii)
        {
            String numberAscii = "";
            foreach (char c in ascii)
            {
                int unicode = c;
                //numberAscii += String.Format("{0:000}",unicode);

                int myInt = unicode;
                string myHex = myInt.ToString("X");  
                numberAscii += String.Format("{0}", myHex);
            }
            return numberAscii;
        }
        private string toNumberAscii(string number)
        {
            int numberLen= 1;
            int maxLen = 2;
            String asciiHex = "";
            String ascii = "";
            foreach (char c in number) {
                asciiHex += c.ToString();
                if (numberLen == maxLen) {                    
                    int decValue = int.Parse(asciiHex,System.Globalization.NumberStyles.HexNumber); ;
                    ascii += Convert.ToChar(decValue);
                    asciiHex = "";
                    maxLen += 2;
                }
                numberLen++;
            }
            return ascii;
        }

        private string dencriptRadTORChar(char character, int size, int token_index)
        {
            int indexRadTOR = 0;
            if (pattern_hash.IndexOf(character) != -1)
            {
                if ((pattern_hash.IndexOf(character) - size - token_index) > 0)
                {
                    indexRadTOR = (pattern_hash.IndexOf(character) - size - token_index) % pattern_hash.Length;
                }
                else
                {
                    indexRadTOR = (pattern_token.Length) + (pattern_hash.IndexOf(character) - size - token_index) % pattern_hash.Length;
                }
                indexRadTOR = indexRadTOR % pattern_hash.Length;
                return pattern_token.Substring(indexRadTOR, 1);
            }
            else
            {
                return character.ToString();
            }
        }
    }
}
