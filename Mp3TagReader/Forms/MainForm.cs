﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FileExplorer_TreeView;
using Mp3TagReader.com.wikia.lyrics;
using TagLib;

namespace Mp3TagReader
{
    public partial class MainForm : Form
    {
        private FileExplorer fe = new FileExplorer();
        private static string filePath = string.Empty;
        private static string selectedFileName = string.Empty;
        private static string temp = string.Empty;
        private static int selectedRowIndex = 0;
        private static int flag = 0;
        private static List<string> listSelectedFiles = new List<string>();
        private MySortableBindingList<Mp3Info> listMp3Infos = new MySortableBindingList<Mp3Info>();
        private FileInfo[] mp3Files = null;
        private FileInfo[] wmaFiles = null;
        private FileInfo[] flacFiles = null;
        private FileInfo[] m4aFiles = null;

        private FileInfo[] mp3Files_Untagged = null;
        private FileInfo[] wmaFiles_Untagged = null;
        private FileInfo[] flacFiles_Untagged = null;

        private FileInfo[] mp4Files = null;
        private FileInfo[] mpgFiles = null;
        private FileInfo[] wmvFiles = null;
        private FileInfo[] flvFiles = null;
        private FileInfo[] aviFiles = null;

        private String folderListPath = "folderList.txt";

        private WMPLib.IWMPPlaylist pl;
        private WMPLib.IWMPPlaylistArray plItems;
        private WMPLib.WindowsMediaPlayer windowsMediaPlayer = new WMPLib.WindowsMediaPlayer();

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        // Volume control
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;

        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
        IntPtr wParam, IntPtr lParam);

        /// </summary>

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                loadTree();
                gridView.AutoGenerateColumns = false;
                DataGridViewTextBoxColumn columnFilePath = new DataGridViewTextBoxColumn();
                columnFilePath.DataPropertyName = "Path";
                columnFilePath.Name = "ColumnPath";
                columnFilePath.Visible = false;
                DataGridViewTextBoxColumn columnFileName = new DataGridViewTextBoxColumn();
                columnFileName.DataPropertyName = "Name";
                columnFileName.HeaderText = "File Name";
                columnFileName.Width = gridView.Width - 180;
                DataGridViewTextBoxColumn columnFileDate = new DataGridViewTextBoxColumn();
                columnFileDate.DataPropertyName = "CreationTime";
                columnFileDate.HeaderText = "Creation Time";
                columnFileDate.Width = 130;

                gridView.Columns.Add(columnFilePath);
                gridView.Columns.Add(columnFileName);
                gridView.Columns.Add(columnFileDate);

                // load combobox from file
                reloadFolderList(folderListPath);

                // special: for autoplay
                //ListFiles();
                //windowsMediaPlayer.settings.setMode("shuffle", true);
                //_chkPlayAll.Checked = true;
                //PlaySelectedMp3Files();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void reloadFolderList(String folder)
        {
            cbbFilePath.Items.Clear();
            string[] lines = System.IO.File.ReadAllLines(folder);
            foreach (String line in lines)
            {
                cbbFilePath.Items.Add(line);
            }
            cbbFilePath.SelectedIndex = 0;
        }

        private void loadTree()
        {
            fe.CreateTree(this.treeViewFolder);
        }

        private void ClearMp3List()
        {
            try
            {
                gridView.DataSource = null;
                listMp3Infos.Clear();
            }
            catch (Exception) { }
        }

