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

namespace FileCapaApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Err(int type)      //  エラーメッセージ
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

        private void button1_Click(object sender, EventArgs e)          //  参照ボタンクリック
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();        //FolderBrowserDialogクラスのインスタンスを作成
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            fbd.RootFolder = Environment.SpecialFolder.Desktop;            //デフォルトでDesktop
            //ユーザーが新しいフォルダを作成できるようにする
            fbd.ShowNewFolderButton = true;                          //デフォルトでTrue

            if (fbd.ShowDialog(this) == DialogResult.OK)            //ダイアログを表示する
            {
                textBox1.Text = fbd.SelectedPath;                //選択されたフォルダを表示する
            }
        }

        private void button2_Click(object sender, EventArgs e)      //  検索ボタンクリック
        {
            dataGridView1.Rows.Clear();             //  表初期化
            string Path = "";
            int Capacity = 0;
            int count = 0;
            int count2 = 0;
            var diPath = new List<string>();
            Path = textBox1.Text;

            if (Path == "")             //  空白ではないか確認
            {
                Err(1);
            }

            else
            {
                try
                {
                    Capacity = int.Parse(textBox2.Text);        //  int型に変換できるか確認
                }
                catch
                {
                    Err(2);
                }

                try
                {
                    DirectoryInfo DirInfo1 = new DirectoryInfo(Path);
                    count = FileCapa(DirInfo1, Capacity, count);
                    DirectoryInfo DirInfo2 = DirInfo1;

                    while (true)
                    {
                        try
                        {
                            foreach (DirectoryInfo di in DirInfo2.GetDirectories())//フォルダ内のフォルダを取得
                            {
                                diPath.Add(di.FullName);
                                DirectoryInfo DirInfo3 = new DirectoryInfo(di.FullName);
                                count = FileCapa(DirInfo3, Capacity, count);
                            }
                            DirInfo2 = new DirectoryInfo(diPath[count2]);   //  新たなフォルダのパス
                            count2++;
                        }
                        catch
                        {
                            break;
                        }
                    }
                    textBox3.Text = count.ToString();           //  何件あったか表示
                }
                catch
                {
                    Err(1);
                }
            }
        }

        private void csv保存ToolStripMenuItem_Click(object sender, EventArgs e)    //  csv保存ボタンクリック
        {
            SaveFileDialog sa = new SaveFileDialog();            //SaveFileDialogを生成する
            sa.Title = "ファイルを保存する";
            sa.InitialDirectory = @"C:\";
            sa.FileName = @"検索結果.csv";
            sa.FilterIndex = 1;

            DialogResult result = sa.ShowDialog();            //オープンファイルダイアログを表示する

            if (result == DialogResult.OK)          //「保存」ボタンが押された時の処理
            {
                string FilePath = sa.FileName;       // 指定されたファイルのパスが取得

                StreamWriter sw = new StreamWriter(FilePath, false, Encoding.GetEncoding("SHIFT-JIS"));     // CSVファイルオープン
                for (int r = 0; r <= dataGridView1.Rows.Count - 1; r++)
                {
                    for (int c = 0; c <= dataGridView1.Columns.Count - 1; c++)
                    {
                        string hedder = "";
                        string dt = "";
                        if ( (r == 0) && (c == 0) )
                        {
                            for (int h = 0; h <= dataGridView1.Columns.Count - 1; h++)
                            {
                                hedder = dataGridView1.Columns[h].HeaderCell.Value.ToString() + ",";
                                sw.Write(hedder);
                            }
                            sw.Write("\n");
                        }
                        if (dataGridView1.Rows[r].Cells[c].Value != null)                // DataGridViewのセルのデータ取得
                        {
                            dt = dataGridView1.Rows[r].Cells[c].Value.ToString() + ",";
                        }
                        sw.Write(dt);                        // CSVファイル書込
                    }
                    sw.Write("\n");
                }
                sw.Close();                // CSVファイルクローズ
            }
            else if (result == DialogResult.Cancel)      //「キャンセル」ボタンまたは「×」ボタンが選択された時の処理
            {
            }
        }
        private int FileCapa(DirectoryInfo Dir, int Capa, int count)
        {
            foreach (FileInfo fi in Dir.GetFiles())//フォルダ内の全ファイルを取得
            {
                if (fi.Length / 1024 > Capa)        // 設定した値との比較
                {
                    var addValues = new string[]
                    {
                            (count + 1).ToString(), fi.DirectoryName, fi.Name, (fi.Length / 1024).ToString() + "KB"
                    };
                    dataGridView1.Rows.Insert(count, addValues);        //  表に出力
                    count++;
                }
            }
            return count;
        }
    }
}
