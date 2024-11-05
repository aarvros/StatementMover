using System;
using System.Diagnostics;
using StatementFile;
using System.IO;
using System.Reflection;

namespace StatementWinForms{
public class StatementMoverForm : Form{
    private TextBox selectedInPathBox;
    private string selectedInPathText = "";
    private TextBox selectedOutPathBox;
    private string selectedOutPathText = "L:\\BANK STMTS";
    private ListBox fileLoadListBox;
    private ListBox fileMoveListBox;
    private TextBox fileReadStatusBox;
    private TextBox fileMoveStatusBox;
    private CheckBox allowDupe; 
    private CheckBox disableCopy; 
    private List<Statement> stmts = [];
    public StatementMoverForm(){
        Text = "BOA Statement Mover";
        ClientSize = new System.Drawing.Size(1100, 600); // Width, Height
        Icon = new Icon(LoadIco());

        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 3,AutoSize = true};
        TableLayoutPanel importPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 2,AutoSize = true};
        TableLayoutPanel exportPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 2,AutoSize = true};
        TableLayoutPanel optionsPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};

        Button selectInFolder = new Button {Text = "Select Input Folder", Dock = DockStyle.Fill};
        selectInFolder.Click += InFolderClick;
        Button selectOutFolder = new Button {Text = "Select Destination Folder", Dock = DockStyle.Fill};
        selectOutFolder.Click += InFolderClick;
        
        Button runButton = new Button {Text = "Run", Dock = DockStyle.Fill};
        runButton.Click += RunProcess;
        //Controls.Add(runButton);

        Button openDestfolder = new Button {Text = "Open Destination Folder", Dock = DockStyle.Fill};
        openDestfolder.Click += OpenDestfolder;
        //Controls.Add(openDestfolder);

        Button openFileLocation = new Button {Text = "Open Selected File Location", Dock = DockStyle.Fill};
        openFileLocation.Click += GoToFileLocation;
        //Controls.Add(openFileLocation);

        allowDupe = new CheckBox{Appearance = Appearance.Normal, Text = "Allow Duplicates", AutoSize = true, Checked = true};
        disableCopy = new CheckBox{Appearance = Appearance.Normal, Text = "Don't Move Files", AutoSize = true, Checked = false};

        fileLoadListBox = new ListBox{Dock = DockStyle.Fill};
        //Controls.Add(fileLoadListBox);
        fileMoveListBox = new ListBox{Dock = DockStyle.Fill};
        //Controls.Add(fileLoadListBox);
        fileLoadListBox.SelectedIndexChanged += LoadIndexChanged;
        fileMoveListBox.SelectedIndexChanged += MoveIndexChanged;

        fileReadStatusBox = new System.Windows.Forms.TextBox{
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = System.Windows.Forms.ScrollBars.Both,
            ReadOnly = true
        };
        fileMoveStatusBox = new System.Windows.Forms.TextBox{
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = System.Windows.Forms.ScrollBars.Both,
            ReadOnly = true
        };
        selectedInPathBox = new System.Windows.Forms.TextBox{Dock = DockStyle.Fill,Text = selectedInPathText, ReadOnly = true};
        selectedOutPathBox = new System.Windows.Forms.TextBox{Dock = DockStyle.Fill,Text = selectedOutPathText, ReadOnly = true};

        // Designate row size ratios
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));

        // Designate column size ratios
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        importPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        importPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        importPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        importPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        exportPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        // Add controls
        tableLayoutPanel.Controls.Add(fileLoadListBox, 0, 1);
        tableLayoutPanel.Controls.Add(fileMoveListBox, 1, 1);
        tableLayoutPanel.Controls.Add(fileReadStatusBox, 0, 2);
        tableLayoutPanel.Controls.Add(fileMoveStatusBox, 1, 2);

        tableLayoutPanel.Controls.Add(importPanel, 0, 0);   // options panel in col 0, row 0 of the root pane
        tableLayoutPanel.Controls.Add(exportPanel, 1, 0);   // options panel in col 0, row 0 of the root pane
        importPanel.Controls.Add(selectInFolder, 0, 0);
        importPanel.Controls.Add(selectOutFolder, 0, 1);
        importPanel.Controls.Add(selectedInPathBox, 1, 0);
        importPanel.Controls.Add(selectedOutPathBox, 1, 1);

        exportPanel.Controls.Add(runButton, 0, 0); 
        exportPanel.Controls.Add(optionsPanel, 0, 1);
        exportPanel.Controls.Add(openDestfolder, 1, 0);
        exportPanel.Controls.Add(openFileLocation, 1, 1);

        optionsPanel.Controls.Add(allowDupe, 0, 0);
        optionsPanel.Controls.Add(disableCopy, 1, 0);

        // Add TableLayoutPanel to the form
        this.Controls.Add(tableLayoutPanel);
    }

    private void LoadIndexChanged(object? sender, EventArgs? e){
        int selectedIndex = fileLoadListBox.SelectedIndex;
        try{fileMoveListBox.SelectedIndex = selectedIndex;}catch (Exception){}  // do nothing if it fails, since it hasnt been run yet
        if (selectedIndex != -1){
            fileReadStatusBox.Text = stmts[selectedIndex].GetFileReadStatusText();
        }else{
            fileReadStatusBox.Text = "Select a file to view its status";
        }
    }
    private void MoveIndexChanged(object? sender, EventArgs? e){
        int selectedIndex = fileMoveListBox.SelectedIndex;
        fileLoadListBox.SelectedIndex = selectedIndex;
        if (selectedIndex != -1){
            fileMoveStatusBox.Text = stmts[selectedIndex].GetFileMoveStatusText();
        }else{
            fileMoveStatusBox.Text = "Select a file to view its status";
        }
    }

    private void InFolderClick(object? sender, EventArgs? e){
        using var folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Select a folder";
        folderDialog.ShowNewFolderButton = true;
        folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        DialogResult result = folderDialog.ShowDialog();
        if (result == DialogResult.OK){
            Button? btn = sender as Button;
            if (btn!.Text.Split(" ")[1] == "Input"){
                selectedInPathText = folderDialog.SelectedPath;
                selectedInPathBox.Text = selectedInPathText;
                PopulateFileList(folderDialog.SelectedPath);
            } else if (btn!.Text.Split(" ")[1] == "Destination"){
                selectedOutPathText = folderDialog.SelectedPath;
                selectedOutPathBox.Text = selectedOutPathText;
                Statement.baseDestDir = selectedOutPathText.Replace("\\", "/");
            }
        }
    }

    private void RunProcess(object? sender, EventArgs? e){
        if (selectedInPathText != ""){
            fileMoveListBox.Items.Clear();
            foreach(Statement stmt in stmts){
                stmt.MoveFile(allowDupe.Checked, disableCopy.Checked);
                fileMoveListBox.Items.Add(stmt.GetMovedFileDisplayText());
            }
            fileMoveStatusBox.Text = "Select a file to view its status";
            fileMoveListBox.SelectedIndex = fileLoadListBox.SelectedIndex;
        }
    }

    private void GoToFileLocation(object? sender, EventArgs? e){
        int selectedIndex = fileMoveListBox.SelectedIndex;
        if (selectedIndex != -1 && stmts[selectedIndex].fileMovedOk){
            string dir = stmts[selectedIndex].newFileDir;
            dir = dir.Replace("/", "\\") + "\\";
            Process.Start("explorer.exe", dir);
        }
    }
    private void OpenDestfolder(object? sender, EventArgs? e){
        string dir = selectedOutPathText.Replace("/", "\\");
        Process.Start("explorer.exe", dir);
    }

    private void PopulateFileList(string folderPath){
            stmts = [];
            fileLoadListBox.Items.Clear();
            fileMoveListBox.Items.Clear();
            string [] fileEntries = Directory.GetFiles(folderPath, "*.pdf");
            foreach (var file in fileEntries)
            {
                Statement stmt = new Statement(file);
                stmts.Add(stmt);
                fileLoadListBox.Items.Add(stmt.GetInitialFileDisplayText());
            }
            fileReadStatusBox.Text = "Select a file to view its status";
        }

    private static Stream LoadIco(){
        string resourceName = "StatementMover.resources.lambicon.ico";
        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream? stream = assembly.GetManifestResourceStream(resourceName);
        return stream!;                                                     
    }

    [STAThread]
    public static void Main(string[] args){
        Application.EnableVisualStyles();
        Application.Run(new StatementMoverForm());
    }
}
}