namespace RegexManager{
    public class RegexManager{
        public static readonly string regexPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StatementMoverRegex.config");
        public List<string> ruleRegex = [];
        public List<string> ruleDirectory = [];
        public int ruleCount = 0;
        public RegexManager(){
            LoadRegex();
        }

        public bool LoadRegex(){
            if(!File.Exists(regexPath)){SaveRegex();}   // creates the regex file if it doesnt exist
            FileStream fs = new FileStream(regexPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            try{
                string line;
                while ((line = reader.ReadLine()!) != null){
                    string regex = line.Split("-->")[0];
                    string dir = line.Split("-->")[1];
                    ruleRegex.Append(regex);
                    ruleDirectory.Append(dir);
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
            FileStream fs = new FileStream(regexPath, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            try{
                for(int i = 0; i < ruleCount; i++){
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

        public bool UploadRegex(string filePath){
            try {
                using var fileDialog = new OpenFileDialog();
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                DialogResult result = fileDialog.ShowDialog();
                if (result == DialogResult.OK){
                    MessageBox.Show(fileDialog.FileName);
                    //File.Copy(fileDialog.FileName, regexPath);
                    return true;
                }else{
                    return false;
                }
            } catch (Exception ex){
                MessageBox.Show(ex.Message, "Regex Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}