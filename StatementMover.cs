using System;
using System.Diagnostics;
using StatementFile;
using System.IO;
using System.Reflection;
using RegexManager;

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
    private RegexManager.RegexManager regexManager;
    private FlowLayoutPanel rulesPanel;
    public StatementMoverForm(){
        Text = "BOA Statement Mover v1.4";
        ClientSize = new Size(1500, 650); // Width, Height
        Icon = new Icon(LoadIco());

        regexManager = new RegexManager.RegexManager();

        TableLayoutPanel topLevel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 3,RowCount = 3,AutoSize = true};
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        topLevel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        topLevel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        topLevel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        Controls.Add(topLevel);

        TableLayoutPanel pathsPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 2,AutoSize = true};
        pathsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        pathsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        pathsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        pathsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        topLevel.Controls.Add(pathsPanel, 0, 0);

        Button selectInFolder = new Button {Text = "Select Input Folder", Dock = DockStyle.Fill};
        selectInFolder.Click += InFolderClick;
        Button selectOutFolder = new Button {Text = "Select Destination Folder", Dock = DockStyle.Fill};
        selectOutFolder.Click += InFolderClick;
        selectedInPathBox = new TextBox{Dock = DockStyle.Fill,Text = selectedInPathText, ReadOnly = true};
        selectedOutPathBox = new TextBox{Dock = DockStyle.Fill,Text = selectedOutPathText, ReadOnly = true};
        pathsPanel.Controls.Add(selectInFolder, 0, 0);
        pathsPanel.Controls.Add(selectOutFolder, 0, 1);
        pathsPanel.Controls.Add(selectedInPathBox, 1, 0);
        pathsPanel.Controls.Add(selectedOutPathBox, 1, 1);

        TableLayoutPanel exportPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 2,AutoSize = true};
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        topLevel.Controls.Add(exportPanel, 1, 0);

        Button runButton = new Button {Text = "Run", Dock = DockStyle.Fill};
        runButton.Click += RunProcess;
        Button openDestfolder = new Button {Text = "Open Destination Folder", Dock = DockStyle.Fill};
        openDestfolder.Click += OpenDestfolder;
        Button openFileLocation = new Button {Text = "Open Selected File Location", Dock = DockStyle.Fill};
        openFileLocation.Click += GoToFileLocation;
        exportPanel.Controls.Add(runButton, 0, 0); 
        exportPanel.Controls.Add(openDestfolder, 1, 0);
        exportPanel.Controls.Add(openFileLocation, 1, 1);

        TableLayoutPanel optionsPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        exportPanel.Controls.Add(optionsPanel, 0, 1);

        allowDupe = new CheckBox{Appearance = Appearance.Normal, Text = "Allow Duplicates", AutoSize = true, Checked = true};
        disableCopy = new CheckBox{Appearance = Appearance.Normal, Text = "Don't Move Files", AutoSize = true, Checked = false};
        optionsPanel.Controls.Add(allowDupe, 0, 0);
        optionsPanel.Controls.Add(disableCopy, 1, 0);

        fileLoadListBox = new ListBox{Dock = DockStyle.Fill};
        fileMoveListBox = new ListBox{Dock = DockStyle.Fill};
        fileLoadListBox.SelectedIndexChanged += LoadIndexChanged;
        fileMoveListBox.SelectedIndexChanged += MoveIndexChanged;

        fileReadStatusBox = new TextBox{Dock = DockStyle.Fill,Multiline = true,ScrollBars = ScrollBars.Both,ReadOnly = true};
        fileMoveStatusBox = new TextBox{Dock = DockStyle.Fill,Multiline = true,ScrollBars = ScrollBars.Both,ReadOnly = true};

        topLevel.Controls.Add(fileLoadListBox, 0, 1);   // col 0, row 1
        topLevel.Controls.Add(fileMoveListBox, 1, 1);
        topLevel.Controls.Add(fileReadStatusBox, 0, 2);
        topLevel.Controls.Add(fileMoveStatusBox, 1, 2);

        TableLayoutPanel regexPanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, AutoSize=true};
        regexPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        regexPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        topLevel.Controls.Add(regexPanel, 2, 0);

        TableLayoutPanel regexTopPanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize=true, Padding = new Padding(0), Margin = new Padding(0)};
        regexTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        regexTopPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        regexPanel.Controls.Add(regexTopPanel, 0, 0);
        
        TableLayoutPanel regexBottomPanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, AutoSize=true, Padding = new Padding(0), Margin = new Padding(0)};
        regexBottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        regexBottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        regexBottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        regexPanel.Controls.Add(regexBottomPanel, 0, 1);

        Button saveRegex = new Button{Dock = DockStyle.Fill, Text = "Save Rules"};
        Button reloadRegex = new Button{Dock = DockStyle.Fill, Text = "Reload Rules"};
        Button openLocation = new Button{Dock = DockStyle.Fill, Text = "Open Rules File Location"};
        Button uploadRegex = new Button{Dock = DockStyle.Fill, Text = "Upload Rules File"};
        Button addRule = new Button{Dock = DockStyle.Fill, Text = "Add Rule"};
        saveRegex.Click += SaveRegexClick;
        reloadRegex.Click += ReloadRegexClick;
        openLocation.Click += OpenLocationClick;
        uploadRegex.Click += UploadRegexClick;
        addRule.Click += addRuleClick;
        regexTopPanel.Controls.Add(saveRegex, 0, 0);
        regexTopPanel.Controls.Add(openLocation, 1, 0);
        regexBottomPanel.Controls.Add(reloadRegex, 0, 0);
        regexBottomPanel.Controls.Add(addRule, 1, 0);
        regexBottomPanel.Controls.Add(uploadRegex, 2, 0);

        rulesPanel = new FlowLayoutPanel{Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true};
        topLevel.Controls.Add(rulesPanel, 2, 1);
        topLevel.SetRowSpan(rulesPanel, 2);

        buildRulesPanel();
    }   

    private void SaveRegexClick(object? sender, EventArgs e){
        regexManager.SaveRegex();
    }
    private void ReloadRegexClick(object? sender, EventArgs e){
        regexManager.LoadRegex();
        buildRulesPanel();
    }
    private void OpenLocationClick(object? sender, EventArgs e){
        regexManager.OpenRegexLocation();
    }
    private void UploadRegexClick(object? sender, EventArgs e){
        regexManager.UploadRegex();
        regexManager.LoadRegex();
        buildRulesPanel();
    }

    private void addRuleClick(object? sender, EventArgs e){
        regexManager.AddRule("", "");           //add an empty rule
        addRulePanel(regexManager.ruleCount);   //create the panel for that rule
    }

    private void addRulePanel(int ruleNumber, string reg = "", string dir = ""){
        TableLayoutPanel rulePanel = new TableLayoutPanel{Name = $"panel|{ruleNumber}", Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, AutoSize=true};
        rulePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        rulePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        rulePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f));
        rulePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f));
        rulesPanel.Controls.Add(rulePanel);

        TextBox ruleRegBox = new TextBox{Dock = DockStyle.Fill, Text = reg, Name=$"regbox|{ruleNumber}"};
        TextBox ruleDirBox = new TextBox{Dock = DockStyle.Fill, Text = dir, Name=$"dirbox|{ruleNumber}"};
        Button applyButton = new Button{Dock = DockStyle.Fill, Name = $"apply|{ruleNumber}",Text = "Apply"};
        Button deleteButton = new Button{Dock = DockStyle.Fill, Name = $"delete|{ruleNumber}",Text = "❌"};
        applyButton.Click += applyRule;
        deleteButton.Click += removeRuleClick;
        rulePanel.Controls.Add(ruleRegBox, 0, 0);
        rulePanel.Controls.Add(ruleDirBox, 1, 0);
        rulePanel.Controls.Add(applyButton, 2, 0);
        rulePanel.Controls.Add(deleteButton, 3, 0);
    }

    private void applyRule(object? sender, EventArgs e){
        Button btn = (sender as Button)!;
        int ruleNum = Int32.Parse(btn.Name.Split("|")[1]);
        string ruleReg = rulesPanel.Controls.Find($"regbox|{ruleNum}", true)[0].Text ?? "";
        string ruleDir = rulesPanel.Controls.Find($"dirbox|{ruleNum}", true)[0].Text ?? "";
        regexManager.EditRule(ruleNum, ruleReg, ruleDir);
    }

    private void removeRuleClick(object? sender, EventArgs e){
        Button btn = (sender as Button)!;
        int ruleNum = Int32.Parse(btn.Name.Split("|")[1]);
        TableLayoutPanel? panel = rulesPanel.Controls.Find($"panel|{ruleNum}", true)[0] as TableLayoutPanel;
        rulesPanel.Controls.Remove(panel);

        // This is really dumb but I don't want to try anymore
        for(int i = ruleNum; i <= regexManager.ruleCount; i++){      // decrement the rule number in each panel
            foreach(Control c in rulesPanel.Controls){
                int panelNum = Int32.Parse(c.Name.Split("|").Last());
                if(panelNum == i){
                    c.Name = $"{c.Name.Split("|")[0]}|{panelNum - 1}";                  // rename the panel
                    foreach(Control panelc in c.Controls){
                        panelc.Name = $"{panelc.Name.Split("|")[0]}|{panelNum - 1}";    // rename its controls
                    }
                }
            }
        }
        regexManager.RemoveRule(ruleNum);
    }

    private void buildRulesPanel(){
        rulesPanel.Controls.Clear();
        for(int i = 0; i < regexManager.ruleCount; i++){
            addRulePanel(i+1, regexManager.ruleRegex[i], regexManager.ruleDirectory[i]);
        }
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
                Statement stmt = new Statement(regexManager, file);
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
        try{
            Application.EnableVisualStyles();
            Application.Run(new StatementMoverForm());
        } catch (Exception ex){
            MessageBox.Show(ex.Message);
        }
    }
}
}