using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using XamlMath;

namespace TheMathematician
{
    public class Graph
    {
        public float XMax { get; set; }
        public float YMax { get; set; }
        public float Step { get; set; } // The step size for function points, smaller values will create a smoother graph but this would take longer to render
        public int Zoom { get; set; } // The spacing between points on the graph

        private Panel DrawingSurface;
        private Control MControl; // The parent control to which the graph will be added.

        public string GFunction { get; set; }
        public List<Point> Points { get; }

        public Graph(string function, int xMax, int yMax, float step, int zoom, Size graph_Size, Point graph_Position, AnchorStyles graph_AnchorStyles, DockStyle dock, Control control)
        {
            XMax = xMax;
            YMax = yMax;
            Step = step;
            Zoom = zoom;
            Points = new List<Point>();
            GFunction = function.ToLower();
            MControl = control;

            ParseFunctionAndGeneratePoints(function);

            // The graphing panel. Here where we represent our functions.
            DrawingSurface = new Panel()
            {
                BorderStyle = BorderStyle.FixedSingle,
                Size = graph_Size,
                Anchor = graph_AnchorStyles,
                Dock = dock,
                Location = graph_Position,
            };
            DrawingSurface.Paint += DrawingSurface_Paint;
        }

        public void AddToMainControl()
        {
            MControl.Controls.Add(DrawingSurface); // Add the panel to a parent control 'control'.
        }
        public void UpdateGraph(string function)
        {
            Points.Clear();
            ParseFunctionAndGeneratePoints(GFunction);
            DrawingSurface.Invalidate(); // Refresh the graph to show the new points.
        }

        // Draws The Points on the Graph
        private void DrawingSurface_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; // Set a graphics object

            int xCenter = DrawingSurface.Width / 2;
            int yCenter = DrawingSurface.Height / 2;

