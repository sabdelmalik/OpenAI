using AdvancedAligner;
using AdvancedAligner.Examples;
using Microsoft.VisualBasic.Devices;
using OpenAiAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BibleTaggingUtil.Editor
{
    public class TargetGridView : DataGridView
    {
        TargetParser targetParser;
        HebrewBibleParser hebrewBibleParser;
        ExamplesDatabase examplesDatabase;

        Dictionary<string, AlignmentResult> examples = new();

        public event VerseViewChangedEventHandler VerseViewChanged;
        public event RefernceHighlightRequestEventHandler RefernceHighlightRequest;
        public event GotoVerseRequestEventHandler GotoVerseRequest;

        AlignmentResult currentResult;
        ParserHebrewVerse hebrewVerse;
        ParserTargetVerse targetVerse;

        public TargetGridView()
        {
            this.ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            this.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TargetParser TargetParser
        {
            set { this.targetParser = value; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HebrewBibleParser HebrewBibleParser
        {
            set { this.hebrewBibleParser = value; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ExamplesDatabase ExamplesDatabase
        {
            set
            {
                this.examplesDatabase = value;
                examples.Clear();
                // parse the examples from the database into the examples dictionary
                foreach (var ex in examplesDatabase.Examples)
                {
                    examples[ex.Reference] = ex.Alignment;
                }
            }
        }

        public Dictionary<string, AlignmentResult> Examples
            { get { return this.examples; } }

        public void Remove (string reference)
        {
            this.examples.Remove(reference);
        }

        public void Clear()
        {
            this.Rows.Clear();
        }

        public bool IsLastWord
        {
            get
            {
                if (this.SelectedCells.Count == 1 &&
                    this.SelectedCells[0].ColumnIndex == Columns.Count - 1)
                    return true;
                else
                    return false;
            }
        }

        public bool IsFirstWord
        {
            get
            {
                if (this.SelectedCells.Count == 1 &&
                    this.SelectedCells[0].ColumnIndex == 0)
                    return true;
                else
                    return false;
            }
        }

        #region Context Menue

        private const string MERGE_CONTEXT_MENU = "Merge";
        private const string SWAP_CONTEXT_MENU = "Swap Tags";
        private const string SPLIT_CONTEXT_MENU = "Split";
        private const string DELETE_CONTEXT_MENU = "Delete Tag";
        private const string REVERSE_CONTEXT_MENU = "Reverse Tags";
        private const string DELETE_LEFT_CONTEXT_MENU = "Delete Left Tags";
        private const string DELETE_RIGHT_CONTEXT_MENU = "Delete Right Tags";

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (this.SelectedCells.Count == 0)
                return;


            if (this.SelectedCells.Count > 1)
            {
                bool sameRow = true;
                foreach (DataGridViewCell cell in this.SelectedCells)
                {
                    // we only merge in the top row
                    if (cell.RowIndex != 0)
                    {
                        sameRow = false;
                        break;
                    }
                }

                bool mergeOk = true; // we only merge adjacent cells
                int colIndex = SelectedCells[0].ColumnIndex;
                for (int i = 1; i < this.SelectedCells.Count; i++)
                {
                    if(Math.Abs(SelectedCells[i].ColumnIndex - colIndex) != 1)
                    {
                        mergeOk = false;
                        break;
                    }
                    colIndex = SelectedCells[i].ColumnIndex;
                }


                    if (sameRow)
                {
                    this.ContextMenuStrip.Items.Clear();
                    ToolStripMenuItem mergeMenuItem = new ToolStripMenuItem(MERGE_CONTEXT_MENU);
                    ToolStripMenuItem swapMenuItem = new ToolStripMenuItem(SWAP_CONTEXT_MENU);

                    if(mergeOk)
                        this.ContextMenuStrip.Items.Add(mergeMenuItem);
                    if(this.SelectedCells.Count == 2)
                        this.ContextMenuStrip.Items.Add(swapMenuItem);
                    e.Cancel = false;
                }
            }
            else
            {
                if (this.SelectedCells[0].RowIndex == Rows.Count - 1)
                {
                    string tag = (string)this.SelectedCells[0].Value;

                    if (tag == null)
                        return;
//                    if (string.IsNullOrEmpty(text))
//                        return;

                    this.ContextMenuStrip.Items.Clear();

                    this.ContextMenuStrip.Items.Clear();

                    string[] strings = tag.ToString().Split(' ');
                    if (strings.Length > 1)
                    {
                        ToolStripMenuItem reverseMenuItem = new ToolStripMenuItem(REVERSE_CONTEXT_MENU);
                        this.ContextMenuStrip.Items.Add(reverseMenuItem);

                        ToolStripMenuItem deleteLeftMenuItem = new ToolStripMenuItem(DELETE_LEFT_CONTEXT_MENU);
                        this.ContextMenuStrip.Items.Add(deleteLeftMenuItem);

                        ToolStripMenuItem deleteRightMenuItem = new ToolStripMenuItem(DELETE_RIGHT_CONTEXT_MENU);
                        this.ContextMenuStrip.Items.Add(deleteRightMenuItem);
                    }

                    ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem(DELETE_CONTEXT_MENU);
                    this.ContextMenuStrip.Items.Add(deleteMenuItem);


                    e.Cancel = false;
                }
                else
                {
                    this.ContextMenuStrip.Items.Clear();
                    ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem(SPLIT_CONTEXT_MENU);

                    this.ContextMenuStrip.Items.Add(deleteMenuItem);
                    e.Cancel = false;
                }
            }
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Text == MERGE_CONTEXT_MENU)
                {
                    int savedColumn = this.CurrentCell.ColumnIndex;
                    int savedRow = this.CurrentCell.RowIndex;

                    int firstMergeIndex = this.SelectedCells[0].ColumnIndex;
                    for (int i = 1; i < this.SelectedCells.Count; i++)
                    {
                        if (this.SelectedCells[i].ColumnIndex < firstMergeIndex)
                            firstMergeIndex = this.SelectedCells[i].ColumnIndex;
                    }

                    currentResult.Merge(firstMergeIndex - 1, this.SelectedCells.Count);
                    UpdateGrid();

                    //this.CurrentVerse.Merge(firstMergeIndex, this.SelectedCells.Count);

                    //if (firstMergeIndex == 0 && SelectedCells.Count == 2 && (string)this[firstMergeIndex, 0].Value == "«")
                    //{
                    //    this.CurrentVerse[0].Word = this.CurrentVerse[0].Word.Replace("« ", "«");
                    //}

                    //this.Update(CurrentVerse);
                    //SaveVerse(CurrentVerseReferece);

                    //this[firstMergeIndex, Rows.Count - 1].Selected = true;
                    //this.CurrentCell = this[firstMergeIndex, Rows.Count - 1];
                    //if (!string.IsNullOrEmpty(((string)this[firstMergeIndex, Rows.Count - 1].Value).ToString()))
                    //    FireRefernceHighlightRequest((string)this[firstMergeIndex, Rows.Count - 1].Value);
                    //FireVerseViewChanged();
                }
                else if (e.ClickedItem.Text == SWAP_CONTEXT_MENU)
                {
                    int savedColumn = this.CurrentCell.ColumnIndex;
                    int savedRow = this.CurrentCell.RowIndex;

                    int firstSwapIndex = this.SelectedCells[0].ColumnIndex;
                    int secondSwapIndex = this.SelectedCells[1].ColumnIndex;
                    //for (int i = 1; i < this.SelectedCells.Count; i++)
                    //{
                    //    if (this.SelectedCells[i].ColumnIndex < firstSwapIndex)
                    //        firstSwapIndex = this.SelectedCells[i].ColumnIndex;
                    //}
                    //this.CurrentVerse.SwapTags(firstSwapIndex, secondSwapIndex);

                    //SaveVerse(CurrentVerseReferece);
                    //this.Update(CurrentVerse);

                    //this[firstSwapIndex, Rows.Count - 1].Selected = true;
                    //this.CurrentCell = this[firstSwapIndex, Rows.Count - 1];
                    //if (!string.IsNullOrEmpty(((string)this[firstSwapIndex, Rows.Count - 1].Value).ToString()))
                    //    FireRefernceHighlightRequest((string)this[firstSwapIndex, Rows.Count - 1].Value);
                    FireVerseViewChanged();
                }
                else if (e.ClickedItem.Text == SPLIT_CONTEXT_MENU)
                {
                    int savedColumn = this.CurrentCell.ColumnIndex;
                    int savedRow = this.CurrentCell.RowIndex;

                    int splitIndex = this.SelectedCells[0].ColumnIndex;
                    for (int i = 1; i < this.SelectedCells.Count; i++)
                    {
                        if (this.SelectedCells[i].ColumnIndex < splitIndex)
                            splitIndex = this.SelectedCells[i].ColumnIndex;
                    }

                    currentResult.Split(splitIndex - 1);
                    UpdateGrid();

                    //this.CurrentVerse.split(splitIndex);

                    //this.Update(CurrentVerse);
                    //SaveVerse(CurrentVerseReferece);

                    //this[splitIndex, Rows.Count - 1].Selected = true;
                    //this.CurrentCell = this[splitIndex, Rows.Count - 1];
                    //if (!string.IsNullOrEmpty(((string)this[splitIndex, Rows.Count - 1].Value).ToString()))
                    //    FireRefernceHighlightRequest((string)this[splitIndex, Rows.Count - 1].Value);
                    FireVerseViewChanged();
                }
                else if (e.ClickedItem.Text == DELETE_CONTEXT_MENU)
                {
                    int savedColumn = this.CurrentCell.ColumnIndex;
                    int savedRow = this.CurrentCell.RowIndex;

                    int col = this.SelectedCells[0].ColumnIndex;
                    currentResult.ReplaceHebrew(col, new List<int>());
                    UpdateGrid();
                    //this.CurrentVerse[col].Strong = new string(new string[] { "" });
                    //this.Update(CurrentVerse);
                    //SaveVerse(CurrentVerseReferece);

                    this[col, Rows.Count - 1].Selected = true;
                    this.CurrentCell = this[col, Rows.Count - 1];
                    FireVerseViewChanged();
                }
                else
                {
                    int col = this.SelectedCells[0].ColumnIndex;
                    //string strongsCluster = this.CurrentVerse[col].Strong;
                    //if (strongsCluster.Count < 2) return;

                    //int savedColumn = this.CurrentCell.ColumnIndex;
                    //int savedRow = this.CurrentCell.RowIndex;

                    //if (e.ClickedItem.Text == REVERSE_CONTEXT_MENU)
                    //{
                    //    StrongsNumber temp = strongsCluster[0];
                    //    strongsCluster[0] = strongsCluster[strongsCluster.Count -1];
                    //    strongsCluster[strongsCluster.Count - 1] = temp;
                    //}
                    //else if (e.ClickedItem.Text == DELETE_LEFT_CONTEXT_MENU)
                    //{
                    //    strongsCluster.DeleteAt(0);
                    //}
                    //else if (e.ClickedItem.Text == DELETE_RIGHT_CONTEXT_MENU)
                    //{
                    //    strongsCluster.DeleteAt(strongsCluster.Count-1);
                    //}

                    //SaveVerse(CurrentVerseReferece);
                    //Update(CurrentVerse);
                    //FireRefernceHighlightRequest((string)this[col, Rows.Count - 1].Value);
                    //FireVerseViewChanged();
                }
            }
            catch (Exception ex)
            {
                var cm = System.Reflection.MethodBase.GetCurrentMethod();
                var name = cm.DeclaringType.FullName + "." + cm.Name;
                //Tracing.TraceException(name, ex.Message);
                throw;
            }
        }

        #endregion Context Menue

        #region Save & Update
        /// <summary>
        /// Updates the target targetVerse display when the targetVerse contains tags already
        /// </summary>
        /// <param name="targetVerse">tagged targetVerse</param>
        public void Update(string reference)
        {
            hebrewVerse = hebrewBibleParser.HebrewBible[targetParser.referenceIndices[reference]];
            targetVerse = targetParser.TargetBible[targetParser.referenceIndices[reference]];
            currentResult = examples[reference];
            currentResult = examples[reference];

            if (targetVerse == null)
                return;

            UpdateGrid();

        }
        private void UpdateGrid()
        {
            try
            {
                this.Rows.Clear();


                List<string> indices = new();
                List<string> words = new();
                List<string> indicesH = new();
                List<string> wordsH = new();
                List<string> strongsH = new();
                List<string> lemmasH = new();
                List<string> posH = new();
                List<string> morphsH = new();
                List<string> glossH = new();

                indices.Add(RowTitles.IDXT.ToString());
                words.Add(RowTitles.ENGT.ToString());
                indicesH.Add(RowTitles.IDX.ToString());
                glossH.Add(RowTitles.ENG.ToString());
                wordsH.Add(RowTitles.HEB.ToString());
                lemmasH.Add(RowTitles.LEM.ToString());
                posH.Add(RowTitles.POS.ToString());
                morphsH.Add(RowTitles.MRF.ToString());
                strongsH.Add(RowTitles.SRG.ToString());

                var pairs = currentResult.alignments;
                foreach (var pair in pairs)
                {
                    string idx = string.Empty;
                    string targetWord = string.Empty;
                    foreach (var i in pair.t)
                    {
                        idx += $"{i}, ";
                        targetWord += $"{targetVerse.Tokens[i].surface}, ";
                    }
                    indices.Add(idx.Trim().Trim(','));
                    words.Add(targetWord.Trim().Trim(','));

                    string index = string.Empty;
                    string gloss = string.Empty;
                    string word = string.Empty;
                    string lemma = string.Empty;
                    string pos = string.Empty;
                    string morph = string.Empty;
                                    string strong = string.Empty;
                    foreach (var i in pair.h)
                    {
                        index += $"{i}, ";
                        gloss += $"{hebrewVerse.Tokens[i].gloss}, ";
                        word += $"{hebrewVerse.Tokens[i].surface}, ";
                        lemma += $"{hebrewVerse.Tokens[i].lemma}, ";
                        pos += $"{hebrewVerse.Tokens[i].pos}, ";
                        morph += $"{hebrewVerse.Tokens[i].morph}, ";
                        strong += $"{hebrewVerse.Tokens[i].strong}, ";
                    }
                    indicesH.Add(index.Trim().Trim(','));
                    glossH.Add(gloss.Trim().Trim(','));
                    wordsH.Add(word.Trim().Trim(','));
                    lemmasH.Add(lemma.Trim().Trim(','));
                    posH.Add(pos.Trim().Trim(','));
                    morphsH.Add(morph.Trim().Trim(','));
                    strongsH.Add(strong.Trim().Trim(','));
                }

                this.ColumnCount = indices.Count;
                this.Rows.Add(indices.ToArray());
                this.Rows.Add(words.ToArray());
                this.Rows.Add(indicesH.ToArray());
                this.Rows.Add(glossH.ToArray());
                this.Rows.Add(wordsH.ToArray());
                this.Rows.Add(lemmasH.ToArray());
                this.Rows.Add(posH.ToArray());
                this.Rows.Add(morphsH.ToArray());
                this.Rows.Add(strongsH.ToArray());
                
                //FireRefernceHighlightRequest((string)this[0, tRow].Value);
                //this.Rows[tRow].ReadOnly = true;

            }
            catch (Exception ex)
            {
                var cm = System.Reflection.MethodBase.GetCurrentMethod();
                var name = cm.DeclaringType.FullName + "." + cm.Name;
                //Tracing.TraceException(name, ex.Message);
            }
        }

       #endregion Save & Update


        #region overrides
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            if (row == 0)
            {
                string newWord = (string)this[e.ColumnIndex, e.RowIndex].Value;
                //CurrentVerse.UpdateWord(e.ColumnIndex, newWord);
                //                FireVerseViewChanged();
            }
            else if (row == this.RowCount - 1) 
            {
                //string oldValue = CurrentVerse[col].Strong;
                //string newValue = null;
                // tags Row
                if(this[col, row].Value == null)
                {

                //    newValue = new string(new string[] {""});
                //}
                //else if (this[col, row].Value is string)
                //{
                //    // if the new value is a string, this means the tag may've been edited been edited
                //    string temp = (string)this[col, row].Value;
                //    newValue = new string(temp.Split(' '));
                //    if (oldValue.ToString() == temp)
                //    {
                //        this[col, row].Value = newValue;
                //    }
                }
                else
                {
                    //newValue = (string)this[col, row].Value;
                }
                //if (CurrentVerse[col].Strong.ToString() != newValue.ToString())
                //{
                //    if (this.CurrentVerse != null)
                //        undoStack.Push(new VerseEx(new Verse(this.CurrentVerse), col, row));

                //    // if (newValue == null) newValue = string.Empty; TODO:this should be an exception
                //    CurrentVerse[col].Strong = newValue;

                //    this[e.ColumnIndex, e.RowIndex].Value = newValue;

                //    FireRefernceHighlightRequest(newValue);

                //    //SaveVerse(CurrentVerseReferece);
                //    FireVerseViewChanged();
                //}
            }

            base.OnCellValueChanged(e);
        }


        bool gotFocus = false;

        protected override void OnLostFocus(EventArgs e)
        {
            gotFocus = false;
            base.OnLostFocus(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            gotFocus = true;
            base.OnGotFocus(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            if (this.Rows.Count > 1)
            {
                if (!this.gotFocus)
                {
                    return;
                }
                // during initialsation, we may come here
                // when the grid rows are not fully initialised
                //if (this.SelectedRows.Count > 1)
                //{
                //}
                if (e.RowIndex == Rows.Count - 1)
                {
                    this.ClearSelection();
                    this[e.ColumnIndex, e.RowIndex].Selected = true;
                }
                // find the IDX row
                string hebrewIndices = string.Empty;
                for (int i = 0; i < this.Rows.Count; i++)
                {
                    // our row has cell 0 value = RowTitles.IDX.ToString()
                    if ((string)this.Rows[i].Cells[0].Value == RowTitles.IDX.ToString())
                    {
                        hebrewIndices = (string)this.Rows[i].Cells[e.ColumnIndex].Value;
                        break;
                    }
                }
                FireRefernceHighlightRequest(hebrewIndices);

            }
            //base.OnCellEnter(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnColumnRemoved(DataGridViewColumnEventArgs e)
        //{
        //    FireVerseViewChanged();
        //    base.OnColumnRemoved(e);
        //}

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if(this.SelectedCells.Count == 1)
                {
                    //int savedColumn = this.CurrentCell.ColumnIndex;
                    //int savedRow = this.CurrentCell.RowIndex;
                    //if (this.CurrentVerse != null)
                    //    undoStack.Push(new VerseEx(new Verse(this.CurrentVerse), savedColumn, savedRow));

                    //int col = this.SelectedCells[0].ColumnIndex;
                    //this[col, Rows.Count - 1].Value = string.Empty;
                    //this.CurrentVerse[col].Strong = new string();
                    //SaveVerse(CurrentVerseReferece);
                    //this.Update(CurrentVerse);
                    //SaveVerse(CurrentVerseReferece);

                    //this[col, Rows.Count - 1].Selected = true;
                    //this.CurrentCell = this[col, Rows.Count - 1];
                    FireVerseViewChanged();

                }
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            drgevent.Effect = DragDropEffects.Copy;
            base.OnDragEnter(drgevent);
        }
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            var cm = System.Reflection.MethodBase.GetCurrentMethod();
            var name = cm.DeclaringType.FullName + "." + cm.Name;

            DragData dragData = drgevent.Data.GetData(typeof(DragData)) as DragData;
            //string droppedValue = data.Tag;

            //Tracing.TraceEntry(name, $"string = {droppedValue.ToString()}");
            
            Point cursorLocation = this.PointToClient(new Point(drgevent.X, drgevent.Y));
            try
            {
                System.Windows.Forms.DataGridView.HitTestInfo hittest = this.HitTest(cursorLocation.X, cursorLocation.Y);
                if (hittest.ColumnIndex != -1)
                {
                    string index = string.Empty;
                    string gloss = string.Empty;
                    string word = string.Empty;
                    string lemma = string.Empty;
                    string pos = string.Empty;
                    string morph = string.Empty;
                    string strong = string.Empty;

                    List<int> hebrewList = new List<int>();   

                    int column = hittest.ColumnIndex;
                    foreach (var data in dragData.Data)
                    {
                        if (data.Source.Equals(this))
                        {
                            if (data.ColumnIndex == hittest.ColumnIndex)
                                return;
                        }

                        hebrewList.Add(int.Parse(data.Index));

                        index += $"{data.Index}, ";
                        gloss += $"{data.Gloss}, ";
                        word += $"{data.Word}, ";
                        lemma += $"{data.Lemma}, ";
                        pos += $"{data.POS}, ";
                        morph += $"{data.Morph}, ";
                        strong += $"{data.Strong}, ";
                    }

                    currentResult.ReplaceHebrew(hittest.ColumnIndex - 1, hebrewList);
                    UpdateGrid();

                }
            }
            catch (Exception ex)
            {
                // Tracing.TraceException(name, ex.Message);
            }

            base.OnDragDrop(drgevent);
        }

         /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left && e.Clicks == 1)
        //    {
        //        DataGridView.HitTestInfo info = this.HitTest(e.X, e.Y);
        //        if (info.RowIndex > 0)
        //        {
        //            if (info.RowIndex >= 0 && info.ColumnIndex >= 0)
        //            {
        //                string tag = (string)this.Rows[Rows.Count - 1].Cells[info.ColumnIndex].Value;
        //                DragData data = new DragData(1, info.ColumnIndex, tag, this);
        //                if (data != null)
        //                {
        //                    //Need to put braces here  CHANGE
        //                    this.DoDragDrop(data, DragDropEffects.Copy);
        //                }
        //            }
        //        }
        //    }

        //    base.OnMouseDown(e);
        //}

        #endregion overrides


        #region Firing events
        public void FireVerseViewChanged()
        {
            if (this.VerseViewChanged != null)
            {
                new Thread(() =>
                {
                    try
                    {
                        this.VerseViewChanged(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        var cm = System.Reflection.MethodBase.GetCurrentMethod();
                        var name = cm.DeclaringType.FullName + "." + cm.Name;
                        //Tracing.TraceException(name, ex.Message);
                    }
                }).Start();
            }

        }

        public void FireRefernceHighlightRequest(string tag)
        {
            if (this.RefernceHighlightRequest != null)
            {
//                new Thread(() =>
//                {
                    try
                    {
                        bool firstHalf = true;
                        //if (this.SelectedCells.Count == 1)
                        {
                            if (this.SelectedCells[0].ColumnIndex > (this.ColumnCount / 2))
                                firstHalf = false;
                        }
                        this.RefernceHighlightRequest(this, tag, firstHalf);
                    }
                    catch (Exception ex)
                    {
                        var cm = System.Reflection.MethodBase.GetCurrentMethod();
                        var name = cm.DeclaringType.FullName + "." + cm.Name;
                        //Tracing.TraceException(name, ex.Message);
                    }
//                }).Start();
            }
        }

        public void FireGotoVerseRequest(string tag)
        {
            if (this.GotoVerseRequest != null)
            {
                new Thread(() =>
                {
                    try
                    {
                        this.GotoVerseRequest(this, tag);
                    }
                    catch (Exception ex)
                    {
                        var cm = System.Reflection.MethodBase.GetCurrentMethod();
                        var name = cm.DeclaringType.FullName + "." + cm.Name;
                        //Tracing.TraceException(name, ex.Message);
                    }
                }).Start();
            }

        }
        #endregion Firing events

    }
    public delegate void VerseViewChangedEventHandler(object sender, EventArgs e);
    public delegate void RefernceHighlightRequestEventHandler(object sender, string tag, bool firstHalf);
    public delegate void GotoVerseRequestEventHandler(object sender, string reference);


}
