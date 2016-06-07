import c4d
from c4d import gui
import subprocess
import os.path
import json

path = "";
jobname = "";
ff = 0;
ft = 0;
priority = "50";
fstr = "F:/render/";
rstr = "\\\\USER/render/";
#smb://

server_ip = "192.168.1.5";
server_group = "RENDER";
server_count = 5;
server_c4dloc = "C:\\Program Files\\MAXON\\CINEMA 4D R17\\CINEMA 4D.exe";
cmdjob = "C:\\Program Files (x86)\\Autodesk\\Backburner\\cmdjob.exe";
#/usr/discreet/backburner/cmdjob

def Execute():
    global path, jobname, ff, ft, priority, fstr, rstr, server_ip, server_group, server_count, server_c4dloc, cmdjob;
    rd = doc.GetActiveRenderData();
    #saveDir = doc.GetDocumentPath().replace(fstr, rstr);
    fstr = fstr.replace("/", os.sep).replace("\\", os.sep);
    rstr = rstr.replace("/", os.sep).replace("\\", os.sep);

    saveDir = rd[c4d.RDATA_PATH].replace(fstr, rstr);
    #if not os.path.exists(doc.GetDocumentPath()+os.sep+"results"):
        #os.mkdir(doc.GetDocumentPath()+os.sep+"results");

    saveDir = saveDir.replace("/", os.sep).replace("\\", os.sep);

    # tasklist生成
    if ft>=server_count:
        pt = (ft - ff) / server_count;
        pt_ce = int(round(pt, 0)) - 1;
        pt_tmp = ff;
        tasklist = "";
        for i in range(0, server_count-1):
            tasklist += jobname + "_" + str(pt_tmp) + "-" + str((pt_tmp + pt_ce)) + "\t" + str(pt_tmp) + "\t" + str((pt_tmp + pt_ce)) + "\n";
            pt_tmp += pt_ce + 1;

        tasklist += jobname + "_" + str(pt_tmp) + "-" + str(ft) + "\t" + str(pt_tmp) + "\t" + str(ft);
    else:
        server_count = ft;
        pt_tmp = ff;

        tasklist = "";
        for i in range(0, server_count+1):
            tasklist += jobname + "_" + str(pt_tmp) + "\t" + str(pt_tmp) + "\t" + str(pt_tmp) + "\n";
            pt_tmp += 1;
    f = open(doc.GetDocumentPath()+os.sep+"tasklist.txt","w");
    f.write(tasklist);
    f.close();
    tasklistpath = doc.GetDocumentPath() + "/";

    # 引数
    cmd = cmdjob + " -jobname " + jobname + " -manager "+server_ip+" -priority " + priority + " -serverCount "+str(server_count)+" -group "+server_group +" -taskList \"" + tasklistpath.replace("/", os.sep).replace("\\", os.sep).replace(fstr, rstr) + "tasklist.txt\" -taskname 1 -timeout 4320 ";

    if(rd[c4d.RDATA_FRAMESTEP]>1):
        cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path.replace(fstr, rstr) + "\" -frame %tp2 %tp3 " + rd[c4d.RDATA_FRAMESTEP] + "-oimage \"" + saveDir + "\"";
    else:
        cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path.replace(fstr, rstr) + "\" -frame %tp2 %tp3 -oimage \"" + saveDir + "\"";

    if(rd[c4d.RDATA_MULTIPASS_ENABLE]):
        cmd += " -omultipass \"" + rd[c4d.RDATA_MULTIPASS_FILENAME].replace("/", os.sep).replace("\\", os.sep).replace(fstr, rstr) + "\""

    if(takename != ""):
        cmd += " -take \"" + takename + "\"";

    print "[C4D_BB] "+cmd;
    subprocess.call(cmd, shell=False);
    return

