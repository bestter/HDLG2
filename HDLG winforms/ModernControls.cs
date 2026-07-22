/*
 This file is part of HTML Directory List Generator.

 HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

 HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
 */

using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace HDLG_winforms
{
	/// <summary>
	/// Represents a modern card panel container with header, heading, and description.
	/// </summary>
	public class ModernCardPanel : Panel
	{
		private readonly Panel headerPanel;
		private readonly Label headingLabel;
		private readonly Label descriptionLabel;
		private readonly Panel contentPanel;

		private string heading = "Header";
		private string description = string.Empty;

		public ModernCardPanel()
		{
			DoubleBuffered = true;
			BackColor = Color.White;
			Padding = new Padding(1);

			headerPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 52,
				BackColor = Color.FromArgb(248, 250, 252),
				Padding = new Padding(14, 8, 14, 6)
			};

			headingLabel = new Label
			{
				Dock = DockStyle.Top,
				Height = 22,
				Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
				ForeColor = Color.FromArgb(15, 23, 42),
				Text = heading
			};

			descriptionLabel = new Label
			{
				Dock = DockStyle.Top,
				Height = 18,
				Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
				ForeColor = Color.FromArgb(100, 116, 139),
				Text = description
			};

			headerPanel.Controls.Add(descriptionLabel);
			headerPanel.Controls.Add(headingLabel);

			contentPanel = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Color.White,
				Padding = new Padding(12)
			};

			base.Controls.Add(contentPanel);
			base.Controls.Add(headerPanel);
		}

		[Category("Appearance")]
		[DefaultValue("Header")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Heading
		{
			get => heading;
			set
			{
				heading = value;
				headingLabel.Text = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Description
		{
			get => description;
			set
			{
				description = value;
				descriptionLabel.Text = value;
				descriptionLabel.Visible = !string.IsNullOrWhiteSpace(value);
				headerPanel.Height = string.IsNullOrWhiteSpace(value) ? 36 : 52;
			}
		}

		[Category("Layout")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Panel ContentPanel => contentPanel;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				headingLabel.Dispose();
				descriptionLabel.Dispose();
				headerPanel.Dispose();
				contentPanel.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			ArgumentNullException.ThrowIfNull(e);
			base.OnPaint(e);
			using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
			e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
		}
	}

	/// <summary>
	/// Represents a modern flat styled button.
	/// </summary>
	public class ModernButton : Button
	{
		private bool isPrimary = true;
		private bool isHovered;
		private bool isPressed;

		public ModernButton()
		{
			DoubleBuffered = true;
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
			Cursor = Cursors.Hand;
			Size = new Size(130, 36);
			UpdateColors();
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool IsPrimary
		{
			get => isPrimary;
			set
			{
				isPrimary = value;
				UpdateColors();
				Invalidate();
			}
		}

		private void UpdateColors()
		{
			if (isPrimary)
			{
				ForeColor = Color.White;
			}
			else
			{
				ForeColor = Color.FromArgb(15, 23, 42);
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			isHovered = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			isHovered = false;
			isPressed = false;
			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			base.OnMouseDown(mevent);
			isPressed = true;
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp(mevent);
			isPressed = false;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			ArgumentNullException.ThrowIfNull(pevent);
			Graphics g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			Color baseBg = isPrimary ? Color.FromArgb(2, 132, 199) : Color.FromArgb(241, 245, 249);
			Color hoverBg = isPrimary ? Color.FromArgb(3, 105, 161) : Color.FromArgb(226, 232, 240);
			Color pressBg = isPrimary ? Color.FromArgb(7, 89, 133) : Color.FromArgb(203, 213, 225);

			if (!Enabled)
			{
				baseBg = Color.FromArgb(226, 232, 240);
			}
			else if (isPressed)
			{
				baseBg = pressBg;
			}
			else if (isHovered)
			{
				baseBg = hoverBg;
			}

			using (var brush = new SolidBrush(baseBg))
			{
				g.FillRectangle(brush, ClientRectangle);
			}

			if (!isPrimary && Enabled)
			{
				using var borderPen = new Pen(Color.FromArgb(203, 213, 225), 1);
				g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
			}

			Color textColor = !Enabled ? Color.FromArgb(148, 163, 184) : (isPrimary ? Color.White : Color.FromArgb(15, 23, 42));

			TextRenderer.DrawText(
				g,
				Text,
				Font,
				ClientRectangle,
				textColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
		}
	}
}
