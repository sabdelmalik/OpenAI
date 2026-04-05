using AdvancedAligner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace BibleTaggingUtil.Editor
{
    public class HebrewGridView : DataGridView
    {
        HebrewBibleParser hebrewBibleParser;
        public HebrewGridView()
        {

        }

 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HebrewBibleParser HebrewBibleParser
        {
            set
            {
                hebrewBibleParser = value;
            }
        }

        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            // Ignore this event
            //base.OnCellEnter(e);
        }
        protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left &&
                                e.Clicks == 1 &&
                                e.RowIndex == this.Rows.Count - 1 &&
                                e.ColumnIndex > 0 &&
                                this.SelectedCells.Count > 0)
                {
                    // prepare a list of Selected columns
                    List<int> selctedColumns = new();
                    foreach (DataGridViewCell cell in this.SelectedCells)
                    {
                        if (!selctedColumns.Contains(cell.ColumnIndex))
                            selctedColumns.Add(cell.ColumnIndex);
                    }
                    selctedColumns.Sort();
                    DragData dragData = new DragData();
                    foreach (int column in selctedColumns)
                    {
                        int rowIndex = 0;
                        int columnIndex = 0;
                        string index = string.Empty;
                        string gloss = string.Empty;
                        string word = string.Empty;
                        string lemma = string.Empty;
                        string pos = string.Empty;
                        string morph = string.Empty;
                        string strong = string.Empty;
                        DataGridView source = this;
                        // get data from all rows for the current column
                        for (int i = 0; i < this.RowCount; i++)
                        {
                            rowIndex = i;
                            columnIndex = column;
                            source = this;
                            var cells = this.Rows[i].Cells;
                            string rowTitleS = cells[0].Value as string;
                            RowTitles title;
                            Enum.TryParse<RowTitles>(rowTitleS, true, out title);
                            switch (title)
                            {
                                case RowTitles.IDX:
                                    index = cells[column].Value as string;
                                    break;
                                case RowTitles.ENG:
                                    gloss = cells[column].Value as string;
                                    break;
                                case RowTitles.HEB:
                                    word = cells[column].Value as string;
                                    break;
                                case RowTitles.LEM:
                                    lemma = cells[column].Value as string;
                                    break;
                                case RowTitles.POS:
                                    pos = cells[column].Value as string;
                                    break;
                                case RowTitles.MRF:
                                    morph = cells[column].Value as string;
                                    break;
                                case RowTitles.SRG:
                                    strong = cells[column].Value as string;
                                    break;
                            }
                        }
                        dragData.Add(rowIndex, columnIndex,
                            index, gloss, word, lemma, pos, morph, strong,
                            source);

                    }
                    // add column data to the Drag Data
                    this.DoDragDrop(dragData, DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                var cm = System.Reflection.MethodBase.GetCurrentMethod();
                var name = cm.DeclaringType.FullName + "." + cm.Name;
            }

            base.OnCellMouseDown(e);
        }

        public void Clear()
        {
            this.Rows.Clear();
        }

        public void Update(string reference)
        {
            ParserHebrewVerse verse = hebrewBibleParser.HebrewBible[hebrewBibleParser.referenceIndices[reference]];
            if (verse == null)
                return;

            this.Rows.Clear();
           
            List<string> indices = new();
            List<string> words = new();
            List<string> strongs = new();
            List<string> lemmas = new();
            List<string> pos = new();
            List<string> morphs = new();
            List<string> gloss = new();
            try
            {
                indices.Add(RowTitles.IDX.ToString());
                gloss.Add(RowTitles.ENG.ToString());
                words.Add(RowTitles.HEB.ToString());
                lemmas.Add(RowTitles.LEM.ToString());
                pos.Add(RowTitles.POS.ToString());
                morphs.Add(RowTitles.MRF.ToString());
                strongs.Add(RowTitles.SRG.ToString());

                var tokens = verse.Tokens;
                for (int i = 0; i < tokens.Count; i++)
                {
                    var word = tokens[i];
                    indices.Add(word.index.ToString());
                    gloss.Add(word.gloss);
                    words.Add(word.surface);
                    lemmas.Add(word.lemma);
                    pos.Add(word.pos);
                    morphs.Add(word.morph);
                    strongs.Add(word.strong);
                    ;
                }

                //this.ColumnCount = verseWords.Count;
                this.ColumnCount = indices.Count;

                this.Rows.Add(indices.ToArray());
                this.Rows.Add(gloss.ToArray());
                this.Rows.Add(words.ToArray());
                this.Rows.Add(lemmas.ToArray());
                this.Rows.Add(pos.ToArray());
                this.Rows.Add(morphs.ToArray());
                this.Rows.Add(strongs.ToArray());
            }
            catch (Exception ex)
            {
                var cm = System.Reflection.MethodBase.GetCurrentMethod();
                var name = cm.DeclaringType.FullName + "." + cm.Name;
                //Tracing.TraceException(name, ex.Message);
            }


            this.ClearSelection();

            this.Rows[0].ReadOnly = true;
            this.Rows[1].ReadOnly = true;

        }

}
}
