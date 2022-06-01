using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FileCapaApp
{
    public partial class FormMain : Form
    {
        private const string TextSearching = "検索中";
        private const string TextIdle = "検索";

        delegate int Delegate(DirectoryInfo dir, int capa, int count);
        public FormMain()
        {
            InitializeComponent();
        }
        private void ShowErrMessage(int type)      //  エラーメッセージ
        {
            string Message = "";
            switch (type)
            {
                case 1:
                    Message = "参照ボタンをクリックしフォルダを選んでください。";
                    break;
                case 2:
                    Message = "容量の欄に正の整数を入力してください。";
                    break;
                default:
                    break;
            }
            MessageBox.Show(Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonSelectFolder_Click(object sender, EventArgs e)          //  参照ボタンクリック
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();        //FolderBrowserDialogクラスのインスタンスを作成
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            fbd.RootFolder = Environment.SpecialFolder.Desktop;            //デフォルトでDesktop
            //ユーザーが新しいフォルダを作成できるようにする
            fbd.ShowNewFolderButton = true;                          //デフォルトでTrue

            if (fbd.ShowDialog(this) == DialogResult.OK)            //ダイアログを表示する
            {
                textBoxFolderName.Text = fbd.SelectedPath;                //選択されたフォルダを表示する
            }
            fbd.Dispose();
        }
        private bool IsSearching
        {
            get; set;
        } = false;

        private void buttonSearch_Click(object sender, EventArgs e)      //  検索ボタンクリック
        {
            if (buttonSearch.Text == TextSearching)                // ファイルを検索中かどうか
            {
                Canceled();
            }
            else
            {
                dataGridView1.Rows.Clear(); //  表初期化
                string Path = "";
                int Capacity = 0;

                if (textBoxFolderName.Text == "")             //  空白ではないか確認
                {
                    ShowErrMessage(1);
                }
                else
                {
                    Path = textBoxFolderName.Text;
                    try
                    {
                        int.Parse(textBoxCapacity.Text);        //  int型に変換できるか確認
                        Capacity = int.Parse(textBoxCapacity.Text);
                        Searcing(Path, Capacity);
                    }
                    catch
                    {
                        ShowErrMessage(2);
                        return;
                    }
                }
            }
        }

        private void csvSaveToolStripMenuItem_Click(object sender, EventArgs e)     // csv保存ボタンクリック
        {
            SaveFileDialog sa = new SaveFileDialog();            //SaveFileDialogを生成する
            sa.Title = "ファイルを保存する";
            sa.InitialDirectory = @"C:\";
            sa.FileName = @"検索結果.csv";
            sa.FilterIndex = 1;

            DialogResult result = sa.ShowDialog();            //オープンファイルダイアログを表示する
            try
                {
                if (result == DialogResult.OK)          //「保存」ボタンが押された時の処理
                {
                    string FilePath = sa.FileName;       // 指定されたファイルのパスが取得

                    using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.GetEncoding("SHIFT-JIS")))     // CSVファイルオープン
                    {
                        for (int r = 0; r <= dataGridView1.Rows.Count - 1; r++)
                        {
                            for (int c = 0; c <= dataGridView1.Columns.Count - 1; c++)
                            {
                                string hedder = "";
                                string dt = "";
                                if ((r == 0) && (c == 0))
                                {
                                    for (int h = 0; h <= dataGridView1.Columns.Count - 1; h++)
                                    {
                                        hedder = dataGridView1.Columns[h].HeaderCell.Value.ToString() + ",";
                                        sw.Write(hedder);
                                    }
                                    sw.Write("\n");
                                }
                                if (dataGridView1.Rows[r].Cells[c].Value != null)                
                                {
                                    dt = dataGridView1.Rows[r].Cells[c].Value.ToString() + ",";         // DataGridViewのセルのデータ取得
                                }
                                sw.Write(dt);                    // CSVファイル書込
                            }
                            sw.Write("\n");
                        }
                        sw.Close();         // CSVファイルクローズ
                    }                 
                }
                else if (result == DialogResult.Cancel)      //「キャンセル」ボタンまたは「×」ボタンが選択された時の処理
                {
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private int StoreinFilesCapacity(DirectoryInfo dir, int capa, int count)
        {
            foreach (FileInfo fi in dir.GetFiles())//フォルダ内の全ファイルを取得
            {
                if (fi.Length / 1024 > capa)        // 設定した値との比較
                {
                    var addValues = new string[]
                    {
                            (count + 1).ToString(), fi.DirectoryName, fi.Name, (fi.Length / 1024).ToString() + "KB"
                    };
                    if (IsSearching == false)
                    {
                        break;
                    }
                    else
                    {
                        dataGridView1.Rows.Insert(count, addValues);        //  表に出力
                        count++; 
                    }
                    
                }
                
            }
            return count;
        }
        private void Canceled()
        {
            IsSearching = false;
        }
        private void Searcing(string path, int capacity)
        {
            int count = 0;
            int count2 = 0;
            var diPath = new List<string>();
            Task.Run( () =>
            {
                IsSearching = true;

                try
                {
                    DirectoryInfo DirInfo1 = new DirectoryInfo(path);
                    Delegate storeinFilesCapacity = StoreinFilesCapacity;
                    count = Convert.ToInt32(Invoke(storeinFilesCapacity, DirInfo1, capacity, count));
                    DirectoryInfo DirInfo2 = DirInfo1;

                    while (IsSearching)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            buttonSearch.Text = TextSearching;   // 検索ボタンの文字を検索中にする
                        }));
                        try
                        {
                            foreach (DirectoryInfo di in DirInfo2.GetDirectories())//フォルダ内のフォルダを取得
                            {
                                diPath.Add(di.FullName);
                                DirectoryInfo DirInfo3 = new DirectoryInfo(di.FullName);

                                count = Convert.ToInt32(Invoke(storeinFilesCapacity, DirInfo3, capacity, count));
                                if (IsSearching == false)
                                {
                                    break;
                                }
                            }
                            DirInfo2 = new DirectoryInfo(diPath[count2]);   //  新たなフォルダのパス
                            count2++;
                        }
                        catch
                        {
                            break;
                        }
                    }
                    Invoke((MethodInvoker)(() =>
                    {
                        textBoxFindDisplay.Text = count.ToString();           //  何件あったか表示
                    }));
                }
                catch
                {
                    ShowErrMessage(1);
                }
                Invoke((MethodInvoker)(() =>
                {
                    buttonSearch.Text = TextIdle;   // 検索ボタンの文字を検索にする
                }));
            });
        }
    }
}
