using System.Diagnostics;

namespace RegexManager{
    public class RegexManager{
        public static readonly string regexDirPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\StatementMover\\";
        public static readonly string regexPath = $"{regexDirPath}\\StatementMoverRegex.txt";
        public List<string> ruleRegex = [];
        public List<string> ruleDirectory = [];
        public List<string> ruleGeneralDir = [];
        public int ruleCount = 0;
        public RegexManager(){
            LoadRegex();
        }

        public bool OpenRegexLocation(){
            if(Path.Exists(regexDirPath)){
                Process.Start("explorer.exe", regexDirPath);
                return true;
            }else{
                MessageBox.Show($"Folder {regexDirPath} does not exist!", "Regex Does Not Exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool LoadRegex(){
            if(!File.Exists(regexPath)){SaveRegex();}   // creates the regex file if it doesnt exist
            FileStream fs = new FileStream(regexPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            try{
                ruleRegex = [];
                ruleDirectory = [];
                ruleGeneralDir = [];
                ruleCount = 0;
                string line;
                while ((line = reader.ReadLine()!) != null){
                    string regex = line.Split("-->")[0];
                    string dir = line.Split("-->")[1];
                    ruleRegex.Add(regex);
                    ruleDirectory.Add(dir);
                    ruleCount++;
                }
                reader.Close();
                fs.Close();
                return true;
            } catch (Exception ex){
                reader.Close();
                fs.Close();
                MessageBox.Show(ex.Message, "Regex Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;   
            }
        }

        public bool SaveRegex(){
            if (!Directory.Exists("StatementMover")){Directory.CreateDirectory(regexDirPath);}  //create the dir if it doesnt exist
            FileStream fs = new FileStream(regexPath, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            try{
                for(int i = 0; i < ruleCount; i++){
                    if(ruleRegex[i] == "" && ruleDirectory[i] == ""){continue;} // dont save empty rules
                    string line = ruleRegex[i] + "-->" + ruleDirectory[i];
                    writer.WriteLine(line);
                }
                writer.Close();
                fs.Close();
                return true;
            } catch (Exception ex){
                writer.Close();
                fs.Close();
                MessageBox.Show(ex.Message, "Regex Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;   
            }
        }

        public bool UploadRegex(){
            try {
                using var fileDialog = new OpenFileDialog{Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"};
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                DialogResult result = fileDialog.ShowDialog();
                if (result == DialogResult.OK){
                    File.Copy(fileDialog.FileName, regexPath, overwrite: true);
                    return true;
                }else{
                    return false;
                }
            } catch (Exception ex){
                MessageBox.Show(ex.Message, "Regex Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void EditRule(int ruleNum, string ruleReg, string ruleDir){
            try{
                ruleRegex[ruleNum-1] = ruleReg;
                ruleDirectory[ruleNum-1] = ruleDir;
            } catch (Exception ex){
                MessageBox.Show($"You went out of bounds editing a rule!\n\nRule Count: {ruleCount}\nIndex: {ruleNum-1}\n\n{ex.Message}", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddRule(string ruleReg, string ruleDir){
            ruleRegex.Add(ruleReg);
            ruleDirectory.Add(ruleDir);
            ruleCount++;
        }

        public void RemoveRule(int ruleNum){
            try{
                int index = ruleNum - 1;
                ruleRegex.RemoveAt(index);
                ruleDirectory.RemoveAt(index);
                ruleCount--;
            } catch (Exception ex){
                MessageBox.Show($"You went out of bounds removing a rule!\n\nRule Count: {ruleCount}\nIndex: {ruleNum-1}\n\n{ex.Message}", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}