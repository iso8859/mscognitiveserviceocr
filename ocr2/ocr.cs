using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocr2
{
    public class BoundingBox
    {
        public string boundingBox;
        public Rectangle rect
        {
            get
            {
                Rectangle result = Rectangle.Empty;
                string[] items = boundingBox.Split(',');
                if (items.Length == 4)
                    result = new Rectangle(Convert.ToInt32(items[0]), Convert.ToInt32(items[1]), Convert.ToInt32(items[2]), Convert.ToInt32(items[3]));
                return result;
            }
        }
    }

    public class Region : BoundingBox
    {
        public List<Line> lines;
    }

    public class Line : BoundingBox
    {
        public List<Word> words;
    }

    public class Word : BoundingBox
    {
        public string text;
        public override string ToString()
        {
            return text;
        }
    }

    public class OCR
    {
        public string language;
        public double textAngle;
        public string orientation;
        public List<Region> regions;
    }

}