        private void RebuildList()
        {
            try
            {
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
        }

        private FileInfo[] getFileInfo(String filePath, String fileFormat)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(filePath);
                return di.GetFiles(fileFormat, SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void listAudioFiles(String filePath)
        {
            listAudioFiles(filePath, "");
        }

        private void listAudioFiles(String filePath, String searchString)
        {
            DirectoryInfo di = new DirectoryInfo(filePath);
            mp3Files = di.GetFiles("*.mp3", SearchOption.AllDirectories);
            wmaFiles = di.GetFiles("*.wma", SearchOption.AllDirectories);
            flacFiles = di.GetFiles("*.flac", SearchOption.AllDirectories);
            m4aFiles = di.GetFiles("*.m4a", SearchOption.AllDirectories);

            foreach (FileInfo fi in mp3Files)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in wmaFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in flacFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in m4aFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
        }

        private void listVideoFiles(String filePath)
        {
            listVideoFiles(filePath, "");
        }

        private void listVideoFiles(String filePath, String searchString)
        {
            DirectoryInfo di = new DirectoryInfo(filePath);
            mp4Files = di.GetFiles("*.mp4", SearchOption.AllDirectories);
            aviFiles = di.GetFiles("*.avi", SearchOption.AllDirectories);
            mpgFiles = di.GetFiles("*.mpg", SearchOption.AllDirectories);
            flvFiles = di.GetFiles("*.flv", SearchOption.AllDirectories);
            wmvFiles = di.GetFiles("*.wmv", SearchOption.AllDirectories);
            foreach (FileInfo fi in mp4Files)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in aviFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in mpgFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in flvFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
            foreach (FileInfo fi in wmvFiles)
            {
                if (!searchString.Equals(""))
                {
                    if (fi.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }
                else
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }
        }

        private void listFolders(String filePath)
        {
            listFolders(filePath, "");
        }

        private void listFolders(String filePath, String searchString)
        {
            DirectoryInfo di = new DirectoryInfo(filePath);
            DirectoryInfo[] folders = di.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                if (searchString.Equals(""))
                    listMp3Infos.Add(new FolderInfo(folder.FullName, folder.Name));
                else
                {
                    if (folder.Name.ToLower().Contains(searchString.ToLower()))
                        listMp3Infos.Add(new FolderInfo(folder.FullName, folder.Name));

                    // continue to search children folders
                    else
                        loopThroughFolders(folder, searchString);
                }
            }
        }

        private void ListFiles()
        {
            string _temp = string.Empty;
            try
            {
                ClearMp3List();
                _temp = filePath;
                filePath = cbbFilePath.Text;

                listAudioFiles(filePath);
                listVideoFiles(filePath);

                RebuildList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                filePath = _temp;
                cbbFilePath.Text = filePath;
            }
        }

        private void CleanFileNames()
        {
            string result = string.Empty;
            try
            {
                filePath = cbbFilePath.Text;
                DirectoryInfo di = new DirectoryInfo(filePath);
                mp3Files = di.GetFiles("*.mp3");
                wmaFiles = di.GetFiles("*.wma");
                foreach (FileInfo file in mp3Files)
                {
                    if (file.Name.Contains("%20"))
                    {
                        file.MoveTo(filePath + file.Name.Replace("%20", " "));
                        result += file.Name + Environment.NewLine;
                    }
                }
                foreach (FileInfo file in wmaFiles)
                {
                    if (file.Name.Contains("%20"))
                    {
                        file.MoveTo(filePath + file.Name.Replace("%20", " "));
                        result += file.Name + Environment.NewLine;
                    }
                }
                if (result == string.Empty)
                {
                    lblResult.Text = "No need to clean";
                }
                else
                {
                    ListFiles();
                    lblResult.Text = "Cleaned";
                    MessageBox.Show(result);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private Byte[] binArtwork;
        private TagLib.File selectedMp3;

        private void ShowTags(string _path)
        {
            try
            {
                if (!isFolder() && !isVideo())
                {
                    selectedMp3 = TagLib.File.Create(_path);
                    txtArtist.Text = Manipulator.ArrayToString(selectedMp3.Tag.Performers, ",");
                    txtAlbum.Text = selectedMp3.Tag.Album;
                    txtTitle.Text = selectedMp3.Tag.Title;
                    txtTrack.Text = selectedMp3.Tag.Track.ToString();
                    txtYear.Text = selectedMp3.Tag.Year.ToString();
                    txtGenre.Text = Manipulator.ArrayToString(selectedMp3.Tag.Genres, ",");
                    txtBitrate.Text = selectedMp3.Properties.AudioBitrate.ToString() + " kbps";
                    txtLyrics.Text = selectedMp3.Tag.Lyrics;
                    txtComments.Text = selectedMp3.Tag.Comment;

                    // show cover
                http://stackoverflow.com/questions/17904184/using-taglib-to-display-the-cover-art-in-a-image-box-in-wpf
                    TagLib.File file = TagLib.File.Create(_path);
                    if (file.Tag.Pictures.Length >= 1)
                    {
                        binArtwork = (byte[])(file.Tag.Pictures[0].Data.Data);
                        picBxArtwork.Image = Image.FromStream(new MemoryStream(binArtwork)).GetThumbnailImage(picBxArtwork.Height, picBxArtwork.Height, null, IntPtr.Zero);
                    }
                    else
                    {
                        binArtwork = null;
                        picBxArtwork.Image = null;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ShowMultiTags(List<string> fileNames)
        {
            string temp = string.Empty;
            string artistTag = string.Empty;
            string titleTag = string.Empty;
            string albumTag = string.Empty;
            string trackTag = string.Empty;
            string yearTag = string.Empty;
            string genreTag = string.Empty;
            string lyricTag = string.Empty;
            string commentTag = string.Empty;

            TagLib.File tempFile = null;
            try
            {
                tempFile = TagLib.File.Create(fileNames[0]);
                artistTag = Manipulator.ArrayToString(tempFile.Tag.Performers, ",");
                titleTag = tempFile.Tag.Title;
                albumTag = tempFile.Tag.Album;
                trackTag = tempFile.Tag.Track.ToString();
                yearTag = tempFile.Tag.Year.ToString();
                genreTag = Manipulator.ArrayToString(tempFile.Tag.Genres, ",");
                lyricTag = tempFile.Tag.Lyrics;
                commentTag = tempFile.Tag.Comment;
            }
            catch (Exception) { }

            foreach (string filePath in fileNames)
            {
                try
                {
                    TagLib.File tag = TagLib.File.Create(filePath);
                    temp = Manipulator.ArrayToString(tag.Tag.Performers, ",");
                    if (temp != artistTag)
                    {
                        txtArtist.Text = "(multiple values)";
                    }
                    else
                    {
                        txtArtist.Text = temp;
                    }
                    temp = tag.Tag.Album;
                    if (temp != albumTag)
                    {
                        txtAlbum.Text = "(multiple values)";
                    }
                    else
                    {
                        txtAlbum.Text = temp;
                    }
                    temp = tag.Tag.Title;
                    if (temp != titleTag)
                    {
                        txtTitle.Text = "(multiple values)";
                    }
                    else
                    {
                        txtTitle.Text = temp;
                    }
                    temp = tag.Tag.Track.ToString();
                    if (temp != trackTag)
                    {
                        txtTrack.Text = "(multiple values)";
                    }
                    else
                    {
                        txtTrack.Text = temp;
                    }
                    temp = tag.Tag.Year.ToString();
                    if (temp != yearTag)
                    {
                        txtYear.Text = "(multiple values)";
                    }
                    else
                    {
                        txtYear.Text = temp;
                    }
                    temp = Manipulator.ArrayToString(tag.Tag.Genres, ",");
                    if (temp != genreTag)
                    {
                        txtGenre.Text = "(multiple values)";
                    }
                    else
                    {
                        txtGenre.Text = temp;
                    }
                    temp = tag.Tag.Lyrics;
                    if (temp != lyricTag)
                    {
                        txtLyrics.Text = "(multiple values)";
                    }
                    else
                    {
                        txtLyrics.Text = temp;
                    }
                    temp = tag.Tag.Comment;
                    if (temp != commentTag)
                    {
                        txtComments.Text = "(multiple values)";
                    }
                    else
                    {
                        txtComments.Text = temp;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count == 1)
            {
                try
                {
                    TagLib.File mp3 = TagLib.File.Create(selectedFileName);
                    mp3.Tag.Performers = new string[] { txtArtist.Text };
                    mp3.Tag.Album = txtAlbum.Text;
                    mp3.Tag.Title = txtTitle.Text;
                    mp3.Tag.Track = Convert.ToUInt32(txtTrack.Text);
                    mp3.Tag.Year = Convert.ToUInt32(txtYear.Text);
                    mp3.Tag.Genres = new string[] { txtGenre.Text };
                    mp3.Tag.Lyrics = txtLyrics.Text;
                    mp3.Tag.Comment = txtComments.Text;
                    mp3.Save();
                    lblResult.Text = "Saved";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                foreach (string filePath in listSelectedFiles)
                {
                    TagLib.File mp3 = TagLib.File.Create(filePath);
                    if (txtArtist.Text != "(multiple values)")
                    {
                        mp3.Tag.Performers = new string[] { txtArtist.Text };
                    }
                    if (txtAlbum.Text != "(multiple values)")
                    {
                        mp3.Tag.Album = txtAlbum.Text;
                    }
                    if (txtTitle.Text != "(multiple values)")
                    {
                        mp3.Tag.Title = txtTitle.Text;
                    }
                    if (txtTrack.Text != "(multiple values)")
                    {
                        mp3.Tag.Track = Convert.ToUInt32(txtTrack.Text);
                    }
                    if (txtYear.Text != "(multiple values)")
                    {
                        mp3.Tag.Year = Convert.ToUInt32(txtYear.Text);
                    }
                    if (txtGenre.Text != "(multiple values)")
                    {
                        mp3.Tag.Genres = new string[] { txtGenre.Text };
                    }
                    if (txtLyrics.Text != "(multiple values)")
                    {
                        mp3.Tag.Lyrics = txtLyrics.Text;
                    }
                    if (txtComments.Text != "(multiple values)")
                    {
                        mp3.Tag.Comment = txtComments.Text;
                    }
                    mp3.Save();
                    lblResult.Text = "Saved";
                }
            }
        }

        private void gridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                lblArtwork.Text = string.Empty;
                lblResult.Text = gridView.SelectedRows.Count.ToString() + " selected";
                if (gridView.SelectedRows.Count == 1)
                {
                    selectedRowIndex = gridView.CurrentRow.Index;
                    DisplaySelectedFileInfo();

                    // add to list for setting artwork later
                    listSelectedFiles.Clear();
                    listSelectedFiles.Add(selectedFileName);
                }
                else
                {
                    DisplayMultiSelectedFilesInfo();
                }
                changeCoverFlag = false;
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DisplaySelectedFileInfo()
        {
            try
            {
                if (gridView.CurrentRow.Index != selectedRowIndex || gridView.CurrentRow.Index == 0)
                {
                    selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
                    ShowTags(selectedFileName);
                    lblResult.Text = "";
                }
                else { }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DisplayMultiSelectedFilesInfo()
        {
            string selectedPath = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
            if (isFolder() || isVideo())
            {
                //
            }
            else
            {
                listSelectedFiles.Clear();
                foreach (DataGridViewRow row in gridView.SelectedRows)
                {
                    listSelectedFiles.Add(row.Cells["ColumnPath"].Value.ToString());
                }
                ShowMultiTags(listSelectedFiles);
            }
        }

        private void gridView_SelectionChanged(object sender, EventArgs e)
        {
            DisplaySelectedFileInfo();
        }

        //http://forum.codecall.net/csharp-tutorials/20420-tutorial-playing-mp3-files-c.html#post199646
        //http://www.codeproject.com/Articles/14709/Playing-MP3s-using-MCI
        private void PlaySelectedMp3Files()
        {
            try
            {
                selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
                if (Manipulator.isVideoFile(selectedFileName) && _btnList.Enabled == false)
                {
                    System.Diagnostics.Process.Start(selectedFileName);
                }
                else
                {
                    //mciSendString("open \"" + selectedFileName + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
                    //mciSendString("play MediaFile" + playMode, null, 0, IntPtr.Zero);

                    if (windowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        if (_chkReplay.Checked == true) // loop
                        {
                            windowsMediaPlayer.settings.setMode("loop", true);
                        }
                        else if (_chkShuffle.Checked == true)
                        {
                            windowsMediaPlayer.settings.setMode("shuffle", true);
                        }
                        else
                        {
                            windowsMediaPlayer.settings.setMode("loop", false);
                            windowsMediaPlayer.settings.setMode("shuffle", false);
                        }
                    }
                    else
                    {
                        string myPlaylist = "MyPlayList";

                        plItems = windowsMediaPlayer.playlistCollection.getByName(myPlaylist);

                        pl = windowsMediaPlayer.playlistCollection.newPlaylist(myPlaylist);

                        int index = getSongIndex(selectedFileName);

                        string fi = string.Empty;
                        if (_chkPlayAll.Checked == true) // play all songs in list
                        {
                            for (int i = index; i <= gridView.Rows.Count - 1; i++)
                            {
                                fi = gridView.Rows[i].Cells["ColumnPath"].Value.ToString();
                                if (Manipulator.isAudioFile(fi))
                                {
                                    WMPLib.IWMPMedia m1 = windowsMediaPlayer.newMedia(fi);
                                    pl.appendItem(m1);
                                }
                            }
                        }
                        else // play selected songs
                        {
                            foreach (DataGridViewRow row in gridView.SelectedRows)
                            {
                                fi = row.Cells["ColumnPath"].Value.ToString();
                                if (Manipulator.isAudioFile(fi))
                                {
                                    WMPLib.IWMPMedia m1 = windowsMediaPlayer.newMedia(fi);
                                    pl.appendItem(m1);
                                }
                            }
                        }

                        if (_chkReplay.Checked == true)
                        {
                            windowsMediaPlayer.settings.setMode("loop", true);
                        }

                        if (_chkShuffle.Checked == true)
                        {
                            windowsMediaPlayer.settings.setMode("shuffle", true);
                        }

                        windowsMediaPlayer.currentPlaylist = pl;
                        windowsMediaPlayer.controls.play();
                        _btnPlayAll.Text = "Pause";

                        timerNowPlayingText.Enabled = true;

                        this.Text = "Now Playing - " + Path.GetFileNameWithoutExtension(selectedFileName) + " | ";
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void _btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isFolder())
                {
                    //mciSendString("close MediaFile", null, 0, IntPtr.Zero);
                    _btnPlayAll.Text = "Play";
                }

                windowsMediaPlayer.controls.stop();
                windowsMediaPlayer.close();

                this.Text = "Mp3TagReader";

                timerNowPlayingText.Enabled = false;
                _chkPlayAll.Enabled = true;

                // progress bar
                setProgressBar(0);
            }
            catch
            {
                MessageBox.Show("No song in list", "Warning");
                return;
            }
        }

        private void _btnList_Click(object sender, EventArgs e)
        {
            ListFiles();
        }

        private void _btnSearch_Click(object sender, EventArgs e)
        {
            if (_btnList.Enabled == true)
            {
                _btnList.Enabled = false;
                _btnUntagged.Enabled = true;
                _btnSearch.Text = "Search ON";
                cbbFilePath.Focus();
                cbbFilePath.Text = "";
            }
            else
            {
                _btnList.Enabled = true;
                _btnUntagged.Enabled = true;
                _btnSearch.Text = "Search OFF";
                cbbFilePath.Focus();
            }
        }

        private void loopThroughFolders(DirectoryInfo folder, string searchString)
        {
            DirectoryInfo[] folders = folder.GetDirectories();

            if (folders.Length > 0)
            {
                foreach (DirectoryInfo folderTemp in folders)
                {
                    if (folderTemp.Name.ToLower().Contains(searchString.ToLower()))
                    {
                        listMp3Infos.Add(new FolderInfo(folderTemp.FullName, folderTemp.Name));
                    }
                    else
                    {
                        loopThroughFolders(folderTemp, searchString);
                    }
                }
            }
        }

        private void txtLyrics_DoubleClick(object sender, EventArgs e)
        {
            if (txtLyrics.Text != string.Empty)
            {
                LyricsViewer lv = new LyricsViewer(txtArtist.Text, txtTitle.Text, txtLyrics.Text);
                lv.Show();
            }
        }

        private void chkReplay_CheckedChanged(object sender, EventArgs e)
        {
            if (windowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                if (_chkReplay.Checked == true) // loop
                {
                    windowsMediaPlayer.settings.setMode("loop", true);
                }
                else
                {
                    windowsMediaPlayer.settings.setMode("loop", false);
                }
            }
        }

        private void _btnUntagged_Click(object sender, EventArgs e)
        {
            TagLib.File audio;
            ClearMp3List();
            try
            {
                foreach (FileInfo fi in mp3Files)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("zing") || audio.Tag.Album.ToLower().Contains(".com") || audio.Tag.Album.ToLower().Contains(".vn") || audio.Tag.Album.ToLower().Contains(".org"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                    if (audio.Tag.Album == null || Manipulator.ArrayToString(audio.Tag.Performers, ",") == string.Empty || Manipulator.ArrayToString(audio.Tag.Performers, ",") == string.Empty || audio.Tag.Title == string.Empty)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                    }
                }
                foreach (FileInfo fi in wmaFiles)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("zing"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                    if (audio.Tag.Album == null || Manipulator.ArrayToString(audio.Tag.Performers, ",") == string.Empty || Manipulator.ArrayToString(audio.Tag.Performers, ",") == string.Empty || audio.Tag.Title == string.Empty)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                    }
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (CorruptFileException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("No song in list", "Warning");
                return;
            }
        }

        private void _btnPlayList_Click(object sender, EventArgs e)
        {
            if (listMp3Infos.Count == 0)
            {
                MessageBox.Show("No song in list", "Warning");
                return;
            }

            if (_btnPlayAll.Text == "Play")
            {
                PlaySelectedMp3Files();
                _chkPlayAll.Enabled = false;
            }
            else if (_btnPlayAll.Text == "Pause")
            {
                windowsMediaPlayer.controls.pause();
                _btnPlayAll.Text = "Resume";
                timerNowPlayingText.Enabled = false;
            }
            else if (_btnPlayAll.Text == "Resume")
            {
                windowsMediaPlayer.controls.play();
                _btnPlayAll.Text = "Pause";
                timerNowPlayingText.Enabled = true;
            }
        }

        private void gridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // open folder
            try
            {
                selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
                if (isFolder())
                {
                    ClearMp3List();
                    string searchString = cbbFilePath.Text;

                    listFolders(selectedFileName);
                    listAudioFiles(selectedFileName);
                    listVideoFiles(selectedFileName);

                    gridView.DataSource = listMp3Infos;
                    _lblCount.Text = gridView.RowCount.ToString() + " results";
                }
            }
            catch (Exception) { }
        }

        private bool isFolder()
        {
            string selectedPath = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
            return Manipulator.isFolder(selectedPath);
        }

        private bool isVideo()
        {
            string selectedPath = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
            return Manipulator.isVideoFile(selectedPath);
        }

        private string folderOf(string filePath) // return folder of the selected file
        {
            selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
            return Path.GetDirectoryName(selectedFileName);
        }

        private void lblAlbum_Click(object sender, EventArgs e)
        {
            if (txtAlbum.Text == string.Empty || txtAlbum.Text.Contains("zing") || txtAlbum.Text.Contains("."))
            {
                txtAlbum.Text = "Unknown";
            }
            else
            {
                txtAlbum.Text = txtTitle.Text + " (Single)";
            }
        }

        private string fileNameOf()
        {
            string fileName = string.Empty;
            fileName = Path.GetFileName(selectedFileName);
            return fileName;
        }

        private string getArtistFromFileName(string fileName)
        {
            string artistName = string.Empty;
            int index = 0;
            index = fileName.LastIndexOf(".");
            if (index > 0)
            {
                artistName = fileName.Substring(0, index);
            }
            index = artistName.LastIndexOf("-");
            if (index > 0)
            {
                artistName = artistName.Substring(index + 1).Trim();
            }
            return artistName;
        }

        private string getTitleFromFileName(string fileName)
        {
            string title = string.Empty;
            int index = 0;
            index = fileName.LastIndexOf(".");
            if (index > 0)
            {
                title = fileName.Substring(0, index);
            }
            index = title.LastIndexOf("-");
            if (index > 0)
            {
                title = title.Substring(0, index).Trim();
            }
            return title;
        }

        private void lblComments_Click(object sender, EventArgs e)
        {
            txtComments.Text = string.Empty;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void fix20ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CleanFileNames();
        }

        private void lblArtist_Click(object sender, EventArgs e)
        {
            if (flag == 0)
            {
                string artistName = string.Empty;
                string fileName = fileNameOf();
                artistName = getArtistFromFileName(fileName);
                txtArtist.Text = artistName;
                flag = 1;
            }
            else
            {
                string artistName = string.Empty;
                string fileName = fileNameOf();
                artistName = getTitleFromFileName(fileName);
                txtArtist.Text = artistName;
                flag = 0;
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {
            if (flag == 0)
            {
                string title = string.Empty;
                string fileName = fileNameOf();
                title = getTitleFromFileName(fileName);
                txtTitle.Text = title;
                flag = 1;
            }
            else
            {
                string title = string.Empty;
                string fileName = fileNameOf();
                title = getArtistFromFileName(fileName);
                txtTitle.Text = title;
                flag = 0;
            }
        }

        // http://samuelhaddad.com/2009/03/22/c-net-and-lyricwiki-to-lookup-lyrics/
        private void _btnGetLyrics_Click(object sender, EventArgs e)
        {
            Thread oThread = new Thread(new ThreadStart(getLyrics));
            oThread.Start();
        }

        private void getLyrics()
        {
            LyricWiki wiki = new LyricWiki();
            LyricsResult result = new LyricsResult();
            string artist = txtArtist.Text;
            string title = txtTitle.Text;
            if (wiki.checkSongExists(artist, title))
            {
                result = wiki.getSong(artist, title);
                Process.Start(result.url);

                //Encoding iso8859 = Encoding.GetEncoding("ISO-8859-1");
                //txtLyrics.Text = Encoding.UTF8.GetString(iso8859.GetBytes(result.lyrics));
            }
            else
            {
                MessageBox.Show("Lyrics not found");

                Searcher searchLyric = new Searcher(title + " " + artist);
                searchLyric.Show();
            }
        }

        private void _btnInfo_Click(object sender, EventArgs e)
        {
            FullInfo info = new FullInfo(selectedFileName);
            info.Show();
        }

        private void timerSong_Tick(object sender, EventArgs e)
        {
            try
            {
                if (windowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsStopped)
                {
                    _btnStop_Click(null, null);
                    progressBar.Value = (int)0;
                }
                else
                {
                    if (_btnPlayAll.Text == "Pause")
                    {
                        int songIndex = 0;
                        string currentSong = windowsMediaPlayer.controls.currentItem.sourceURL;
                        if (currentSong != selectedFileName) // next song
                        {
                            selectedFileName = currentSong;

                            songIndex = getSongIndex(currentSong);

                            //gridView.CurrentRow.Selected = false;
                            //gridView.Rows[songIndex - 1].Selected = false;
                            // clear other selected rows
                            gridView.ClearSelection();
                            gridView.Rows[songIndex].Selected = true;

                            ShowTags(selectedFileName);

                            this.Text = "Now Playing - " + Path.GetFileNameWithoutExtension(currentSong) + " | ";
                        }

                        setProgressBar(1);
                    }
                }

                // notification after copied to clipboard, display "copied" then change back to number of files
                if (_lblCount.Text != "Copied")
                {
                    temp = _lblCount.Text;
                }
                else
                {
                    _lblCount.Text = temp;
                }
            }
            catch { }
        }

        private int getSongIndex(string path)
        {
            int index = 0;
            try
            {
                foreach (DataGridViewRow row in gridView.Rows)
                {
                    if (row.Cells["ColumnPath"].Value.ToString() == path)
                    {
                        index = row.Index;
                    }
                }
            }
            catch
            {
                MessageBox.Show("No song in list", "Warning");
                index = 0;
            }
            return index;
        }

        private void CopyFiles(List<string> ListToCopy)
        {
            if (ListToCopy.Count != 0)
            {
                System.Collections.Specialized.StringCollection FileCollection = new System.Collections.Specialized.StringCollection();

                foreach (string FileToCopy in ListToCopy)
                {
                    FileCollection.Add(FileToCopy);
                }

                if (FileCollection != null)
                {
                    Clipboard.SetFileDropList(FileCollection);
                }
            }
        }

        private void _btnCopy_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count == 1)
            {
                selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
                listSelectedFiles.Clear();
                listSelectedFiles.Add(selectedFileName);
            }

            if (listSelectedFiles.Count != 0)
            {
                CopyFiles(listSelectedFiles);
                _lblCount.Text = "Copied";
            }
            else
            {
                MessageBox.Show("Nothing to copy", "Warning");
            }
        }

        private void move128KbpsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Please modify the code", "Warning");
            //return;
            //string fromFolder = @"D:\Test\music";
            //string toFolder = @"D:\Test\128";
            //move128kbps(fromFolder, toFolder);
        }

        private void move128kbps(String fromFolder, String toFolder)
        {
            if (fromFolder == string.Empty || toFolder == string.Empty)
            {
                MessageBox.Show("fromFolder is not set", "Warning");
                return;
            }

            DirectoryInfo di = new DirectoryInfo(fromFolder);
            mp3Files = di.GetFiles("*.mp3", SearchOption.AllDirectories);
            wmaFiles = di.GetFiles("*.wma", SearchOption.AllDirectories);
            TagLib.File audio;
            try
            {
                foreach (FileInfo fi in mp3Files)
                {
                    audio = TagLib.File.Create(fi.FullName);
                    if (audio.Properties.AudioBitrate <= 128)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        fi.MoveTo(toFolder + @"\" + fi.Name);
                    }
                }
                foreach (FileInfo fi in wmaFiles)
                {
                    audio = TagLib.File.Create(fi.FullName);
                    if (audio.Properties.AudioBitrate <= 128)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        fi.MoveTo(toFolder + @"\" + fi.Name);
                    }
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        private void list128KbpsFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(cbbFilePath.Text);
            mp3Files = di.GetFiles("*.mp3", SearchOption.AllDirectories);
            wmaFiles = di.GetFiles("*.wma", SearchOption.AllDirectories);
            TagLib.File audio;
            try
            {
                foreach (FileInfo fi in mp3Files)
                {
                    audio = TagLib.File.Create(fi.FullName);
                    if (audio.Properties.AudioBitrate <= 128)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                    }
                }
                foreach (FileInfo fi in wmaFiles)
                {
                    audio = TagLib.File.Create(fi.FullName);
                    if (audio.Properties.AudioBitrate <= 128)
                    {
                        listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                    }
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.cbbFilePath.Text = folderBrowserDialog.SelectedPath;
                filePath = cbbFilePath.Text;
            }
            else { }
        }

        private void timerNowPlaying_Tick(object sender, EventArgs e)
        {
            if (this.Text != "Mp3TagReader")
            {
                string sScrollText = this.Text;
                sScrollText = sScrollText.Substring(1,
                    sScrollText.Length - 1) + sScrollText.Substring(0, 1);
                this.Text = sScrollText;
            }
        }

        private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                gridView.Rows[0].Selected = true;
            }
            catch { }
        }

        private void _btnClearLyrics_Click(object sender, EventArgs e)
        {
            try
            {
                txtLyrics.Clear();
            }
            catch { }
        }

        private void browseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ClearMp3List();
                listMp3Infos.Add(new Mp3Info(openFileDialog.FileName, openFileDialog.SafeFileName, System.IO.File.GetCreationTime(openFileDialog.FileName)));
                gridView.DataSource = listMp3Infos;
            }
            else { }
        }

        private void deleteFlacFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearMp3List();

            if (_txtExtensionToRemove.Text.Contains("File"))
            {
                MessageBox.Show("Input file extension first", "Warning");
                _txtExtensionToRemove.Focus();
                return;
            }

            filePath = cbbFilePath.Text;
            DirectoryInfo di = new DirectoryInfo(filePath);
            flacFiles = di.GetFiles("*." + _txtExtensionToRemove.Text, SearchOption.AllDirectories);

            foreach (FileInfo fi in flacFiles)
            {
                listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
            }

            gridView.DataSource = listMp3Infos;
            _lblCount.Text = gridView.RowCount.ToString() + " files";

            if (gridView.RowCount == 0)
            {
                MessageBox.Show("Nothing to remove", "Warning");
                return;
            }

            if (MessageBox.Show("Will you really remove " + _lblCount.Text + " items in " + filePath + " ?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                foreach (FileInfo fi in flacFiles)
                {
                    System.IO.File.Delete(fi.FullName);
                }

                MessageBox.Show("Success", "Warning");

                flacFiles = di.GetFiles("*." + _txtExtensionToRemove.Text, SearchOption.AllDirectories);

                foreach (FileInfo fi in flacFiles)
                {
                    listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                }

                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            else { }
        }

        private void _btnPrev_Click(object sender, EventArgs e)
        {
            if (windowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                MessageBox.Show("Not Playing", "Warning");
                return;
            }

            try
            {
                windowsMediaPlayer.controls.previous();
            }
            catch { }
        }

        private void _btnNext_Click(object sender, EventArgs e)
        {
            if (windowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                MessageBox.Show("Not Playing", "Warning");
                return;
            }

            try
            {
                windowsMediaPlayer.controls.next();
            }
            catch { }
        }

        private void gridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void gridView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith("jpg") || files[0].EndsWith("png"))
            {
                imagePath = files[0];
                definePicture(files[0]);
            }
            else
            {
                try
                {
                    gridView.DataSource = null;
                }
                catch (Exception) { }
                foreach (string file in files)
                {
                    listMp3Infos.Add(new Mp3Info(file, System.IO.Path.GetFileName(file), System.IO.File.GetCreationTime(file)));
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
        }

        private void searchZINGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Searcher zs = new Searcher("");
            zs.Show();
        }

        private void _chkShuffle_CheckedChanged(object sender, EventArgs e)
        {
            if (_chkShuffle.Checked == true)
            {
                windowsMediaPlayer.settings.setMode("shuffle", true);
                _chkPlayAll.Checked = true;
            }
            else
            {
                windowsMediaPlayer.settings.setMode("shuffle", false);
            }
        }

        private TreeNode m_OldSelectNode;

        private void treeViewFolder_MouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                TreeNode node = treeViewFolder.GetNodeAt(p);
                if (node != null)
                {
                    // Select the node the user has clicked.
                    // The node appears selected until the menu is displayed on the screen.
                    m_OldSelectNode = treeViewFolder.SelectedNode;
                    treeViewFolder.SelectedNode = node;

                    contextMenuFolder.Show(treeViewFolder, p);

                    // Highlight the selected node.
                    treeViewFolder.SelectedNode = m_OldSelectNode;
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                cbbFilePath.Text = m_OldSelectNode.FullPath;
                updateSelectedFilePath(cbbFilePath.Text);
                ListFiles();
            }
            catch (Exception) { }
        }

        private void treeViewFolder_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes[0].Text == "")
            {
                TreeNode node = fe.EnumerateDirectory(e.Node);
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Process.Start("explorer.exe", @"/select, " + m_OldSelectNode.FullPath);
            if (Path.HasExtension(m_OldSelectNode.FullPath))
            {
                Process.Start(Path.GetDirectoryName(m_OldSelectNode.FullPath));
            }
            else
            {
                Process.Start(m_OldSelectNode.FullPath);
            }
        }

        private void _btnDisableTimerSong_Click(object sender, EventArgs e)
        {
            if (timerSong.Enabled == true)
            {
                timerSong.Enabled = false;
                _btnDisableTimerSong.Text = "Enable Auto Find Song";
            }
            else if (timerSong.Enabled == false)
            {
                timerSong.Enabled = true;
                _btnDisableTimerSong.Text = "Disable Auto Find Song";
            }
        }

        private void lblTrack_Click(object sender, EventArgs e)
        {
            int trackNo = 0;
            try
            {
                trackNo = Convert.ToInt32(Path.GetFileName(selectedFileName).Substring(0, 2));
                txtTrack.Text = trackNo.ToString();
            }
            catch (Exception) { }

            if (trackNo == 0)
            {
                if (txtTrack.Text == "0" || txtTrack.Text == string.Empty)
                {
                    txtTrack.Text = "1";
                }
                else
                {
                    txtTrack.Text = (Convert.ToInt32(txtTrack.Text) + 1).ToString();
                }
            }
        }

        private void lblYear_Click(object sender, EventArgs e)
        {
            if (txtYear.Text == "0" || txtYear.Text == string.Empty)
            {
                txtYear.Text = DateTime.Now.Year.ToString();
            }
            else
            {
                txtYear.Text = (Convert.ToInt32(txtYear.Text) - 1).ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TagLib.File audio;
            ClearMp3List();
            try
            {
                foreach (FileInfo fi in mp3Files)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || audio.Tag.Title.ToLower().Contains("/"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                }
                foreach (FileInfo fi in wmaFiles)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || audio.Tag.Title.ToLower().Contains("/"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (CorruptFileException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        private void checkDuplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TagLib.File audio;
            ClearMp3List();
            try
            {
                foreach (FileInfo fi in mp3Files)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains(",") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || audio.Tag.Title.ToLower().Contains("/"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                }
                foreach (FileInfo fi in wmaFiles)
                {
                    audio = TagLib.File.Create(fi.FullName, ReadStyle.None);
                    if (audio.Tag.Album != null)
                    {
                        if (audio.Tag.Album.ToLower().Contains("/") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains(",") || Manipulator.ArrayToString(audio.Tag.Performers, ",").ToLower().Contains("/") || audio.Tag.Title.ToLower().Contains("/"))
                        {
                            listMp3Infos.Add(new Mp3Info(fi.FullName, fi.Name, System.IO.File.GetCreationTime(fi.FullName)));
                        }
                    }
                }
                gridView.DataSource = listMp3Infos;
                _lblCount.Text = gridView.RowCount.ToString() + " files";
            }
            catch (CorruptFileException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        private void btnVolumeUp_Click(object sender, EventArgs e)
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
            (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        private void btnVolumeDown_Click(object sender, EventArgs e)
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
            (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
            (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }

        // right click at grid
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
            System.Diagnostics.Process.Start(selectedFileName);
        }

        private void gridView_MouseDown(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // get the row index in selected point
                int rowIndex = gridView.HitTest(e.X, e.Y).RowIndex;

                if (rowIndex == -1)
                {
                    return;
                }

                // clear other selected rows
                gridView.ClearSelection();

                gridView.Rows[rowIndex].Selected = true;
                Point p = new Point(e.X, e.Y);
                contextGrid.Show(gridView, p);
            }
        }

        // open folder right click
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                // no need
                //selectedFileName = gridView.CurrentRow.Cells["ColumnPath"].Value.ToString();
                if (isFolder())
                {
                    Process.Start(selectedFileName);
                }
                else
                {
                    Process.Start("explorer.exe", @"/select, " + selectedFileName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No song in list", "Warning");
                cbbFilePath.Text = filePath;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    int i = gridView.CurrentRow.Index;
            //    listMp3Infos.RemoveAt(i);
            //    RebuildList();
            //}
            //catch (NullReferenceException ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    return;
            //}
        }

        private void setProgressBar(int now)
        {
            if (now == 1)
            {
                // progress bar
                double duration = windowsMediaPlayer.currentMedia.duration;
                double position = windowsMediaPlayer.controls.currentPosition;

                progressBar.Minimum = 0;
                progressBar.Maximum = (int)duration;
                progressBar.Value = (int)position;

                // time playing
                double t = Math.Floor(position);
                double s = Math.Floor(duration);
                int minT = (Int32)(t / 60);
                int secT = (Int32)(t % 60);
                int minS = (Int32)(s / 60);
                int secS = (Int32)(s % 60);
                labelTimerSong.Text = minT + ":" + String.Format("{0:00}", secT) + " / " + minS + ":" + String.Format("{0:00}", secS);
            }
            else if (now == 0)
            {
                progressBar.Value = 0;
                labelTimerSong.Text = "0:00";
            }
        }

        private void progressBar_Click(object sender, EventArgs e)
        {
            // set the progress bar's value based on mouse position
            progressBar.Value = (((MouseEventArgs)e).X) * progressBar.Maximum / progressBar.Width;

            // set the media's position
            windowsMediaPlayer.controls.currentPosition = progressBar.Value;
        }

        private void _btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (isFolder())
                    Process.Start(selectedFileName);
                else
                    Process.Start("explorer.exe", @"/select, " + selectedFileName);
            }
            catch (Exception)
            {
                MessageBox.Show("No song in list", "Warning");
                cbbFilePath.Text = filePath;
            }
        }

        // before rename
        private string beforeRenamed = "";

        private void gridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            beforeRenamed = gridView.CurrentCell.Value.ToString();
            renamePath = selectedFileName;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView.ReadOnly == true)
            {
                gridView.ReadOnly = false;
            }
        }

        // after rename
        private string renamePath = "";

        private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string afterRenamed = gridView.CurrentCell.Value.ToString();
            if (afterRenamed != beforeRenamed)
            {
                if (MessageBox.Show("Modify file name?", "Info", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    System.IO.File.Move(renamePath, Path.GetDirectoryName(renamePath) + "\\" + afterRenamed);
                    gridView.ReadOnly = true;
                    gridView.RefreshEdit();

                    gridView.CurrentRow.Cells["ColumnPath"].Value = Path.GetDirectoryName(renamePath) + "\\" + afterRenamed; // prevent file not found exception after renamed
                }
                else
                {
                    gridView.CurrentCell.Value = beforeRenamed;
                }
            }
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearMp3List();
        }

        private void fixTrackNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 0;
            int trackNo = 0;
            String path = string.Empty;
            try
            {
                foreach (DataGridViewRow row in gridView.Rows)
                {
                    try
                    {
                        path = row.Cells["ColumnPath"].Value.ToString();
                        trackNo = Convert.ToInt32(Path.GetFileName(path).Substring(0, 2));
                        if (trackNo != 0)
                        {
                            TagLib.File mp3 = TagLib.File.Create(path);
                            mp3.Tag.Track = Convert.ToUInt32(trackNo);
                            mp3.Save();
                        }

                        count++;
                    }
                    catch (Exception) { }
                }

                MessageBox.Show(count + " Tracks Updated", "Information");
            }
            catch
            {
            }
        }

        private void addToContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AddToContext.Add(), "Information");
        }

        private void removeFromContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AddToContext.Remove(), "Information");
        }

        private DirectoryInfo[] folders = null;
        private DirectoryInfo[] folders2 = null;

        private void cbbFilePath_TextChanged(object sender, EventArgs e)
        {
            if (_btnList.Enabled == false)
            {
                string searchString = cbbFilePath.Text;
                ClearMp3List();
                int count = 0;
                try
                {
                    count = mp3Files.Length;
                }
                catch
                {
                    count = 0;
                }
                if (searchString.Length == 0 && count < 2000)
                {
                    listAudioFiles(selectedFilePath);
                    listVideoFiles(selectedFilePath);
                    listFolders(selectedFilePath);
                }
                else
                {
                    // folder search results
                    listFolders(selectedFilePath, searchString);
                    // audio search results
                    listAudioFiles(selectedFilePath, searchString);
                    // video search results
                    listVideoFiles(selectedFilePath, searchString);
                }

                // check to prevent null exception
                if (listMp3Infos.Count > 0)
                {
                    gridView.DataSource = listMp3Infos;
                    _lblCount.Text = gridView.RowCount.ToString() + " results";
                }
            }
        }

        private void picBxArtwork_Click(object sender, EventArgs e)
        {
            if (binArtwork != null)
            {
                Image image = Image.FromStream(new MemoryStream(binArtwork)).GetThumbnailImage(500, 500, null, IntPtr.Zero);
                ArtworkViewer av = new ArtworkViewer(image);
                av.ShowDialog();
            }
        }

        private List<TagLib.File> ls = new List<TagLib.File>();
        private String imagePath = "1";
        private Boolean unloadArtwork = false;
        private Boolean changeCoverFlag = false;

        //http://stackoverflow.com/questions/13667378/embed-album-art-in-mp3-using-tag-lib-c-sharp

        private void btnChangeCover_Click(object sender, EventArgs e)
        {
            unloadArtwork = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;

                // define picture
                definePicture(imagePath);
            }
            else { }
        }

        private void btnSaveCover_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridView.SelectedRows.Count >= 1)
                {
                    if (changeCoverFlag == true)
                    {
                        // define picture
                        TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                        if (!imagePath.Equals(string.Empty))
                        {
                            pic.TextEncoding = TagLib.StringType.Latin1;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.Data = TagLib.ByteVector.FromPath(imagePath);
                        }

                        foreach (String selectedFile in listSelectedFiles)
                        {
                            TagLib.File mp3 = TagLib.File.Create(selectedFile);

                            // save or unload picture to file
                            if (!unloadArtwork && !imagePath.Equals(string.Empty))
                            {
                                mp3.Tag.Pictures = new TagLib.IPicture[1] { pic };
                            }
                            else
                            {
                                mp3.Tag.Pictures = null;
                            }

                            mp3.Save();
                            changeCoverFlag = false; // void the next Save attempt
                        }
                    }
                    else
                    {
                        lblArtwork.Text = "No cover to change";
                    }
                }

                if (picBxArtwork.Image != null)
                {
                    lblArtwork.Text = listSelectedFiles.Count + " artwork(s) set successfully";
                }
                else
                {
                    binArtwork = null;
                    lblArtwork.Text = listSelectedFiles.Count + " artwork(s) unset successfully";
                }
            }
            catch (Exception) { }
        }

        private void btnUnloadCover_Click(object sender, EventArgs e)
        {
            imagePath = string.Empty;
            unloadArtwork = true;
            picBxArtwork.Image = null;
            changeCoverFlag = true;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("folderList.txt");
        }

        private void btnReloadList_Click(object sender, EventArgs e)
        {
            reloadFolderList(folderListPath);
        }

        private String selectedFilePath = "";

        private void updateSelectedFilePath(String value)
        {
            selectedFilePath = value;
            lblSelectedFilePath.Text = value;
        }

        private void cbbFilePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateSelectedFilePath(cbbFilePath.Text);
        }

        private void picBxArtwork_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void picBxArtwork_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // define picture
            definePicture(files[0]);
        }

        private void definePicture(String filePath)
        {
            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
            pic.TextEncoding = TagLib.StringType.Latin1;
            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
            pic.Type = TagLib.PictureType.FrontCover;
            pic.Data = TagLib.ByteVector.FromPath(filePath);

            binArtwork = (byte[])(pic.Data.Data);
            picBxArtwork.Image = Image.FromStream(new MemoryStream(binArtwork)).GetThumbnailImage(picBxArtwork.Height, picBxArtwork.Height, null, IntPtr.Zero);

            changeCoverFlag = true;
        }

        private void lblGenre_Click(object sender, EventArgs e)
        {
            txtGenre.Text = "EDM";
        }
    }
}