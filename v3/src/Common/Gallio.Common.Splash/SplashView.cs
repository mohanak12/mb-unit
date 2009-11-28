﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gallio.Common.Splash
{
    /// <summary>
    /// A SplashView is a custom control to display styled text intermixed with
    /// embedded content such as images.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This control is optimized for high-performance append-only text output such as
    /// scrolling consoles.  Consequently, the control does not support editing or
    /// modifications to currently displayed content (besides clearing it).
    /// </para>
    /// <para>
    /// Display updates are performed asynchronously in batches.
    /// </para>
    /// </remarks>
    public class SplashView : ScrollableControl
    {
        private readonly SplashDocument document;
        private readonly SplashLayout layout;
        private readonly PaintOptions paintOptions;

        private MenuItem copyMenuItem;
        private MenuItem selectAllMenuItem;
        private MenuItem readingOrderMenuItem;

        private int selectionStart;
        private int selectionLength;
        private SnapPosition selectionSnapPosition;
        private bool selectionInProgress;

        private int minimumTextLayoutWidth = 100;

        /// <summary>
        /// Creates an empty SplashView.
        /// </summary>
        public SplashView()
        {
            document = new SplashDocument();
            layout = new SplashLayout(document);
            paintOptions = new PaintOptions();

            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable | ControlStyles.UserMouse
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            base.BackColor = paintOptions.BackgroundColor;
            Padding = new Padding(5);

            AttachLayoutEvents();
            InitializeContextMenu();
        }

        private void AttachLayoutEvents()
        {
            layout.UpdateRequired += HandleLayoutChanged;
        }

        private void HandleLayoutChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Event raised when the selection has changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <inheritdoc />
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                paintOptions.BackgroundColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected text color.
        /// </summary>
        public Color SelectedTextColor
        {
            get { return paintOptions.SelectedTextColor; }
            set
            {
                if (paintOptions.SelectedTextColor != value)
                {
                    paintOptions.SelectedTextColor = value;
                    UpdateSelectionColors();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected text color.
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return paintOptions.SelectedBackgroundColor; }
            set
            {
                if (paintOptions.SelectedBackgroundColor != value)
                {
                    paintOptions.SelectedBackgroundColor = value;
                    UpdateSelectionColors();
                }
            }
        }

        /// <summary>
        /// Gets the selection start character index.
        /// </summary>
        public int SelectionStart
        {
            get { return selectionStart; }
        }

        /// <summary>
        /// Gets the selection length, or 0 if none.
        /// </summary>
        public int SelectionLength
        {
            get { return selectionLength; }
        }

        /// <summary>
        /// Gets or sets the minimum width of the text area.
        /// </summary>
        /// <remarks>
        /// If the control is resized to less than this width, then the control will automatically
        /// display a horizontal scrollbar.
        /// </remarks>
        public int MinimumTextLayoutWidth
        {
            get { return minimumTextLayoutWidth; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Value must be at least 1.");

                if (minimumTextLayoutWidth != value)
                {
                    minimumTextLayoutWidth = value;
                    UpdateLayoutSize();
                    UpdateScrollBars();
                }
            }
        }

        /// <summary>
        /// Gets the document displayed in the view.
        /// </summary>
        public SplashDocument Document
        {
            get { return document; }
        }

        /// <summary>
        /// Gets the snap position that corresponds to a point in the control.
        /// </summary>
        /// <param name="point">The point relative to the layout origin.</param>
        /// <returns>The character snap.</returns>
        public SnapPosition GetSnapPositionAtPoint(Point point)
        {
            Rectangle displayRect = DisplayRectangle;
            return layout.GetSnapPositionAtPoint(point, displayRect.Location);
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="selectionStart">The selection start character index.</param>
        /// <param name="selectionLength">The selection length or 0 if none.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="selectionStart"/>
        /// or <paramref name="selectionLength"/> is less than 0.</exception>
        public void Select(int selectionStart, int selectionLength)
        {
            if (selectionStart < 0)
                throw new ArgumentOutOfRangeException("selectionStart");
            if (selectionLength < 0)
                throw new ArgumentOutOfRangeException("selectionLength");

            InternalSetSelection(selectionStart, selectionLength);
        }

        /// <summary>
        /// Sets the selection to include all text in the document.
        /// </summary>
        public void SelectAll()
        {
            InternalSetSelection(0, document.CharCount);
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void SelectNone()
        {
            if (selectionLength != 0)
                InternalSetSelection(0, 0);
        }

        private void InternalSetSelection(int selectionStart, int selectionLength)
        {
            int charCount = document.CharCount;
            if (selectionStart > charCount)
            {
                selectionStart = charCount;
                selectionLength = 0;
            }
            else if (selectionStart + selectionLength > charCount)
            {
                selectionLength = charCount - selectionStart;
            }

            if (this.selectionStart != selectionStart || this.selectionLength != selectionLength)
            {
                this.selectionStart = selectionStart;
                this.selectionLength = selectionLength;

                OnSelectionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the currently selected text in the document, or an empty string
        /// if there is no selection.
        /// </summary>
        /// <returns>The selected text.</returns>
        public string GetSelectedText()
        {
            if (selectionLength == 0)
                return string.Empty;

            return document.GetText(selectionStart, selectionLength);
        }

        /// <summary>
        /// Copies the selected text to the clipboard.
        /// Does nothing if there is no current selection.
        /// </summary>
        /// <returns>True if some text was copied, false if there was no current selection.</returns>
        public bool CopySelectedTextToClipboard()
        {
            if (selectionLength == 0)
                return false;

            Clipboard.SetText(GetSelectedText().Replace("\n", "\r\n"));
            return true;
        }

        /// <inheritdoc />
        protected override Cursor DefaultCursor
        {
            get { return Cursors.IBeam; }
        }

        /// <inheritdoc />
        protected override Size DefaultSize
        {
            get { return new Size(minimumTextLayoutWidth + Padding.Horizontal,
                Style.CreateDefaultStyle().Font.Height + Padding.Vertical); }
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            UpdateLayout();

            Rectangle displayRect = DisplayRectangle;
            layout.Paint(e.Graphics, displayRect.Location, e.ClipRectangle, paintOptions, selectionStart, selectionLength);

            base.OnPaint(e);
        }

        /// <inheritdoc />
        protected override void OnPaddingChanged(EventArgs e)
        {
            UpdateLayoutSize();

            base.OnPaddingChanged(e);
        }

        /// <inheritdoc />
        protected override void OnResize(EventArgs e)
        {
            UpdateLayoutSize();

            base.OnResize(e);
        }

        /// <inheritdoc />
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            UpdateLayoutRightToLeft();

            base.OnRightToLeftChanged(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!selectionInProgress)
                {
                    selectionSnapPosition = GetSnapPositionAtPoint(MousePositionToLayout(e.Location));
                    SelectNone();
                }
            }

            base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectionInProgress)
                {
                    UpdateSelectionInProgress(e);
                    selectionInProgress = false;
                }
            }
            else
            {
                selectionInProgress = false;
            }

            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectionSnapPosition.Kind != SnapKind.None)
                    selectionInProgress = true;

                if (selectionInProgress)
                    UpdateSelectionInProgress(e);
            }
            else
            {
                selectionInProgress = false;
            }

            base.OnMouseMove(e);
        }

        private void UpdateSelectionInProgress(MouseEventArgs e)
        {
            int selectionCharIndex = selectionSnapPosition.CharIndex;
            if (selectionSnapPosition.Kind == SnapKind.Trailing)
                selectionCharIndex += 1;

            SnapPosition currentSnapPosition = GetSnapPositionAtPoint(MousePositionToLayout(e.Location));
            if (currentSnapPosition.Kind == SnapKind.None)
            {
                InternalSetSelection(selectionCharIndex, 0);
            }
            else
            {
                int currentCharIndex = currentSnapPosition.CharIndex;
                if (currentSnapPosition.Kind == SnapKind.Trailing)
                    currentCharIndex += 1;

                if (selectionCharIndex <= currentCharIndex)
                    InternalSetSelection(selectionCharIndex, currentCharIndex - selectionCharIndex);
                else
                    InternalSetSelection(currentCharIndex, selectionCharIndex - currentCharIndex);
            }
        }

        /// <summary>
        /// Called when the selection changes.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);

            Invalidate();
        }

        /// <summary>
        /// Maps a mouse position to a layout point.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        /// <returns>The layout point.</returns>
        public Point MousePositionToLayout(Point mousePosition)
        {
            // HACK: Correct for the fact that the I-Beam cursor seems to have its hotspot to the
            // left of the bar rather than on the bar.  -- Jeff.
            mousePosition.X += 3;
            return mousePosition;
        }

        /// <inheritdoc />
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            UpdateContextMenu();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Initializes the context menu.
        /// </summary>
        protected virtual void InitializeContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Popup += (sender, e) => UpdateContextMenu();

            copyMenuItem = new MenuItem("Copy", HandleCopyMenuItemClicked)
            {
                MergeOrder = 0,
                Shortcut = Shortcut.CtrlC,
                ShowShortcut = true
            };
            ContextMenu.MenuItems.Add(copyMenuItem);

            selectAllMenuItem = new MenuItem("Select All", HandleSelectAllMenuItemClicked)
            {
                MergeOrder = 0,
                Shortcut = Shortcut.CtrlA,
                ShowShortcut = true
            };
            ContextMenu.MenuItems.Add(selectAllMenuItem);

            ContextMenu.MenuItems.Add("-");

            readingOrderMenuItem = new MenuItem("Right to Left Reading Order", HandleRightToLeftReadingOrderMenuItemClicked)
            {
                MergeOrder = 1
            };
            ContextMenu.MenuItems.Add(readingOrderMenuItem);
        }

        /// <summary>
        /// Updates the context menu in time for pop-up.
        /// </summary>
        protected virtual void UpdateContextMenu()
        {
            copyMenuItem.Enabled = selectionLength != 0;
            selectAllMenuItem.Enabled = document.CharCount != 0;
            readingOrderMenuItem.Checked = RightToLeft == RightToLeft.Yes;
        }

        private void HandleCopyMenuItemClicked(object sender, EventArgs e)
        {
            CopySelectedTextToClipboard();
        }

        private void HandleSelectAllMenuItemClicked(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void HandleRightToLeftReadingOrderMenuItemClicked(object sender, EventArgs e)
        {
            RightToLeft = RightToLeft == RightToLeft.Yes ? RightToLeft.No : RightToLeft.Yes;
        }

        private void UpdateLayout()
        {
            UpdateLayoutSize();
            UpdateLayoutRightToLeft();
            layout.Update();
            UpdateScrollBars();
        }

        private void UpdateLayoutSize()
        {
            Rectangle displayRect = DisplayRectangle;
            layout.DesiredLayoutWidth = Math.Max(displayRect.Width, minimumTextLayoutWidth);
        }

        private void UpdateLayoutRightToLeft()
        {
            layout.DesiredLayoutRightToLeft = RightToLeft == RightToLeft.Yes;
        }

        private void UpdateScrollBars()
        {
            AutoScrollMinSize = new Size(minimumTextLayoutWidth, layout.CurrentLayoutHeight);
        }

        private void UpdateSelectionColors()
        {
            if (selectionLength != 0)
                Invalidate();
        }
    }
}
