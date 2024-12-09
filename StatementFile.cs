using System;
using System.IO;
using PdfiumViewer;
using System.Text.RegularExpressions;

namespace StatementFile{
    public class Statement{
        private static string nl = Environment.NewLine;
        public static readonly string regexPath = Path.Combine(AppContext.BaseDirectory, "regex.config");
        private static int numRegex = 0;
        private static List<string> regexes = [];
        private static List<string> generalDirs = [];
        private static List<string> boaBases = [];
        private const string fileOkIcon = "✓";
        private const string fileErrorIcon = "⚠️";
        private bool disableCopy;
        private bool fileReadOk = true;
        public bool fileMovedOk = true;
        private string fileReadDump = "";
        private string fileMoveDump = "";
        public readonly string filePath;
        public readonly string fileName;
        public readonly string fileDate;
        public readonly string accountNumber;
        public readonly string accountName;
        public string newFileDir = "";
        public string newFileName;
        public static string baseDestDir = "L:/BANK STMTS";

        public Statement(string file){
            LoadRegex();
            filePath = file;
            fileName = filePath.Split("\\").Last();
            fileReadDump += $"{nl}File Name: {fileName}";
            fileDate = GetDate(fileName);
            (accountNumber, accountName) = ProcessPdf(filePath);
            newFileName = $"{fileDate}_{accountNumber}_{accountName}";
            fileReadDump += $"{nl}New File Name: {newFileName}.pdf";
        }
        
        private void LoadRegex(){
            StreamReader reader = File.OpenText(regexPath);
            _ = reader.ReadLine();      // Toss header
            string? s;
            while ((s = reader.ReadLine()) != null)
            {   
                numRegex++;
                string regex = s.Split("!!!")[0];
                string boaDir = s.Split("!!!")[1];
                if (regex.StartsWith("<>")){    // general dir
                    regexes.Add(regex[2..]);
                    generalDirs.Add(regex[2..]);
                }else{
                    regexes.Add(regex);
                }
                boaBases.Add(boaDir);
            }
            reader.Close();
        }

        private string GetDate(string fileName){
            try{
            string datePattern = @"\d{4}-\d{2}-\d{2}";
            string date = Regex.Match(fileName, datePattern).Value.ToString();
            fileReadDump += $"{nl}Date: {date}";
            return date;
            } catch (Exception e){
                fileReadOk = false;
                fileReadDump += $"{nl}Error: " + e.Message;
                return "";
            }
        }

        private (string accNum, string accName) ProcessPdf(string filePath){
            try{
            using var document = PdfDocument.Load(filePath);
            int pageCount = document.PageCount;
            string text = document.GetPdfText(1);   // Info we need is always on page 2 (zero indexed)
            string info = text.Split("Page")[0];
            string accountName = info.Split("!")[0].Trim();
            string accountNumber = info.Split("!")[1].Trim()[^4..];
            fileReadDump += $"{nl}Account Name: {accountName}{nl}Account Number: {accountNumber}";
            return (accountNumber, accountName);
            } catch (Exception e){
                fileReadOk = false;
                fileReadDump += $"{nl}Error: " + e.Message;
                return ("", "");
            }
        }

