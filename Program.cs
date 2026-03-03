using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheMathematician.Properties;

namespace TheMathematician
{
    static class Program
    {
        private static int EXIT_CODE_SUCCESS = 0;
        private static int EXIT_CODE_FAILURE = 1;

        public static List<FontFamily> DefaultFonts = new List<FontFamily>();

        public static string Program_Name = Application.ProductName;
        public static string Program_Version = Application.ProductVersion;
        public static string Program_DeveloperName = Application.CompanyName;

        [STAThread]
        static void Main()
        {
            // Initialize Default Fonts
            try
            {
                DefaultFonts.Add(new FontFamily("Times New Roman"));
                DefaultFonts.Add(new FontFamily("Cambria Math"));
                DefaultFonts.Add(new FontFamily("Arial"));
                DefaultFonts.Add(new FontFamily("Segoe UI"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing default fonts: " + ex.Message, $"{Program_Name} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(EXIT_CODE_FAILURE);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Open The Main Form
            Application.Run(new MainForm());

            Environment.Exit(EXIT_CODE_SUCCESS);
        }

        // Program Forms
        private class MainForm : Form
        {
            // Components
            #region panel_TitleBar
            Panel panel_titleBar;
            Panel panel_Functions;
            Label panel_titleBar_Text1;
            Button panel_titleBar_AboutButton;
            #endregion
            #region panel_Functions
            Label panel_Functions_Text1;
            Label panel_Functions_SubText;
            TextBox panel_Functions_input;
            Button panel_Functions_PlugButton;
            Button panel_Functions_UpdateLimitsButton;
            NumericUpDown panel_Functions_xLimitCounter;
            Label panel_Functions_xLimitLabel;
            NumericUpDown panel_Functions_yLimitCounter;
            Label panel_Functions_yLimitLabel;
            TextBox panel_Functions_StepLimitTextBox;
            Label panel_Functions_StepLimitLabel;
            NumericUpDown panel_Functions_ZoomLimitCounter;
            Label panel_Functions_ZoomLimitLabel;
            Label panel_Functions_DisclaimerText;
            #endregion
            Graph graph;

            public MainForm()
            {
                // Set the form properties
                Width = 1000;
                Height = 700;
                MaximizeBox = false;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                WindowState = FormWindowState.Normal;
                StartPosition = FormStartPosition.CenterScreen;
                Icon = Resources.LogoIcon;
                BackColor = Color.White;
                ForeColor = Color.Black;
                Font = new Font(DefaultFonts[0], 12);

                Text = $"{Program_Name} - {Program_DeveloperName}"; // Set the form title.

                #region Panel_TitleBar
                panel_titleBar = new Panel
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Top,
                    Size = new Size(Width - 16, 35),
                    Location = new Point(0, 0),
                };
                panel_titleBar_Text1 = new Label
                {
                    Text = $"{Program_Name} Program Version {Program_Version} - {Program_DeveloperName}.",
                    Font = new Font(Font.Name, 15, FontStyle.Regular),

                    Anchor = AnchorStyles.Left,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(3, 5),
                };
                panel_titleBar_AboutButton = new Button
                {
                    Text = "About",
                    Font = new Font(Font.Name, 15, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Size = new Size(69, panel_titleBar.Height),
                    Location = new Point(panel_titleBar.Width - 70, 0),
                    Cursor = Cursors.Hand,
                };
                panel_titleBar_AboutButton.Click += (s, e) =>
                {
                    AboutForm aboutForm = new AboutForm();
                    aboutForm.ShowDialog();
                };

                panel_titleBar.Controls.Add(panel_titleBar_AboutButton);
                panel_titleBar.Controls.Add(panel_titleBar_Text1);
                #endregion

                #region Panel_Functions
                panel_Functions = new Panel()
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Left,
                    Size = new Size(300, 100),
                    Location = new Point(0, 0),
                };
                panel_Functions_Text1 = new Label()
                {
                    Text = "Function f(x)=",
                    Font = new Font(Font.Name, 15, FontStyle.Italic),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(5, 10),
                };
                panel_Functions_input = new TextBox()
                {
                    Font = new Font(Font.Name, 15, FontStyle.Italic),
                    BorderStyle = BorderStyle.FixedSingle,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Width = 230,
                    Location = new Point(8, 34),
                };
                panel_Functions_PlugButton = new Button()
                {
                    Text = "Plug",
                    FlatStyle = FlatStyle.Flat,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Size = new Size(46, panel_Functions_input.Height),
                    Location = new Point(panel_Functions_input.Width + 13, 34),
                    Cursor = Cursors.Hand,
                };
                panel_Functions_PlugButton.Click += (s, e) =>
                {
                    if (panel_Functions_input.Text == string.Empty)
                    {
                        MessageBox.Show("You cannot plug an empty function.", $"{Program_Name} - Stop", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Cursor.Current = Cursors.WaitCursor;

                    graph.GFunction = panel_Functions_input.Text.ToLower();

                    panel_Functions_SubText.Invalidate();

                    if (float.Parse(panel_Functions_StepLimitTextBox.Text) >= 1 || float.Parse(panel_Functions_StepLimitTextBox.Text) <= 0)
                        panel_Functions_StepLimitTextBox.Text = "0.004";

                    graph.Zoom = (int)panel_Functions_ZoomLimitCounter.Value;
                    graph.Step = float.Parse(panel_Functions_StepLimitTextBox.Text);
                    graph.YMax = (float)panel_Functions_yLimitCounter.Value;
                    graph.XMax = (float)panel_Functions_xLimitCounter.Value;

                    graph.UpdateGraph(panel_Functions_input.Text);
                    Cursor.Current = Cursors.Default;
                };
                panel_Functions_SubText = new Label()
                {
                    Text = "Graph Options:",
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(5, 65),
                };
                panel_Functions_xLimitCounter = new NumericUpDown()
                {
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    BorderStyle = BorderStyle.FixedSingle,
                    Maximum = 100,
                    Minimum = 10,
                    Width = 50,
                    Value = 10,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Location = new Point(60, 87)
                };
                panel_Functions_xLimitLabel = new Label()
                {
                    Text = "X Limit:",
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(5, 90)
                };
                panel_Functions_yLimitCounter = new NumericUpDown()
                {
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    BorderStyle = BorderStyle.FixedSingle,
                    Maximum = 100,
                    Minimum = 10,
                    Value = 10,
                    Width = 50,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Location = new Point(60, 118)
                };
                panel_Functions_yLimitLabel = new Label()
                {
                    Text = "Y Limit:",
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(5, 122)
                };
                panel_Functions_StepLimitTextBox = new TextBox()
                {
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = "0.004",
                    Width = 60,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Location = new Point(152, 87)
                };
                panel_Functions_StepLimitLabel = new Label()
                {
                    Text = "Step:",
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(112, 90)
                };
                panel_Functions_ZoomLimitCounter = new NumericUpDown()
                {
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    BorderStyle = BorderStyle.FixedSingle,
                    Maximum = 50,
                    Minimum = 10,
                    Value = 32,
                    Width = 50,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Location = new Point(162, 118)
                };
                panel_Functions_ZoomLimitLabel = new Label()
                {
                    Text = "Zoom:",
                    Font = new Font(Font.Name, 12, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Location = new Point(112, 122)
                };
                panel_Functions_UpdateLimitsButton = new Button()
                {
                    Text = "Update",
                    Font = new Font(Font.Name, 9, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(62, 27),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(227, 87),
                    Cursor = Cursors.Hand,
                };
                panel_Functions_UpdateLimitsButton.Click += (s, e) =>
                {
                    Cursor.Current = Cursors.WaitCursor;

                    graph.Zoom = (int)panel_Functions_ZoomLimitCounter.Value;
                    graph.Step = float.Parse(panel_Functions_StepLimitTextBox.Text);
                    graph.YMax = (float)panel_Functions_yLimitCounter.Value;
                    graph.XMax = (float)panel_Functions_xLimitCounter.Value;
                    graph.UpdateGraph(panel_Functions_input.Text);

                    Cursor.Current = Cursors.Default;
                };
                panel_Functions_DisclaimerText = new Label()
                {
                    Text = "Note: It is recommended to set the graph's step value to 0.004;\ndecreasing the value may cause the graph to take more time\nto render the function.",
                    Font = new Font(Font.Name, 8, FontStyle.Regular),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.TopLeft,
                    AutoSize = true,
                    Location = new Point(1, 153)
                };
                PictureBox panel_Functions_line = new PictureBox
                {
                    BackColor = Color.Black,
                    Size = new Size(panel_Functions.Width, 1),
                    Location = new Point(0, 200),
                };

                panel_Functions.Controls.Add(panel_Functions_DisclaimerText);
                panel_Functions.Controls.Add(panel_Functions_UpdateLimitsButton);
                panel_Functions.Controls.Add(panel_Functions_ZoomLimitCounter);
                panel_Functions.Controls.Add(panel_Functions_ZoomLimitLabel);
                panel_Functions.Controls.Add(panel_Functions_StepLimitLabel);
                panel_Functions.Controls.Add(panel_Functions_StepLimitTextBox);
                panel_Functions.Controls.Add(panel_Functions_xLimitCounter);
                panel_Functions.Controls.Add(panel_Functions_xLimitLabel);
                panel_Functions.Controls.Add(panel_Functions_yLimitCounter);
                panel_Functions.Controls.Add(panel_Functions_yLimitLabel);
                panel_Functions.Controls.Add(panel_Functions_line);
                panel_Functions.Controls.Add(panel_Functions_PlugButton);
                panel_Functions.Controls.Add(panel_Functions_input);
                panel_Functions.Controls.Add(panel_Functions_SubText);
                panel_Functions.Controls.Add(panel_Functions_Text1);
                #endregion

                // Set our beautiful graph
                Size graphSize = new Size(100, 100);
                Point graphPosition = new Point(Width / 2, panel_titleBar.Height - 1);

                graph = new Graph(string.Empty, (int)panel_Functions_xLimitCounter.Value, (int)panel_Functions_yLimitCounter.Value, float.Parse(panel_Functions_StepLimitTextBox.Text), (int)panel_Functions_ZoomLimitCounter.Value, graphSize, graphPosition, AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom, DockStyle.Fill, this);

                #region Added Controls
                graph.AddToMainControl();
                Controls.Add(panel_Functions);
                Controls.Add(panel_titleBar);
                #endregion

            }
        }

        private class AboutForm : Form
        {
            public AboutForm()
            {
                // Form styling
                Text = $"About {Program_Name}";
                ClientSize = new Size(480, 240);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                BackColor = Color.White;
                Font = new Font(DefaultFonts[0], 12);

                // Main layout panel
                var mainLayout = new TableLayoutPanel()
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    ColumnCount = 2,
                    RowCount = 3
                };

                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Text column
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // 64px panel column

                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                // Program Name (Title)
                var titleLabel = new Label()
                {
                    Text = Program_Name,
                    Font = new Font(Font.Name, 24, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true
                };

                // 64x64 Panel (Top Right)
                var logo = new PictureBox()
                {
                    Image = Resources.Logo,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(64, 64),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray, // Placeholder color
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Margin = new Padding(10, 0, 0, 0)
                };

                // Version + Developer Info
                var infoLabel = new Label()
                {
                    Text = $"- Version: {Program_Version}\n- Libraries:\n  ▻ {mXparser.VERSION_NAME} (github.com/mariuszgromada)\n\nDeveloped by {Program_DeveloperName}.",
                    Font = new Font(Font.Name, 10, FontStyle.Regular),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopLeft,
                    AutoSize = false
                };

                // Close Button
                var closeButton = new Button()
                {
                    Text = "Close",
                    DialogResult = DialogResult.OK,
                    Anchor = AnchorStyles.None,
                    AutoSize = true,
                    Padding = new Padding(10, 5, 10, 5),
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.System
                };

                AcceptButton = closeButton;

                // Add controls
                mainLayout.Controls.Add(titleLabel, 0, 0);
                mainLayout.Controls.Add(logo, 1, 0);
                mainLayout.Controls.Add(infoLabel, 0, 1);
                mainLayout.SetColumnSpan(infoLabel, 2);
                mainLayout.Controls.Add(closeButton, 0, 2);
                mainLayout.SetColumnSpan(closeButton, 2);

                Controls.Add(mainLayout);
            }
        }
    }
}
