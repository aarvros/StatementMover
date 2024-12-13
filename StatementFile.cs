using System;
using System.IO;
using PdfiumViewer;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace StatementFile{
    public class Statement{
        public RegexManager.RegexManager regexManager;
        private static string nl = Environment.NewLine;
        public static readonly string regexPath = Path.Combine(AppContext.BaseDirectory, "regex.txt");
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

        public Statement(RegexManager.RegexManager r, string file){
            regexManager = r;
            filePath = file;
            fileName = filePath.Split("\\").Last();
            fileReadDump += $"{nl}File Name: {fileName}";
            fileDate = GetDate(fileName);
            (accountNumber, accountName) = ProcessPdf(filePath);
            newFileName = $"{fileDate}_{accountNumber}_{accountName}";
            fileReadDump += $"{nl}New File Name: {newFileName}.pdf";
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
            if (!fileReadOk){
                fileMovedOk = false;
                fileMoveDump += $"{nl}File read error! Skipping file";
                newFileDir = "???";
                return;
            }
            string outfileDir = FindOutfileDir();
            if (!fileMovedOk){
                fileMoveDump += $"{nl}File copy error! Skipping file";
                newFileDir = "???";
                return;
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

        private string FindBOADir(string dirName){
            string dirPath = $"{baseDestDir}/{dirName}";
            string[] allDirs = Directory.GetDirectories(dirPath);
            List<string> dirs = [];
            List<string> accNums = [];
            foreach(string dir in allDirs){
                string testDirName = dir.Split("\\").Last(); 
                Match match = Regex.Match(testDirName, @"BOA\s\d{4}.*", RegexOptions.IgnoreCase);
                if(match.Success){
                    Match numMatch = Regex.Match(testDirName, @"\d{4}", RegexOptions.IgnoreCase);
                    dirs.Add(testDirName);
                    accNums.Add(numMatch.Value.ToString());
                }
            }
            int idx = accNums.IndexOf(accountNumber);
            if(idx != -1){
                return dirs[idx];
            }else{
                return "";
            }

        }
        
        private string FindOutfileDirFromBase(string accountDir){
            string boaDir = FindBOADir(accountDir);
                if(boaDir == ""){   // if accountname dir has no boa folder, drop it there
                    fileMoveDump += $"{nl}Could Not Find BOA Directory!";
                    fileMoveDump += $"{nl}Found Directory {accountName}/{nl}File Destination: {baseDestDir}/{accountDir}/";
                    return $"{baseDestDir}/{accountDir}";
                }else{              // if the accountname dir does have a boa folder
                    fileMoveDump += $"{nl}Found Directory {accountName}/{boaDir}/{nl}File Destination: {baseDestDir}/{accountDir}/{boaDir}/";
                    return $"{baseDestDir}/{accountDir}/{boaDir}";
                }
        }

        private string FindOutfileDir(){
            string[] allDirs = Directory.GetDirectories($"{baseDestDir}/");
            bool dirExistsAsName = false;
            foreach(string dir in allDirs){
                if(dir.Contains(accountName)){
                    dirExistsAsName = true;
                }
            }

            if(dirExistsAsName){    //if dir matches account name
                fileMovedOk = true;
                return FindOutfileDirFromBase(accountName);
            }else{                  // if no account name dir is found
                for(int i = 0; i < regexManager.ruleCount; i++){
                    Match match = Regex.Match(accountName, regexManager.ruleRegex[i], RegexOptions.IgnoreCase);
                    if (match.Success){     //if the regex rule matches the accountname
                        string outBaseDir = regexManager.ruleDirectory[i];
                        fileMovedOk = true;
                        return FindOutfileDirFromBase(outBaseDir);
                    }
                }
                fileMovedOk = false;
                fileMoveDump += $"{nl}No suitable file destination found!";
                return "";
            }
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