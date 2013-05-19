using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Retroverse
{
    //From user bvj
    //http://stackoverflow.com/a/4322461/2089233
    //with mods for use with XNA colors
    public class XmlColor
    {
        private System.Drawing.Color color_ = System.Drawing.Color.Black;

        public XmlColor() { }
        public XmlColor(Microsoft.Xna.Framework.Color c) { color_ = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B); }


        public Microsoft.Xna.Framework.Color ToColor()
        {
            return new Microsoft.Xna.Framework.Color(color_.R, color_.G, color_.B, color_.A);
        }

        public static implicit operator Microsoft.Xna.Framework.Color(XmlColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlColor(Microsoft.Xna.Framework.Color c)
        {
            return new XmlColor(c);
        }

        [XmlAttribute]
        public string Web
        {
            get { return System.Drawing.ColorTranslator.ToHtml(color_); }
            set
            {
                try
                {
                    if (Alpha == 0xFF) // preserve named color value if possible
                        color_ = System.Drawing.ColorTranslator.FromHtml(value);
                    else
                        color_ = System.Drawing.Color.FromArgb(Alpha, System.Drawing.ColorTranslator.FromHtml(value));
                }
                catch (Exception)
                {
                    color_ = System.Drawing.Color.Black;
                }
            }
        }

        [XmlAttribute]
        public byte Alpha
        {
            get { return color_.A; }
            set
            {
                if (value != color_.A) // avoid hammering named color if no alpha change
                    color_ = System.Drawing.Color.FromArgb(value, color_);
            }
        }

        public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
    }
}
