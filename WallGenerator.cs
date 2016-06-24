using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace Wall_generator {
    public partial class WallGenerator : Form {
        Bitmap svgView;
        List<PointFloat> vertexList = new List<PointFloat>();
        List<Tuple<Int32, Int32>> edgeList = new List<Tuple<Int32, Int32>>();

        public WallGenerator() {
            InitializeComponent();
        }

        private void curveToCubic(float x1, float y1, float x2, float y2, float x, float y) {
            float x0 = vertexList.Last().x;
            float y0 = vertexList.Last().y;
            float distance = (x - x0) * (x - x0) + (y - y0) * (y - y0);

            float step = 15000 / distance;

            for (float t = step; t < 1; t += step) {
                float newX = (1 - t) * (1 - t) * (1 - t) * x0 + 3 * (1 - t) * (1 - t) * t * x1 + 3 * (1 - t) * t * t * x2 + t * t * t * x;
                float newY = (1 - t) * (1 - t) * (1 - t) * y0 + 3 * (1 - t) * (1 - t) * t * y1 + 3 * (1 - t) * t * t * y2 + t * t * t * y;
                lineTo(newX, newY);
            }
            lineTo(x, y);
        }

        private void curveToQuad(float x1, float y1, float x, float y) {
            float x0 = vertexList.Last().x;
            float y0 = vertexList.Last().y;
            float distance = (x - x0) * (x - x0) + (y - y0) * (y - y0);

            float step = 15000 / distance;

            for (float t = step; t < 1; t += step) {
                float newX = (1 - t) * (1 - t) * x0 + (1 - t) * t * x1 + t * t * x;
                float newY = (1 - t) * (1 - t) * y0 + (1 - t) * t * y1 + t * t * y;
                lineTo(newX, newY);
            }
            lineTo(x, y);
        }

        private void lineTo(float x, float y) {
            int fromIndex = vertexList.Count - 1;
            int toIndex = vertexList.Count;
            bool duplicate = false;
            for (int i = 0; i < vertexList.Count; i++) {
                if (vertexList[i].x == x && vertexList[i].y == y) {
                    duplicate = true;
                    toIndex = i;
                    break;
                }
            }
            if (!duplicate) {
                vertexList.Add(new PointFloat(x, y));
            }
            edgeList.Add(new Tuple<int, int>(fromIndex, toIndex));
        }

        private void addLineToLastTwo() {
            edgeList.Add(new Tuple<Int32, Int32>(vertexList.Count - 2, vertexList.Count - 1));
        }

        private string readNextToken(ref string str, String delimiter) {
            Match match = Regex.Match(str, delimiter);
            int delimiterIndex = Regex.Match(str, delimiter).Index;
            if (!match.Success) {
                delimiterIndex = str.Length;
            }
            String result = str.Substring(0, delimiterIndex);
            str = str.Substring(delimiterIndex);
            //System.Diagnostics.Debug.WriteLine(str + " - " + delimiterIndex);
            return result;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Windows.Forms.Application.Exit();
        }

        private void openSVGToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    XDocument doc = XDocument.Load(openFileDialog1.FileName);
                    var svgElement = doc.Elements().Where(s => s.Name.LocalName == "svg").FirstOrDefault();
                    int width = (int)float.Parse(Regex.Replace(svgElement.Attribute("width").Value, "[^0-9\\-\\.]", ""));
                    int height = (int)float.Parse(Regex.Replace(svgElement.Attribute("height").Value, "[^0-9\\-\\.]", ""));
                    System.Diagnostics.Debug.WriteLine(width + "x" + height);
                    float translateX = 0, translateY = 0, scaleX = 1, scaleY = 1;
                    svgView = new Bitmap(width, height);
                    pictureBox1.Image = svgView;
                    Graphics g = Graphics.FromImage(svgView);
                    var paths = svgElement.Descendants().Where(s => s.Name.LocalName == "path");
                    Pen pen = new Pen(Color.Black);
                    vertexList.Clear();
                    edgeList.Clear();
                    foreach (var path in paths) {
                        if (path.Parent.Name.LocalName == "g") {
                            System.Diagnostics.Debug.WriteLine(path.Parent.Name);
                            string transform = path.Parent.Attribute("transform").Value;
                            Match translateMatch = Regex.Match(transform, "translate[^\\)]+\\)");
                            if (translateMatch != null) {
                                string translate = translateMatch.ToString().Substring("translate(".Length);
                                translate = translate.Substring(0, translate.Length - 1);
                                string[] args = translate.Split(",".ToCharArray());
                                translateX = float.Parse(args[0].Trim());
                                translateY = float.Parse(args[1].Trim());
                            }
                            Match scaleMatch = Regex.Match(transform, "scale[^\\)]+\\)");
                            if (scaleMatch != null) {
                                string scale = scaleMatch.ToString().Substring("scale(".Length);
                                scale = scale.Substring(0, scale.Length - 1);
                                string[] args = scale.Split(",".ToCharArray());
                                scaleX = float.Parse(args[0].Trim());
                                scaleY = float.Parse(args[1].Trim());
                            }
                        }

                        string d = path.Attribute("d").Value.Replace(',', ' ').Replace("\r", "").Replace("\n", "").Replace("\t", " ");
                        d = Regex.Replace(d, "\\s+", " ");
                        System.Diagnostics.Debug.WriteLine(d);
                        System.Diagnostics.Debug.WriteLine(translateX + ", " + translateY + " - " + scaleX + ", " + scaleY);
                        float cursorX = 0;
                        float cursorY = 0;
                        string token = "";
                        string previousToken = "";
                        int loopStart = vertexList.Count;
                        while (d.Length > 0) {
                            readNextToken(ref d, "\\S");
                            if (Regex.IsMatch(d.Substring(0, 1), "[^0-9\\-\\.]")) {
                                token = d.Substring(0, 1);
                                d = d.Substring(1);
                            }
                            else {
                                token = previousToken;
                            }
                            switch (token) {
                                case "M":
                                    cursorX = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                    readNextToken(ref d, "[^\\s]");
                                    cursorY = float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY + translateY;
                                    vertexList.Add(new PointFloat(cursorX, cursorY));
                                    break;
                                case "m":
                                    cursorX += float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                    readNextToken(ref d, "[^\\s]");
                                    cursorY += float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY;
                                    vertexList.Add(new PointFloat(cursorX, cursorY));
                                    break;
                                case "L":
                                case "l": {
                                        float x = 0, y = 0;
                                        if (token == "L") {
                                            x = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY + translateY;
                                        }
                                        else {
                                            x = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = cursorY + float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY;
                                        }
                                        g.DrawLine(pen, cursorX, cursorY, x, y);
                                        cursorX = x;
                                        cursorY = y;
                                        lineTo(cursorX, cursorY);
                                        break;
                                    }
                                case "H":
                                case "h": {
                                        float x = 0;
                                        if (token == "H") {
                                            x = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                        }
                                        else {
                                            x = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                        }
                                        g.DrawLine(pen, cursorX, cursorY, x, cursorY);
                                        cursorX = x;
                                        lineTo(cursorX, cursorY);
                                        break;
                                    }
                                case "V":
                                case "v": {
                                        float y = 0;
                                        if (token == "H") {
                                            y = float.Parse(readNextToken(ref d, "\\s")) * scaleY + translateY;
                                        }
                                        else {
                                            y = cursorY + float.Parse(readNextToken(ref d, "\\s")) * scaleY;
                                        }
                                        g.DrawLine(pen, cursorX, cursorY, cursorX, y);
                                        cursorY = y;
                                        lineTo(cursorX, cursorY);
                                        break;
                                    }
                                case "C":
                                case "c": {
                                        float x = 0, y = 0, x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                                        if (token == "C") {
                                            x1 = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y1 = float.Parse(readNextToken(ref d, "\\s")) * scaleY + translateY;
                                            readNextToken(ref d, "[^\\s]");
                                            x2 = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y2 = float.Parse(readNextToken(ref d, "\\s")) * scaleY + translateY;
                                            readNextToken(ref d, "[^\\s]");
                                            x = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY + translateY;
                                        }
                                        else {
                                            x1 = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y1 = cursorY + float.Parse(readNextToken(ref d, "\\s")) * scaleY;
                                            readNextToken(ref d, "[^\\s]");
                                            x2 = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y2 = cursorY + float.Parse(readNextToken(ref d, "\\s")) * scaleY;
                                            readNextToken(ref d, "[^\\s]");
                                            x = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = cursorY + float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY;
                                        }
                                        g.DrawBezier(pen, cursorX, cursorY, x1, y1, x2, y2, x, y);
                                        cursorX = x;
                                        cursorY = y;
                                        curveToCubic(x1, y1, x2, y2, x, y);
                                        break;
                                    }
                                case "Q":
                                case "q": {
                                        float x = 0, y = 0, x1 = 0, y1 = 0;
                                        if (token == "Q") {
                                            x1 = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y1 = float.Parse(readNextToken(ref d, "\\s")) * scaleY + translateY;
                                            readNextToken(ref d, "[^\\s]");
                                            x = float.Parse(readNextToken(ref d, "\\s")) * scaleX + translateX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY + translateY;
                                        }
                                        else {
                                            x1 = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y1 = cursorY + float.Parse(readNextToken(ref d, "\\s")) * scaleY;
                                            readNextToken(ref d, "[^\\s]");
                                            x = cursorX + float.Parse(readNextToken(ref d, "\\s")) * scaleX;
                                            readNextToken(ref d, "[^\\s]");
                                            y = cursorY + float.Parse(readNextToken(ref d, "[^0-9\\-\\.]")) * scaleY;
                                        }
                                        cursorX = x;
                                        cursorY = y;
                                        curveToQuad(x1, y1, x, y);
                                        break;
                                    }
                                case "z":
                                case "Z":
                                    lineTo(vertexList[loopStart].x, vertexList[loopStart].y);
                                    loopStart = vertexList.Count;
                                    break;
                            }
                            previousToken = token;
                            //System.Diagnostics.Debug.WriteLine(d);
                        }
                    }
                    pen.Color = Color.Red;
                    foreach (Tuple<Int32, Int32> edge in edgeList) {
                        PointFloat point1 = vertexList[edge.Item1];
                        PointFloat point2 = vertexList[edge.Item2];
                        g.DrawLine(pen, point1.x, point1.y, point2.x, point2.y);
                    }
                    pen.Color = Color.Blue;
                    foreach (PointFloat point in vertexList) {
                        g.FillEllipse(pen.Brush, point.x - 2, point.y - 2, 4, 4);
                    }
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
