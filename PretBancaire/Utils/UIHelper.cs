using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PretBancaire.Utils
{
    public static class UIHelper
    {
        public static readonly Color BgDark = Color.FromArgb(15, 23, 42);
        public static readonly Color BgCard = Color.FromArgb(30, 41, 59);
        public static readonly Color BgInput = Color.FromArgb(20, 30, 48);
        public static readonly Color TextPrimary = Color.FromArgb(226, 232, 240);
        public static readonly Color TextSecondary = Color.FromArgb(148, 163, 184);
        public static readonly Color AccentBlue = Color.FromArgb(56, 189, 248);
        public static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        public static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        public static readonly Color AccentOrange = Color.FromArgb(251, 146, 60);
        public static readonly Color GridLine = Color.FromArgb(30, 41, 59);
        public static readonly Color RowAlt = Color.FromArgb(20, 30, 50);
        public static readonly Color SelectionBg = Color.FromArgb(29, 78, 137);

        // Couleurs des pastilles par categorie
        public static readonly Color DotIdentity = Color.FromArgb(99, 102, 241);   // violet - ID
        public static readonly Color DotPerson = Color.FromArgb(59, 130, 246);      // bleu - personnes
        public static readonly Color DotDocument = Color.FromArgb(234, 179, 8);     // jaune - documents
        public static readonly Color DotContact = Color.FromArgb(34, 197, 94);      // vert - contact
        public static readonly Color DotDate = Color.FromArgb(236, 72, 153);        // rose - dates
        public static readonly Color DotMoney = Color.FromArgb(251, 146, 60);       // orange - montants
        public static readonly Color DotStatus = Color.FromArgb(20, 184, 166);      // teal - statuts
        public static readonly Color DotSecurity = Color.FromArgb(239, 68, 68);     // rouge - securite
        public static readonly Color DotSettings = Color.FromArgb(148, 163, 184);   // gris - parametres

        // Stocke la couleur de pastille pour chaque colonne
        private static readonly Dictionary<string, Color> _columnDotColors = new();

        // Polices et formats mis en cache pour éviter les fuites mémoire GDI
        private static readonly Font HeaderFont = new("Segoe UI", 9.5f, FontStyle.Bold);
        private static readonly StringFormat HeaderStringFormat = new()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        /// <summary>
        /// Enregistre la couleur de pastille pour un en-tete de colonne
        /// </summary>
        public static void SetColumnDotColor(DataGridView dgv, string columnName, Color dotColor)
        {
            string key = dgv.GetHashCode() + "_" + columnName;
            _columnDotColors[key] = dotColor;
        }

        public static void FormaterGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = BgDark;
            dgv.ForeColor = TextPrimary;
            dgv.GridColor = GridLine;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;
            dgv.Font = new Font("Segoe UI", 10);

            dgv.DefaultCellStyle.BackColor = BgDark;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = SelectionBg;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = SelectionBg;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            // En-tetes - fond clair distinct
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 238, 248);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(26, 10, 8, 10);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 42;

            // Custom paint pour dessiner les pastilles de couleur
            dgv.CellPainting += DgvCellPainting;
        }

        private static void DgvCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            if (e.RowIndex != -1) return; // seulement les en-tetes
            if (e.ColumnIndex < 0) return;

            var col = dgv.Columns[e.ColumnIndex];
            string key = dgv.GetHashCode() + "_" + col.Name;

            if (!_columnDotColors.TryGetValue(key, out Color dotColor))
                return;

            e.PaintBackground(e.ClipBounds, false);

            // Dessiner la pastille ronde coloree
            int dotSize = 10;
            int dotX = e.CellBounds.X + 10;
            int dotY = e.CellBounds.Y + (e.CellBounds.Height - dotSize) / 2;

            using (var brush = new SolidBrush(dotColor))
            {
                e.Graphics!.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }

            // Dessiner le texte apres la pastille
            int textX = dotX + dotSize + 8;
            var textRect = new Rectangle(textX, e.CellBounds.Y, e.CellBounds.Width - (textX - e.CellBounds.X) - 4, e.CellBounds.Height);
            var textColor = Color.FromArgb(55, 65, 81);

            using (var textBrush = new SolidBrush(textColor))
            {
                e.Graphics!.DrawString(col.HeaderText, HeaderFont, textBrush, textRect, HeaderStringFormat);
            }

            e.Handled = true;
        }

        public static Button CreerBouton(string texte, Color couleur, int largeur, int hauteur = 38)
        {
            var btn = new Button
            {
                Text = texte,
                Size = new Size(largeur, hauteur),
                BackColor = couleur,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(6, 0, 6, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(couleur, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(couleur, 0.1f);

            btn.Region = CreateRoundedRegion(largeur, hauteur, 8);
            return btn;
        }

        public static Panel CreerChamp(string texteLabel, Control input, int largeur)
        {
            var p = new Panel { Width = largeur, Height = 62, Margin = new Padding(0, 0, 12, 10) };
            var lbl = new Label
            {
                Text = texteLabel.ToUpper(),
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold)
            };
            input.Dock = DockStyle.Bottom;
            input.Height = 34;
            input.Font = new Font("Segoe UI", 10);
            p.Controls.Add(input);
            p.Controls.Add(lbl);
            return p;
        }

        public static TextBox CreerTextBox(int w) => new()
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(w, 34),
            BackColor = BgInput,
            ForeColor = TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };

        private static Region CreateRoundedRegion(int w, int h, int r)
        {
            using var path = CreateRoundedPath(0, 0, w, h, r);
            return new Region(path);
        }

        private static GraphicsPath CreateRoundedPath(int x, int y, int w, int h, int r)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}