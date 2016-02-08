import c4d
from c4d import gui
import subprocess
import os.path

def main():
    path = doc.GetDocumentPath()+'\\'+doc.GetDocumentName()
    base = os.path.dirname(os.path.abspath(__file__))
    c4dbb = os.path.normpath(os.path.join(base, './C4D_bb.exe'))

    if doc.GetDocumentPath()=='':
        dlg = okDialog()
        dlg.Open(c4d.DLG_TYPE_MODAL)
        return True

    os.path.exists(path)

    rd = doc.GetActiveRenderData()
    fps = doc.GetFps()
    ff = str((rd[c4d.RDATA_FRAMEFROM]).GetFrame(fps))
    ft = str((rd[c4d.RDATA_FRAMETO]).GetFrame(fps))

    fstep = rd[c4d.RDATA_FRAMESTEP]
    if fstep>1:
       ft = str((rd[c4d.RDATA_FRAMETO]).GetFrame(fps))+'_'+str((rd[c4d.RDATA_FRAMESTEP]))

    mp = '0'
    if rd[c4d.RDATA_MULTIPASS_ENABLE]==True:
       mp = '1'

    cmd = c4dbb + ' '+path+ ' '+ff+ ' '+ft+' '+mp
    subprocess.call(cmd, shell=False)

class okDialog(gui.GeDialog):
    def CreateLayout(self):
        self.SetTitle('C4D Backburner')
        self.AddStaticText(1000, c4d.BFH_SCALEFIT,300, 10, 'プロジェクトが保存されていません')
        self.AddButton(20001, c4d.BFH_SCALE, name='OK')
        return True

    def Command(self, id, msg):
        if id==20001:
          self.Close()
        return True

if __name__=='__main__':
    main()
    c4d.EventAdd()