            // 'Pen' is used to determine each pixel's color and width.
            using (Pen pen = new Pen(Color.Black))
            {
                g.Clear(DrawingSurface.BackColor);

                Color GridColor = Color.LightGray;

                // Draw a grid for better visualization of the graph
                // Horizontal lines
                for (int i = (int)-DrawingSurface.Height; i <= DrawingSurface.Height; i++)
                {
                    g.DrawLine(new Pen(GridColor), new Point(-DrawingSurface.Width, yCenter + (i * Zoom)), new Point(DrawingSurface.Width, yCenter + (i * Zoom)));
                }
                // Vertical lines
                for (int i = (int)-DrawingSurface.Width; i <= DrawingSurface.Width; i++)
                {
                    g.DrawLine(new Pen(GridColor), new Point(xCenter + (i * Zoom), 0), new Point(xCenter + (i * Zoom), DrawingSurface.Height));
                }

                // X-Axis
                g.DrawLine(new Pen(Color.Black), new Point(0, yCenter), new Point(DrawingSurface.Width, yCenter));
                // Y-Axis
                g.DrawLine(new Pen(Color.Black), new Point(xCenter, 0), new Point(xCenter, DrawingSurface.Height));

                if (Points.Count == 0)
                {
                    // NoFunctions Text
                    string NoFunctionstext = "No functions to Display.";
                    Font NoFunctionsFont = new Font(Program.DefaultFonts[2], 12, FontStyle.Regular);
                    SizeF NoFunctionstextSize = g.MeasureString(NoFunctionstext, NoFunctionsFont);
                    PointF NoFunctionstextPosition = new PointF(
                        (DrawingSurface.Width / 2) - (NoFunctionstextSize.Width / 2),
                        (DrawingSurface.Height / 2) - (NoFunctionstextSize.Height / 2)
                    );
                    // White rectangle
                    RectangleF NoFunctionsRectBackground = new RectangleF(
                        NoFunctionstextPosition.X + NoFunctionstextSize.Width / 2 - (NoFunctionstextSize.Width / 2) - 10,
                        NoFunctionstextPosition.Y + NoFunctionstextSize.Height / 2 - NoFunctionstextSize.Height,
                        NoFunctionstextSize.Width + 20,
                        NoFunctionstextSize.Height + 20
                    );
                    Rectangle NoFunctionsRectBorder = new Rectangle(
                        new Point((int)NoFunctionsRectBackground.X, (int)NoFunctionsRectBackground.Y),
                        new Size((int)NoFunctionsRectBackground.Width, (int)NoFunctionsRectBackground.Height)
                    );

                    // Rectangle Behind the text
                    g.FillRectangle(Brushes.White, NoFunctionsRectBackground); // White Background
                    g.DrawRectangle(new Pen(Color.Black), NoFunctionsRectBorder); // Black Border
                    // Text
                    g.DrawString(NoFunctionstext, NoFunctionsFont, Brushes.Red, NoFunctionstextPosition);
                    return;
                }

                // Draw the axes labels
                Font labelFont = new Font(Program.DefaultFonts[2], 10, FontStyle.Regular);

                // Draw x labels
                for (int i = 1; i <= XMax; i++)
                {
                    // Positive X labels
                    g.DrawString(i.ToString(), labelFont, Brushes.Black, new PointF(xCenter + (i * Zoom) - 10, yCenter + 4));
                    // Negative X labels
                    g.DrawString((-i).ToString(), labelFont, Brushes.Black, new PointF(xCenter - (i * Zoom) - 10, yCenter + 4));
                }
                // Draw y labels
                for (int i = 1; i <= YMax; i++)
                {
                    SizeF size = g.MeasureString(i.ToString(), labelFont);

                    // Positive Y labels
                    g.DrawString(i.ToString(), labelFont, Brushes.Black, new PointF(xCenter - 5 - size.Width, yCenter - (i * Zoom) - 10));
                    // Negative Y labels
                    g.DrawString((-i).ToString(), labelFont, Brushes.Black, new PointF(xCenter - 5 - size.Width, yCenter + (i * Zoom) - 10));
                }

                // Draw each point on the graph.
                foreach (Point point in Points)
                {
                    g.FillRectangle(Brushes.RoyalBlue, new Rectangle(point.X + xCenter, point.Y + yCenter, 2, 2)); // I couldn't find a DrawPixel function :(
                }

                //// function label
                //string functionText = $"f(x)={GFunction}";
                //Font functionTextFont = new Font(Program.DefaultFonts[2], 15, FontStyle.Regular);
                //SizeF functionTextSize = g.MeasureString(functionText, functionTextFont);
                //PointF functionTextPosition = new PointF(5, 5);
                //RectangleF functionTextRectBackground = new RectangleF(
                //    functionTextPosition.X,
                //    functionTextPosition.Y,
                //    functionTextSize.Width + 20,
                //    functionTextSize.Height + 20
                //);
                //Rectangle functionTextRectBorder = new Rectangle(
                //    new Point((int)functionTextRectBackground.X, (int)functionTextRectBackground.Y),
                //    new Size((int)functionTextRectBackground.Width, (int)functionTextRectBackground.Height)
                //);

                //// Rectangle Behind the text
                //g.FillRectangle(Brushes.White, functionTextRectBackground); // White Background
                //g.DrawRectangle(new Pen(Color.Black), functionTextRectBorder); // Black Border
                //g.DrawString(functionText, functionTextFont, Brushes.Black, new PointF(functionTextPosition.X + 10, functionTextPosition.Y + 10));
            }
        }

        // Anaylze the functions in order to generate the points for the graph
        private void ParseFunctionAndGeneratePoints(string function)
        {
            if (function == string.Empty)
                return;

            Function gfunction = new Function($"f(x) = {function}");
            Expression expression = new Expression(string.Empty, gfunction); // Initialize the mXParser, expression string will be set later in the loop.

            // Function should not contain any y variables.
            if (function.Contains("y"))
            {
                MessageBox.Show("The function should only contain the variable 'x'.", $"{Program.Program_Name} - Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (function.Contains("="))
            {
                MessageBox.Show("equality is not allowed.", $"{Program.Program_Name} - Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // It starts from the bottom of the graph and keep going up to the top
            for (float x = -XMax; x <= XMax; x += Step)
            {
                float fx = 0;

                try
                {
                    expression.setExpressionString($"f({x})"); // Set the expression to evaluate the function at x.

                    if (expression.checkSyntax())
                        fx = (float)expression.calculate();
                    else
                    {
                        MessageBox.Show("Unable to visualize the function, please validate your expression.", $"{Program.Program_Name} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error parsing the function: {ex.Message}", "Parsing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // After calculating the y coordinate, check if it is within the bounds of the graph
                if (fx >= -YMax && fx <= YMax)
                    Points.Add(new Point((int)(x * Zoom), -(int)(fx * Zoom)));
            }
        }
    }
}