        public void MoveFile(bool allowDupe, bool disableCopy){
            this.disableCopy = disableCopy;
            fileMoveDump = "";
            newFileDir = "";
            fileMovedOk = true;
            if (!fileReadOk){
                fileMoveDump += $"{nl}File read error! Skipping file";
                newFileDir = "???";
            }
            string outfileDir = FindOutfileDir();
            if (!fileMovedOk){
                fileMoveDump += $"{nl}File copy error! Skipping file";
                newFileDir = "???";
            }
            string fileDest = outfileDir + "/" + newFileName + ".pdf";
            if (File.Exists(fileDest)){
                if (allowDupe){
                    string fileDestNoPdf = fileDest[..^4]; 
                    string newFileDest;
                    int copyNum = 0;
                    do {
                        copyNum++;
                        newFileDest = $"{fileDestNoPdf}({copyNum}).pdf";
                    } while (File.Exists(newFileDest));
                    newFileName += $"({copyNum})";
                    fileMoveDump += $"{nl}File name already exists. File renamed";
                    fileMoveDump += $"{nl}New File Name: {newFileName}.pdf";
                    if (!disableCopy){

                        File.Copy(filePath, newFileDest);
                    } else {
                        fileMoveDump += $"{nl}File copying disabled by the user";
                    }
                } else {
                    fileMovedOk = false;
                    fileMoveDump += $"{nl}File name already exists. Duplicates not allowed";
                    newFileDir = outfileDir;
                }
            } else {
                fileMoveDump += $"{nl}New File Name: {newFileName}.pdf";
                if (!disableCopy){
                    File.Copy(filePath, fileDest);
                } else {
                    fileMoveDump += $"{nl}File copying disabled by the user";
                }
            }
            newFileDir = outfileDir;
        }

        private string FindOutfileDir(){
            string[] allDirs = Directory.GetDirectories($"{baseDestDir}/");
            bool dirExistsAsName = false;
            bool dirIsGeneral = false;
            foreach(string dir in allDirs){
                if(dir.Contains(accountName)){
                    dirExistsAsName = true;
                }
            }
            foreach(string dir in generalDirs){
                if(dir.Contains(accountName)){
                    dirIsGeneral = true;
                }
            }
            if (dirExistsAsName && !dirIsGeneral){
                fileMoveDump += $"{nl}Found Directory {accountName}/{nl}File Destination: {baseDestDir}/{accountName}/";
                return $"{baseDestDir}/{accountName}";
            } else {
                fileMoveDump += $"{nl}Directory {accountName} does not exist or is a general directory";
                for(int i = 0; i < numRegex; i++){
                    string r = @$"{regexes[i]}";
                    if (Regex.Match(accountName, r, RegexOptions.IgnoreCase).Success && Directory.Exists($"{baseDestDir}/{boaBases[i]}")){
                        return GetBOADir(boaBases[i]);
                    }
                }
                fileMovedOk = false;
                fileMoveDump += $"{nl}General Directory: No matching general directory found";
                return "";
            }
        }

        private string GetBOADir(string baseDir){
            string dirPath = $"{baseDestDir}/{baseDir}";
            string[] allDirs = Directory.GetDirectories(dirPath);
            foreach(string dir in allDirs){
                string boaDir = dir.Split("\\").Last(); 
                if (boaDir.Length < 8){continue;}
                string boaPart = boaDir[..8]; 
                if (boaPart == $"BOA {accountNumber}"){
                    fileMoveDump += $"{nl}General Directory: {baseDir}/{nl}BOA Folder: {boaDir}/{nl}File Destination: {dirPath}/{boaDir}/";
                    return $"{dirPath}/{boaDir}";
                }
            }
            fileMoveDump += $"General Directory: {baseDir}/{nl}BOA Folder: Could not find a BOA {accountNumber} folder!{nl}File Destination: {dirPath}/";
            return dirPath;
        }

        public string GetInitialFileDisplayText(){
            if (fileReadOk){
                return $"{fileOkIcon}{fileName}";
            }else{
                return $"{fileErrorIcon}{fileName}";
            }
        }

        public string GetMovedFileDisplayText(){
            if (fileMovedOk){
                return $"{fileOkIcon}{newFileDir}/{newFileName}.pdf";
            }else{
                return $"{fileErrorIcon}{newFileDir}/{newFileName}.pdf";
            }
        }

        public string GetFileReadStatusText(){
            string success = fileReadOk ? "File Read Successfully!" : "File Read Failed!";
            return $"File Read Log{fileReadDump}{nl}{nl}{success}";
        }
        public string GetFileMoveStatusText(){
            string success = fileMovedOk ? "File Copied Successfully!" : "File Copy Failed!";
            success = disableCopy && fileMovedOk ? "No Issues Found!" : success;
            return $"File Move Log{fileMoveDump}{nl}{nl}{success}";
        }
    }
}