class settings(gui.GeDialog):
    res = False;
    def CreateLayout(self):
        self.SetTitle("C4D Backburner");

        self.GroupBegin(10000, c4d.BFH_LEFT, 2);
        self.AddStaticText(1000, c4d.BFH_LEFT, 0, 0, 'Project path', c4d.BORDER_NONE);
        self.AddEditText(1001, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1002, c4d.BFH_LEFT, 0, 0, 'Job Name', c4d.BORDER_NONE);
        self.AddEditText(1003, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1004, c4d.BFH_LEFT, 0, 0, 'Start Frame', c4d.BORDER_NONE);
        self.AddEditNumber(1005, c4d.BFH_LEFT, 200, 0);
        self.AddStaticText(1006, c4d.BFH_LEFT, 0, 0, 'End Frame', c4d.BORDER_NONE);
        self.AddEditNumber(1007, c4d.BFH_LEFT, 200, 0);
        self.AddStaticText(1008, c4d.BFH_LEFT, 0, 0, 'Take name', c4d.BORDER_NONE);
        self.AddEditText(1009, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1010, c4d.BFH_LEFT, 0, 0, 'Priority', c4d.BORDER_NONE);
        self.AddEditNumber(1011, c4d.BFH_LEFT, 200, 0);
        self.AddStaticText(1012, c4d.BFH_LEFT, 0, 0, 'Path Replace(from)', c4d.BORDER_NONE);
        self.AddEditText(1013, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1014, c4d.BFH_LEFT, 0, 0, 'Path Replace(to)', c4d.BORDER_NONE);
        self.AddEditText(1015, c4d.BFH_LEFT, 500, 0);

        self.AddStaticText(2000, c4d.BFH_LEFT, 0, 0, '', c4d.BORDER_NONE);
        self.AddStaticText(2000, c4d.BFH_LEFT, 0, 0, '', c4d.BORDER_NONE);

        self.AddStaticText(1016, c4d.BFH_LEFT, 0, 0, 'Manager IP', c4d.BORDER_NONE);
        self.AddEditText(1017, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1018, c4d.BFH_LEFT, 0, 0, 'Server Group', c4d.BORDER_NONE);
        self.AddEditText(1019, c4d.BFH_LEFT, 500, 0);
        self.AddStaticText(1020, c4d.BFH_LEFT, 0, 0, 'Server Counts', c4d.BORDER_NONE);
        self.AddEditNumberArrows(1021, c4d.BFH_LEFT, 200, 0);
        self.AddStaticText(1022, c4d.BFH_LEFT, 0, 0, 'Server C4D Path', c4d.BORDER_NONE);
        self.AddEditText(1023, c4d.BFH_LEFT, 500, 0);

        self.AddStaticText(2000, c4d.BFH_LEFT, 0, 0, '', c4d.BORDER_NONE);
        self.AddStaticText(2000, c4d.BFH_LEFT, 0, 0, '', c4d.BORDER_NONE);

        self.AddStaticText(1024, c4d.BFH_LEFT, 0, 0, 'Cmdjob Path', c4d.BORDER_NONE);
        self.AddEditText(1025, c4d.BFH_LEFT, 500, 0);
        self.GroupEnd();
        self.AddDlgGroup(c4d.DLG_OK|c4d.DLG_CANCEL);
        return False;

    def AskClose(self):
        global path, jobname, ff, ft, takename, priority, fstr, rstr, server_ip, server_group, server_count, server_c4dloc, cmdjob;
        path = self.GetString(1001).replace("/", os.sep).replace("\\", os.sep);
        jobname = self.GetString(1003);
        ff = self.GetLong(1005);
        ft = self.GetLong(1007);
        takename = self.GetString(1009);
        priority =self.GetString(1011);
        fstr = self.GetString(1013).replace("/", os.sep).replace("\\", os.sep);
        rstr = self.GetString(1015).replace("/", os.sep).replace("\\", os.sep);
        server_ip = self.GetString(1017);
        server_group = self.GetString(1019);
        server_count = self.GetLong(1021);
        server_c4dloc = self.GetString(1023);
        cmdjob = self.GetString(1025).replace("/", os.sep).replace("\\", os.sep);

        d = {
            "fstr": fstr,
            "rstr": rstr,
            "server_ip": server_ip,
            "server_group": server_group,
            "server_count": server_count,
            "server_c4dloc": server_c4dloc,
            "cmdjob": cmdjob
        }

        jf = open(os.getenv("HOMEDRIVE") + os.getenv("HOMEPATH") + os.sep + 'c4dbb.json', 'w');
        json.dump(d, jf)
        jf.close();

        return False;

    def InitValues(self):
        self.SetString(1001, path);
        self.SetString(1003, jobname);
        self.SetLong(1005, ff);
        self.SetLong(1007, ft);
        self.SetString(1011, "50");
        self.SetString(1013, fstr.replace("/", os.sep).replace("\\", os.sep));
        self.SetString(1015, rstr.replace("/", os.sep).replace("\\", os.sep));

        self.SetString(1017, server_ip);
        self.SetString(1019, server_group);
        self.SetLong(1021, server_count);
        self.SetString(1023, server_c4dloc);
        self.SetString(1025, cmdjob.replace("/", os.sep).replace("\\", os.sep));

        return True;

    def Command(self, id, msg):
        if id == 1:
            self.res = True;
        if id == 1 or id == 2:
            self.Close();
        return True;

class errordialog(gui.GeDialog):
    def CreateLayout(self):
        self.SetTitle('C4D Backburner');
        self.AddStaticText(1026, c4d.BFH_SCALEFIT,300, 10, 'プロジェクトが保存されていません');
        self.AddButton(20001, c4d.BFH_SCALE, name='OK');
        return True;

    def Command(self, id, msg):
        if id==20001:
          self.Close();
        return True;

def main():
    global path, jobname, ff, ft, fstr, rstr, server_ip, server_group, server_count, server_c4dloc, cmdjob;
    path = doc.GetDocumentPath()+'/'+doc.GetDocumentName();
    if doc.GetDocumentPath()=='':
        dlg = errordialog();
        dlg.Open(c4d.DLG_TYPE_MODAL);
        return True;

    if os.path.exists(os.getenv("HOMEDRIVE") + os.getenv("HOMEPATH") + os.sep + 'c4dbb.json'):
        f = open(os.getenv("HOMEDRIVE") + os.getenv("HOMEPATH") + os.sep + 'c4dbb.json', 'r');
        s = json.load(f);
        f.close();

        fstr = s["fstr"];
        rstr = s["rstr"];
        server_ip = s["server_ip"];
        server_group = s["server_group"];
        server_count = s["server_count"];
        server_c4dloc = s["server_c4dloc"];
        cmdjob = s["cmdjob"];

    path = path.replace("/", os.sep).replace("\\", os.sep);

    jobname = doc.GetDocumentName().replace(".c4d", "");
    rd = doc.GetActiveRenderData();
    fps = doc.GetFps();
    ff = (rd[c4d.RDATA_FRAMEFROM]).GetFrame(fps);
    ft = (rd[c4d.RDATA_FRAMETO]).GetFrame(fps);

    dlg = settings();
    dlg.Open(c4d.DLG_TYPE_MODAL);
    if dlg.res:
        c4d.StopAllThreads();
        Execute();

if __name__=='__main__':
    main();
    c4d.EventAdd();
